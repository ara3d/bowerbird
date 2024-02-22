using System;
using System.Collections.Generic;
using System.Windows;
using Ara3D.Bowerbird.Interfaces;
using Ara3D.Utils;

public class SamplePlugin : IPlugin
{
    public void Initialize(IBowerbirdService service)
    {
        MessageBox.Show("Bowerbird says Hello!");
    }

    public void ShutDown()
    { }

    public IReadOnlyList<INamedCommand> Commands
        => Array.Empty<INamedCommand>();
}