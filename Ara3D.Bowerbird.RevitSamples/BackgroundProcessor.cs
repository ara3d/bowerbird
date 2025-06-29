using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Events;
using System.Linq;
using System.Threading;
using Ara3D.Utils;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace Ara3D.Bowerbird.RevitSamples
{
    /// <summary>
    /// This allows you to execute arbitrary Revit code within a valid UI context. 
    /// </summary>
    public static class ApiContext
    {
        [DllImport("USER32.DLL")]
        public static extern bool PostMessage(IntPtr hWnd, uint msg, uint wParam, uint lParam);

        public class ExternalEventHandler : IExternalEventHandler
        {
            public readonly Action<UIApplication> Action;
            public readonly string Name;

            public ExternalEventHandler(Action<UIApplication> action, string name)
            {
                Action = action ?? throw new ArgumentNullException(nameof(action));
                Name = name;
            }

            public void Execute(UIApplication app)
            {
                try
                {
                    Action(app);
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Error", ex.Message);
                }
            }

            public string GetName()
                => Name;
        }

        public static ExternalEvent CreateEvent(Action<UIApplication> action, string name)
        {
            var eeh = new ExternalEventHandler(action, name);
            return ExternalEvent.Create(eeh);
        }
    }

    /// <summary>
    /// The background processor does arbitrary work in the background while Revit is Idle.
    /// It contains a queue of work items (of any user defined type) for later processing.
    /// Good practice for choosing a work item type is to not use a Revit API type (e.g., use an integer for element ID).
    /// Work processing is performed by a delegate / action provided to the constructors. 
    /// When an OnIdle or DocumentChanged event occurs, work will be extracted out of the queue
    /// and executed using the provided action.
    /// This will be repeated until the maximum MSec per batch is reached. 
    /// You can pause processing, execute all items immediately, or do other work. 
    /// </summary>
    public class BackgroundProcessor<T> : IDisposable
    {
        public ConcurrentQueue<T> Queue { get; private set; } = new ConcurrentQueue<T>();
        public int MaxMSecPerBatch { get; set; } = 100;
        public int HeartBeatMsec { get; set; } = 125;
        public bool ExecuteNextIdleImmediately { get; set; }
        public readonly Action<T> Processor;
        public readonly UIApplication UIApp;
        public Application App => UIApp.Application;
        public bool PauseProcessing { get; set; }
        public event EventHandler<Exception> ExceptionEvent;
        public readonly Stopwatch WorkStopwatch = new Stopwatch();
        public int WorkProcessedCount = 0;
        private bool _enabled = false;
        public bool DoWorkDuringIdle { get; set; } = true;
        public bool DoWorkDuringProgress { get; set; } = true;
        public bool DoWorkDuringExternal { get; set; } = true;
        public static Process Revit = Process.GetCurrentProcess();
        public Thread PokeRevitThread; 

        public bool Enabled
        {
            get => _enabled;
            set
            {

                if (value)
                    Attach();
                else
                    Detach();
            }
        }

        public BackgroundProcessor(Action<T> processor, UIApplication uiApp)
        {
            Processor = processor;
            UIApp = uiApp;
            Attach();
            //var ee = ApiContext.CreateEvent(On_ExternalEventHeartbeat, "Heartbeat");
            PokeRevitThread = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(HeartBeatMsec);
                    //ee.Raise();
                    PokeRevit();
                }
            });
        }

        public void Detach()
        {
            if (!_enabled)
                return;
            App.ProgressChanged -= App_ProgressChanged;
            UIApp.Idling -= UiApp_Idling;
            _enabled = true;
        }

        public void Attach()
        {
            if (_enabled)
                return;
            App.ProgressChanged += App_ProgressChanged;
            UIApp.Idling += UiApp_Idling;
            _enabled = true;
        }

        private void App_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (!DoWorkDuringProgress || PauseProcessing)
                return;
            ProcessWork();
        }

        public void Dispose()
        {
            ExceptionEvent = null;
            PokeRevitThread.Abort();
            Detach();
        }

        public void On_ExternalEventHeartbeat(UIApplication _)
        {
            if (!DoWorkDuringExternal || PauseProcessing)
                return;
            ProcessWork();
        }

        private void UiApp_Idling(object sender, Autodesk.Revit.UI.Events.IdlingEventArgs e)
        {
            if (!DoWorkDuringIdle || PauseProcessing || !HasWork)
                return;
            ProcessWork();
            if (ExecuteNextIdleImmediately && HasWork)
                e.SetRaiseWithoutDelay();
        }

        /// <summary>
        /// Note that this can be called outside of the Revit API context. 
        /// </summary>
        public void EnqueueWork(IEnumerable<T> items)
        {
            foreach (var item in items)
                EnqueueWork(item);
        }

        public void EnqueueWork(T item)
        {
            Queue.Enqueue(item);
        }

        public bool HasWork
            => Queue.Count > 0;

        public void ClearWork()
            => Queue = new ConcurrentQueue<T>();

        public void ResetStats()
        {
            WorkStopwatch.Reset();
            WorkProcessedCount = 0;
        }

        public void ProcessWork(bool doAllNow = false)
        {
            var startedTime = WorkStopwatch.ElapsedMilliseconds;
            WorkStopwatch.Start();
            try
            {
                while (HasWork)
                {
                    if (!Queue.TryDequeue(out var item))
                        continue;
                    Processor(item);
                    WorkProcessedCount++;
                    
                    var elapsedTime = WorkStopwatch.ElapsedMilliseconds - startedTime;
                    if (elapsedTime > MaxMSecPerBatch && !doAllNow)
                        break;
                } 
            }
            catch (Exception ex)
            {
                ExceptionEvent?.Invoke(this, ex);
            }
            finally
            {
                WorkStopwatch.Stop();
            }
        }

        // Technique described by 
        // https://forums.autodesk.com/t5/revit-api-forum/how-to-trigger-onidle-event-or-execution-of-an-externalevent/td-p/6645286
        public static void PokeRevit()
        {
            if (Revit?.HasExited == true || Revit == null)
                return;
            ApiContext.PostMessage(Revit.MainWindowHandle, 0, 0, 0);
        }
    }

    /// <summary>
    /// This uses the background processor with the specific form. 
    /// </summary>
    public class BackgroundUI 
    {
        public BackgroundProcessor<long> Processor;
        public BackgroundForm Form;
        public ExternalEvent EnableProcessorEvent;
        public ExternalEvent DisableProcessorEvent;
        public ExternalEvent DisposeProcessorEvent;
        public ExternalEvent DoSomeWorkEvent;
        public ExternalEvent DoAllWorkEvent;
        public Action<long> OnWorkItem;

        public BackgroundUI(UIApplication uiapp, Action<long> onWorkItem)
        {
            OnWorkItem = onWorkItem;
            Processor = new BackgroundProcessor<long>(Process, uiapp);
            Form = new BackgroundForm();
            Form.Update();
            Form.Show();
            Form.FormClosing += Form_FormClosing;
            Form.checkBoxProcessDuringIdle.Click += CheckBoxProcessDuringIdle_Click;
            Form.checkBoxProcessDuringProgress.Click += CheckBoxProcessDuringProgress_Click;
            Form.checkBoxIdleEventNoDelay.Click += CheckBoxIdleEventNoDelay_Click;
            Form.checkBoxEnabled.Click += CheckBoxEnabled_Click;
            Form.checkBoxPaused.Click += CheckBoxPauseProcessingOnClick;
            Form.checkBoxProcessDuringExternal.Click += CheckBoxProcessDuringExternal_Click;
            Form.numericUpDownMsecPerBatch.ValueChanged += NumericUpDownMsecPerBatch_ValueChanged;
            Form.buttonClearWork.Click += ButtonClearWork_Click;
            Form.buttonResetStats.Click += ButtonResetStats_Click;
            Form.buttonProcessAll.Click += ButtonProcessAll_Click;
            Form.buttonProcessSome.Click += ButtonProcessSome_Click;
            EnableProcessorEvent = ApiContext.CreateEvent(_ => Processor.Enabled = true, "Enable processor");
            DisableProcessorEvent = ApiContext.CreateEvent(_ => Processor.Enabled = false, "Disable processor");
            
            // Very important that we don't forget to disconnect Revit events in Bowerbird, otherwise we have to restart 
            uiapp.Application.DocumentChanged += ApplicationOnDocumentChanged;

            DisposeProcessorEvent = ApiContext.CreateEvent(_ => 
            {
                // Cleaning up the Revit event
                uiapp.Application.DocumentChanged -= ApplicationOnDocumentChanged;
                Processor.Dispose();
                Processor = null;
            }, "Disposing processor and closing form");

            DoSomeWorkEvent = ApiContext.CreateEvent(_ => Processor.ProcessWork(), "Processing some work");
            DoAllWorkEvent = ApiContext.CreateEvent(_ => Processor.ProcessWork(true), "Processing all work");
        }

        private void CheckBoxProcessDuringExternal_Click(object sender, EventArgs e)
        {
            Processor.DoWorkDuringExternal = Form.checkBoxProcessDuringExternal.Checked;
        }

        private void ButtonProcessSome_Click(object sender, EventArgs e)
        {
            DoSomeWorkEvent.Raise();
        }

        private void ButtonProcessAll_Click(object sender, EventArgs e)
        {
            DoAllWorkEvent.Raise();
        }

        private void CheckBoxEnabled_Click(object sender, EventArgs e)
        {
            if (Form.checkBoxEnabled.Checked)
                EnableProcessorEvent.Raise();
            else
                DisableProcessorEvent.Raise();
        }

        private void CheckBoxProcessDuringProgress_Click(object sender, EventArgs e)
        {
            Processor.DoWorkDuringProgress = Form.checkBoxProcessDuringProgress.Checked;
        }

        private void CheckBoxProcessDuringIdle_Click(object sender, EventArgs e)
        {
            Processor.DoWorkDuringIdle = Form.checkBoxProcessDuringIdle.Checked;
        }

        private void Form_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            DisposeProcessorEvent.Raise();
        }

        private void ButtonResetStats_Click(object sender, EventArgs e)
        {
            Processor.ResetStats();
            UpdateForm();
        }

        private void ButtonClearWork_Click(object sender, EventArgs e)
        {
            Processor.ClearWork();
            UpdateForm();
        }

        private void NumericUpDownMsecPerBatch_ValueChanged(object sender, EventArgs e)
        {
            Processor.MaxMSecPerBatch = (int)Form.numericUpDownMsecPerBatch.Value;
        }

        private void CheckBoxPauseProcessingOnClick(object sender, EventArgs e)
        {
            Processor.PauseProcessing = Form.checkBoxPaused.Checked;
        }

        private void CheckBoxIdleEventNoDelay_Click(object sender, EventArgs e)
        {
            Processor.ExecuteNextIdleImmediately = Form.checkBoxIdleEventNoDelay.Checked;
        }

        private void ApplicationOnDocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            Processor.EnqueueWork(e.GetAddedElementIds().Select(eid => eid.Value));
            Processor.EnqueueWork(e.GetDeletedElementIds().Select(eid => eid.Value));
            Processor.EnqueueWork(e.GetModifiedElementIds().Select(eid => eid.Value));
            UpdateForm();
        }

        public void Process(long id)
        {
            OnWorkItem(id);
            UpdateForm();
        }

        public void UpdateForm()
        {
            Form.textBoxCpuTimeOnWork.Text = Processor.WorkStopwatch.PrettyPrintTimeElapsed();
            Form.textBoxItemsQueued.Text = Processor.Queue.Count.ToString();
            Form.textBoxWorkItemProcessed.Text = Processor.WorkProcessedCount.ToString();
            Form.checkBoxPaused.Checked = Processor.PauseProcessing;
            Form.checkBoxEnabled.Checked = Processor.Enabled;
            Form.checkBoxProcessDuringIdle.Checked = Processor.DoWorkDuringIdle;
            Form.checkBoxProcessDuringProgress.Checked = Processor.DoWorkDuringProgress;
            Form.checkBoxIdleEventNoDelay.Checked = Processor.ExecuteNextIdleImmediately;
            Form.numericUpDownMsecPerBatch.Value = Processor.MaxMSecPerBatch;
            System.Windows.Forms.Application.DoEvents();
        }
    }
    
    /// <summary>
    /// This form was designed using Windows designer, and then copied here manually
    /// so that it could be used from Bowerbird 
    /// </summary>
    public class BackgroundForm : System.Windows.Forms.Form
    {
        public BackgroundForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.checkBoxIdleEventNoDelay = new System.Windows.Forms.CheckBox();
            this.checkBoxPaused = new System.Windows.Forms.CheckBox();
            this.numericUpDownMsecPerBatch = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonResetStats = new System.Windows.Forms.Button();
            this.checkBoxProcessDuringIdle = new System.Windows.Forms.CheckBox();
            this.checkBoxProcessDuringProgress = new System.Windows.Forms.CheckBox();
            this.checkBoxEnabled = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxWorkItemProcessed = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxCpuTimeOnWork = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.buttonProcessSome = new System.Windows.Forms.Button();
            this.buttonProcessAll = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxItemsQueued = new System.Windows.Forms.TextBox();
            this.buttonClearWork = new System.Windows.Forms.Button();
            this.checkBoxProcessDuringExternal = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMsecPerBatch)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkBoxIdleEventNoDelay
            // 
            this.checkBoxIdleEventNoDelay.AutoSize = true;
            this.checkBoxIdleEventNoDelay.Location = new System.Drawing.Point(6, 91);
            this.checkBoxIdleEventNoDelay.Name = "checkBoxIdleEventNoDelay";
            this.checkBoxIdleEventNoDelay.Size = new System.Drawing.Size(149, 17);
            this.checkBoxIdleEventNoDelay.TabIndex = 0;
            this.checkBoxIdleEventNoDelay.Text = "Idling Event without Delay";
            this.checkBoxIdleEventNoDelay.UseVisualStyleBackColor = true;
            // 
            // checkBoxPaused
            // 
            this.checkBoxPaused.AutoSize = true;
            this.checkBoxPaused.Location = new System.Drawing.Point(18, 76);
            this.checkBoxPaused.Name = "checkBoxPaused";
            this.checkBoxPaused.Size = new System.Drawing.Size(62, 17);
            this.checkBoxPaused.TabIndex = 1;
            this.checkBoxPaused.Text = "Paused";
            this.checkBoxPaused.UseVisualStyleBackColor = true;
            // 
            // numericUpDownMsecPerBatch
            // 
            this.numericUpDownMsecPerBatch.Location = new System.Drawing.Point(6, 19);
            this.numericUpDownMsecPerBatch.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.numericUpDownMsecPerBatch.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDownMsecPerBatch.Name = "numericUpDownMsecPerBatch";
            this.numericUpDownMsecPerBatch.Size = new System.Drawing.Size(67, 20);
            this.numericUpDownMsecPerBatch.TabIndex = 2;
            this.numericUpDownMsecPerBatch.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(79, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(111, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Msec per Work Batch";
            // 
            // buttonResetStats
            // 
            this.buttonResetStats.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonResetStats.Location = new System.Drawing.Point(7, 80);
            this.buttonResetStats.Name = "buttonResetStats";
            this.buttonResetStats.Size = new System.Drawing.Size(238, 23);
            this.buttonResetStats.TabIndex = 4;
            this.buttonResetStats.Text = "Reset Stats";
            this.buttonResetStats.UseVisualStyleBackColor = true;
            // 
            // checkBoxProcessDuringIdle
            // 
            this.checkBoxProcessDuringIdle.AutoSize = true;
            this.checkBoxProcessDuringIdle.Location = new System.Drawing.Point(6, 45);
            this.checkBoxProcessDuringIdle.Name = "checkBoxProcessDuringIdle";
            this.checkBoxProcessDuringIdle.Size = new System.Drawing.Size(116, 17);
            this.checkBoxProcessDuringIdle.TabIndex = 15;
            this.checkBoxProcessDuringIdle.Text = "Process during Idle";
            this.checkBoxProcessDuringIdle.UseVisualStyleBackColor = true;
            // 
            // checkBoxProcessDuringProgress
            // 
            this.checkBoxProcessDuringProgress.AutoSize = true;
            this.checkBoxProcessDuringProgress.Location = new System.Drawing.Point(6, 68);
            this.checkBoxProcessDuringProgress.Name = "checkBoxProcessDuringProgress";
            this.checkBoxProcessDuringProgress.Size = new System.Drawing.Size(140, 17);
            this.checkBoxProcessDuringProgress.TabIndex = 16;
            this.checkBoxProcessDuringProgress.Text = "Process during Progress";
            this.checkBoxProcessDuringProgress.UseVisualStyleBackColor = true;
            // 
            // checkBoxEnabled
            // 
            this.checkBoxEnabled.AutoSize = true;
            this.checkBoxEnabled.Checked = true;
            this.checkBoxEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxEnabled.Location = new System.Drawing.Point(13, 12);
            this.checkBoxEnabled.Name = "checkBoxEnabled";
            this.checkBoxEnabled.Size = new System.Drawing.Size(65, 17);
            this.checkBoxEnabled.TabIndex = 17;
            this.checkBoxEnabled.Text = "Enabled";
            this.checkBoxEnabled.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.textBoxWorkItemProcessed);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.textBoxCpuTimeOnWork);
            this.groupBox1.Controls.Add(this.buttonResetStats);
            this.groupBox1.Location = new System.Drawing.Point(14, 351);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(251, 122);
            this.groupBox1.TabIndex = 18;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Statistics";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(80, 60);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(95, 13);
            this.label5.TabIndex = 17;
            this.label5.Text = "# Items Processed";
            // 
            // textBoxWorkItemProcessed
            // 
            this.textBoxWorkItemProcessed.Location = new System.Drawing.Point(7, 53);
            this.textBoxWorkItemProcessed.Name = "textBoxWorkItemProcessed";
            this.textBoxWorkItemProcessed.ReadOnly = true;
            this.textBoxWorkItemProcessed.Size = new System.Drawing.Size(66, 20);
            this.textBoxWorkItemProcessed.TabIndex = 16;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(80, 34);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(102, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "CPU Time on Work ";
            // 
            // textBoxCpuTimeOnWork
            // 
            this.textBoxCpuTimeOnWork.Location = new System.Drawing.Point(7, 27);
            this.textBoxCpuTimeOnWork.Name = "textBoxCpuTimeOnWork";
            this.textBoxCpuTimeOnWork.ReadOnly = true;
            this.textBoxCpuTimeOnWork.Size = new System.Drawing.Size(66, 20);
            this.textBoxCpuTimeOnWork.TabIndex = 14;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.checkBoxProcessDuringExternal);
            this.groupBox2.Controls.Add(this.numericUpDownMsecPerBatch);
            this.groupBox2.Controls.Add(this.checkBoxProcessDuringIdle);
            this.groupBox2.Controls.Add(this.checkBoxProcessDuringProgress);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.checkBoxIdleEventNoDelay);
            this.groupBox2.Location = new System.Drawing.Point(14, 201);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(250, 144);
            this.groupBox2.TabIndex = 19;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Options";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.buttonProcessSome);
            this.groupBox3.Controls.Add(this.buttonProcessAll);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.textBoxItemsQueued);
            this.groupBox3.Controls.Add(this.buttonClearWork);
            this.groupBox3.Controls.Add(this.checkBoxPaused);
            this.groupBox3.Location = new System.Drawing.Point(13, 35);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(251, 160);
            this.groupBox3.TabIndex = 20;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Work";
            // 
            // buttonProcessSome
            // 
            this.buttonProcessSome.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonProcessSome.Location = new System.Drawing.Point(7, 128);
            this.buttonProcessSome.Name = "buttonProcessSome";
            this.buttonProcessSome.Size = new System.Drawing.Size(238, 23);
            this.buttonProcessSome.TabIndex = 19;
            this.buttonProcessSome.Text = "Process Some Work Now";
            this.buttonProcessSome.UseVisualStyleBackColor = true;
            // 
            // buttonProcessAll
            // 
            this.buttonProcessAll.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonProcessAll.Location = new System.Drawing.Point(6, 99);
            this.buttonProcessAll.Name = "buttonProcessAll";
            this.buttonProcessAll.Size = new System.Drawing.Size(239, 23);
            this.buttonProcessAll.TabIndex = 18;
            this.buttonProcessAll.Text = "Process All Now";
            this.buttonProcessAll.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(80, 28);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 13);
            this.label3.TabIndex = 17;
            this.label3.Text = "# Items Queued";
            // 
            // textBoxItemsQueued
            // 
            this.textBoxItemsQueued.Location = new System.Drawing.Point(6, 21);
            this.textBoxItemsQueued.Name = "textBoxItemsQueued";
            this.textBoxItemsQueued.ReadOnly = true;
            this.textBoxItemsQueued.Size = new System.Drawing.Size(66, 20);
            this.textBoxItemsQueued.TabIndex = 16;
            // 
            // buttonClearWork
            // 
            this.buttonClearWork.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClearWork.Location = new System.Drawing.Point(6, 47);
            this.buttonClearWork.Name = "buttonClearWork";
            this.buttonClearWork.Size = new System.Drawing.Size(239, 23);
            this.buttonClearWork.TabIndex = 15;
            this.buttonClearWork.Text = "Clear Work";
            this.buttonClearWork.UseVisualStyleBackColor = true;
            // 
            // checkBoxProcessDuringExternal
            // 
            this.checkBoxProcessDuringExternal.AutoSize = true;
            this.checkBoxProcessDuringExternal.Location = new System.Drawing.Point(6, 114);
            this.checkBoxProcessDuringExternal.Name = "checkBoxProcessDuringExternal";
            this.checkBoxProcessDuringExternal.Size = new System.Drawing.Size(137, 17);
            this.checkBoxProcessDuringExternal.TabIndex = 17;
            this.checkBoxProcessDuringExternal.Text = "Process during External";
            this.checkBoxProcessDuringExternal.UseVisualStyleBackColor = true;
            // 
            // BackgroundProcessingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(277, 486);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.checkBoxEnabled);
            this.Name = "BackgroundProcessingForm";
            this.Text = "Background Processor";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMsecPerBatch)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.CheckBox checkBoxIdleEventNoDelay;
        public System.Windows.Forms.CheckBox checkBoxPaused;
        public System.Windows.Forms.NumericUpDown numericUpDownMsecPerBatch;
        public System.Windows.Forms.Label label1;
        public System.Windows.Forms.Button buttonResetStats;
        public System.Windows.Forms.CheckBox checkBoxProcessDuringIdle;
        public System.Windows.Forms.CheckBox checkBoxProcessDuringProgress;
        public System.Windows.Forms.CheckBox checkBoxEnabled;
        public System.Windows.Forms.GroupBox groupBox1;
        public System.Windows.Forms.Label label5;
        public System.Windows.Forms.TextBox textBoxWorkItemProcessed;
        public System.Windows.Forms.Label label4;
        public System.Windows.Forms.TextBox textBoxCpuTimeOnWork;
        public System.Windows.Forms.GroupBox groupBox2;
        public System.Windows.Forms.GroupBox groupBox3;
        public System.Windows.Forms.Button buttonProcessAll;
        public System.Windows.Forms.Label label3;
        public System.Windows.Forms.TextBox textBoxItemsQueued;
        public System.Windows.Forms.Button buttonClearWork;
        public System.Windows.Forms.Button buttonProcessSome;
        public System.Windows.Forms.CheckBox checkBoxProcessDuringExternal;
    }
}