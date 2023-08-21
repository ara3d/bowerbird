# Bowerbird

Bowerbird simplifies and accelerates C# plug-in development by allowing you to dynamically compile assemblies 
on the fly for your favorite tool's C# API.

## What is it?  

Bowerbird a set of libraries and conventions based on Roslyn for enabling dynamic (scripted) C# plug-in development 
in different environments.  

## Problem

There are several popular tools, particularly in the realm of 3D design software, which have an SDK with a C# API for developing plug-ins. 
Two main challenges with plug-in development are:

1. A lot of overhead and boilerplate for creating new tools
2. The slower development cycle of build, compile, copy, and restart host application.   

Two motivating examples for us at Ara 3D are: 
* 3ds Max
* Revit

Other examples include:
* Maya
* Navisworks
* AutoCAD
* Unity
* Rhino
  
## Solution

By using a dynamic compilation library based on Roslyn and Ptarmigan one generic Bowerbird plug-in 
can host many other plug-ins which can be dynamically compiled and modified without restarting the host application.
This means that we can quickly create new experiments, plug-ins, try out new ideas, or test hypotheses or our code.  

## Inspiration 

* MCG
* pyRevit
* [Revit.ScriptCS](https://github.com/sridharbaldava/Revit.ScriptCS)
* Unity 

## Future work 

* Running Bowerbird in the web: - https://github.com/Suchiman/Runny
* Running Bowerbird in Unity - https://worldofzero.com/videos/runtime-c-scripting-embedding-the-net-roslyn-compiler-in-unity/
