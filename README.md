# <img width="64" src="https://github.com/ara3d/bowerbird/assets/1759994/badd9bb6-61cd-409f-9088-19a9db3f519d"/> Bowerbird

Bowerbird accelerates C# tool and plug-in development by dynamically compiling C# source files.  

## Current Release - Bowerbird for Revit 2025

The current release of Bowerbird is for Revit 2025 only. 

![Bowerbird Screenshot 2024-03-15 104808](https://github.com/ara3d/bowerbird/assets/1759994/b6457096-22ef-4946-9c6f-aea08fcebf74)

## How Bowerbird works

When Bowerbird starts up it scans a directory for C# source files (with the extension `.cs`), 
and attempts to compile them into a single assembly. 

For Revit the source files can be found at: 

`%localappdata%\Ara 3D\Bowerbird for Revit 2025\Scripts`.

The assembly is then loaded into memory and scanned for public classes which implement the 
`INamedCommand` interface.  

```csharp
public interface INamedCommand : ICommand
{
   string Name { get; }
   void NotifyCanExecuteChanged();
}

public interface ICommand 
{
    event EventHandler? CanExecuteChanged;
    bool CanExecute(object? parameter);
    void Execute(object? parameter);
}
```

Each command is listed in the main interface of the application. Double clicking on the interface will launch it.

For Revit the "argument" passed to the Execute function will be an instance of [`UIApplication`](https://www.revitapidocs.com/2017/51ca80e2-3e5f-7dd2-9d95-f210950c72ae.htm). 

Editing and saving any file in the directory will trigger a recompilation of all files, and reloading of the commands.  

## Sample Command

For convenience you can derive a command from a class called `NamedCommand` which provides
default implementations of most functions.

```csharp
 /// <summary>
 /// Displays the current active document in a window
 /// </summary>
 public class CurrentDocument : NamedCommand
 {
     public override string Name => "Current Open Document";

     public override void Execute(object arg)
     {
         var app = (UIApplication)arg;
         var doc = app.ActiveUIDocument?.Document;
         if (doc == null)
         {
             MessageBox.Show("No document open");
         }
         else
         {
             MessageBox.Show($"Open document: {doc.PathName}");
         }
     }
 }
```

## Background and Motivation

### Problem

There are several popular tools, particularly in the realm of 3D design software, which have an SDK with a C# API 
for developing plug-ins. Our current focus is Autodesk Revit. 

Two main challenges with plug-in development are:

1. A lot of overhead and boilerplate for creating new tools
2. A slow development cycle of build, compile, copy, and restart host application.   

Using separate languages for quick scripting and another for distributed plug-ins, fractures the community, 
creates a cognitive load for developers, and is a barrier for some developers. 
  
### Solution

By using a dynamic compilation library based on Roslyn one generic Bowerbird plug-in 
can host many other plug-ins which can be dynamically compiled and modified without restarting the host application.
This means that we can quickly create new experiments, plug-ins, try out new ideas, or test hypotheses aboout 
our code.  

## Issues and Feedback

We appreciate and welcome any feedback.

Please submit issues, suggestions, and questions [via the issues tracker](https://github.com/ara3d/bowerbird/issues).

## Inspiration 

* [pyRevit](https://github.com/eirannejad/pyRevit) by [Ehsan Iran-Nejad](https://github.com/eirannejad).
* [Revit.ScriptCS](https://github.com/sridharbaldava/Revit.ScriptCS) by [Sridhar Baldava](https://github.com/sridharbaldava)



