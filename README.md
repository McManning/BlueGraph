
# BlueGraph

A generic visual scripting framework built on top of Unity's experimental GraphView components. Heavily inspired by [xNode](https://github.com/Siccity/xNode) and UE4's Blueprints. 

![BlueGraph Preview](https://github.com/McManning/BlueGraph/blob/master/Preview.png)

## Features

Most of what's advertised for xNode, sans supporting older versions of Unity. Still in progress of trying to come up with a proper feature list that isn't just lifted from theirs. Bear with me. 

* Lightweight runtime graph data
* Editor built on top of UIElements and GraphView
* Easily extendable for different use cases, including UE4-style flow control
* Multiline comment blocks for grouping nodes
* Modularization of nodes and grouping related nodes into searchable subcategories
* Support for exposing pure static functions to the graph, without the need of wrapping each in a class (*platform support is limited*) 
* Typed ports with automatic type conversions where appropriate. `IConvertible` classes supported. 

## Work in Progress

This framework is being built to support AI and procedural content generation for an upcoming title and is not yet ready for prime time. But if you are interested in taking it for a test run yourself, let's talk.
