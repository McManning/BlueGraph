
# Transpiler

This is an example of transpiling a C# class runtime from a graph.

## Intended Features

* Creating custom code building logic per-node (e.g. supporting flow control, generating optimized constants, directly calling static functions)
* Building assemblies dynamically at runtime while editing the graph for instant feedback without runnning through the full Unity compilation pipeline
* Creating procedurally generated code from a graph suited for higher performance / mobile (AOT) platforms

This isn't meant to be a competitor with Unity's own upcoming visual scripting - moreso this would be to create a more decoupled pipeline for generated code for specific use cases (e.g. animation, AI, or terrain generation sub-packages)

This project supercedes FuncNode work that was done in prior versions.
