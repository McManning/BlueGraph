
# BlueGraph

A generic visual scripting framework built on top of Unity's experimental GraphView components. Heavily inspired by [xNode](https://github.com/Siccity/xNode) and UE4's Blueprints. 

![BlueGraph Preview](Documentation~/Preview.png)


## Work in Progress

This framework is being built to support features of an upcoming title and is not yet ready for prime time. But if you are interested in taking it for a test run yourself, let's talk.


## Features

Most of what's advertised for xNode, sans supporting older versions of Unity. Still in progress of trying to come up with a proper feature list that isn't just lifted from theirs. Bear with me. 

* Lightweight runtime graph data
* Editor built on top of UIElements and GraphView
* Easily extendable for different use cases, including UE4-style flow control
* Multiline comment blocks for grouping nodes
* Modularization of nodes and grouping related nodes into searchable subcategories
* Support for exposing pure static functions to the graph, without the need of wrapping each in a class (*AOT platforms not supported - see Examples/ExecGraph for a possible solution*) 
* Typed ports with automatic type conversions where appropriate. `IConvertible` classes supported. 


# Installing with Unity Package Manager

_(Requires Unity version 2019.1 or above)_

To install this project as a Git dependency using the Unity Package Manager, add the following line to your project's manifest.json:

```
"com.github.mcmanning.bluegraph": "https://github.com/McManning/BlueGraph.git"
```


# Examples

The core framework is built to be lightweight and support different workflows and requirements. The Examples folder includes various use cases and some advanced implementations. 


## ExecGraph

Examples of how to add UE4 Blueprints style flow control to graphs via a custom execution port. 

ExecGraph also includes a *basic* example of converting a graph into C# methods to improve runtime performance and support AOT platforms. 

<img align="right" src="Documentation~/MeshPreview.png">


## DynamicVector

Demonstrates how to implement an IConvertible data type. DynamicVector ports can be connected to and from any Unity VectorN port.


## FuncNodes

Examples of how to create modules of `FuncNode` nodes that wrap basic Unity functions. 


## Misc

Other miscellaneous examples, including how to include IMGUI content (such as a mesh preview viewport) and customizing node styles.
