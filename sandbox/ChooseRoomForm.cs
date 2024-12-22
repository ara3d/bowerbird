using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Ara3D.Bowerbird.RevitSamples
{
    public partial class ChooseRoomForm : Form
    {
        public BuildingLayout Layout { get; set; }
        public Action<RoomStruct> RoomChangedCallback { get; set; }
        public List<RoomStruct> CurrentRooms = new List<RoomStruct>();
        private int _lastHighlightedIndex = -1;

        public ChooseRoomForm(BuildingLayout layout, Action<RoomStruct> roomChangedCallback)
        {
            InitializeComponent();
            comboBoxLevels.Items.AddRange(layout.Levels.Values.Cast<object>().ToArray());
            Layout = layout;
            RoomChangedCallback = roomChangedCallback;
            comboBoxLevels.SelectedIndex = 0;
        }

        public void UpdateRooms()
        {
            listBoxRoomList.Items.Clear();

            var level = Layout.Levels.Values.FirstOrDefault(l => l.ToString() == comboBoxLevels.Text);

            if (level == null)
                return;

            foreach (var room in Layout.Rooms.Values)
            {
                if (room.Level == level.Id)
                {
                    listBoxRoomList.Items.Add(room);
                }
            }
        }

        private void listBoxRoomList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedRoom = listBoxRoomList.SelectedItem as RoomStruct;
            RoomChangedCallback(selectedRoom);
        }

        private void comboBoxLevels_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateRooms();
        }

        private void listBoxRoomList_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            var currentIndex = listBoxRoomList.IndexFromPoint(e.Location);
            if (currentIndex != ListBox.NoMatches && currentIndex != _lastHighlightedIndex)
            {
                _lastHighlightedIndex = currentIndex;

                var selectedRoom = listBoxRoomList.Items[currentIndex] as RoomStruct;
                RoomChangedCallback(selectedRoom);
            }
        }
    }
}
