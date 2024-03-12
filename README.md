# Bowerbird

<img width="256" alt="Bowerbird-Github-Small" src="https://github.com/ara3d/bowerbird/assets/1759994/6f6f9fac-15b1-48a4-b8dd-5cbe88a0175a">

Bowerbird accelerates C# plug-in development by allowing you to dynamically compile assemblies 
on the fly for your favorite tool's C# API.

Bowerbird also allows you to do quickly create simple scripts and tools using C# in the same way you would 
use Python or another language. 

## Problem

There are several popular tools, particularly in the realm of 3D design software, which have an SDK with a C# API 
for developing plug-ins.  Two main challenges with plug-in development are:

1. A lot of overhead and boilerplate for creating new tools
2. A slow development cycle of build, compile, copy, and restart host application.   

Using separate languages for quick scripting and another for distributed plug-ins, fractures the community, 
creates a cognitive load for developers, and is a barrier for some developers. 
  
## Solution

By using a dynamic compilation library based on Roslyn one generic Bowerbird plug-in 
can host many other plug-ins which can be dynamically compiled and modified without restarting the host application.
This means that we can quickly create new experiments, plug-ins, try out new ideas, or test hypotheses aboout 
our code.  

## Important: Builing the Source Code

This repository is intended to be used as a sub-module of 
[the main Ara3D repository](http://github.com/ara3d/ara3d). If you want to build the 
code from source, clone that repository, and build the solution. 

## Details: Bowerbird for Revit 

The Revit Bowerbird plug-in is currently only built for Revit 2023. This will change in the future. 

Scripts are loaded from the folder: 

`%programdata%\Autodesk\Revit\Addins\2023\Ara3D.Bowerbird`

## Inspiration 

* [pyRevit](https://github.com/eirannejad/pyRevit)
* [Revit.ScriptCS](https://github.com/sridharbaldava/Revit.ScriptCS)
* [Unity](https://unity.com/) 

## Future work 

* Rhino 
* 3ds Max
* Maya
* Navisworks
* AutoCAD
* ArchiCAD
* Unity
* Running Bowerbird in the web: 
	* https://github.com/Suchiman/Runny
* Running Bowerbird in Unity
	* https://worldofzero.com/videos/runtime-c-scripting-embedding-the-net-roslyn-compiler-in-unity/
	* https://assetstore.unity.com/packages/tools/integration/roslyn-c-runtime-compiler-142753

