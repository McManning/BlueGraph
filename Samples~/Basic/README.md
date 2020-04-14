
# Basic Samples

Examples showing the basic features of BlueGraph and different strategies for implementing different types of nodes for your own project.

`BasicGraph` demos how to create your own Graph asset and `GraphContainer` demos how to use that graph within a MonoBehaviour. 


## Math and Logic Nodes

Basic examples of creating nodes that one or more inputs and returns one or more outputs to connected edges.


## Kitchen Sink Node

Test bed for every out of the box supported port and editable type


## Embedding IMGUI Content

The `MeshPreview` node provides an example of displaying an interactive IMGUI scene view from inside a node view.


## IConvertible Support 

The `DynamicVector` type is an example of using `IConvertible` to support custom type conversions between different objects. If the object has safe conversion support, then the connection will be allowed within the BlueGraph editor.

Use the `Kitchen Sink` node to view how `DynamicVector` ports can interact with other ports.
