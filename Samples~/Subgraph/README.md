
# Subgraph Sample

Example of creating a subgraph asset with specific IO ports (as defined by IO nodes within the graph) and executing that subgraph from within another graph asset.

## Required Samples

This sample requires the **Advanced** sample to also be loaded.

## Usage

Create a new `Subgraph` asset.

In the graph editor for that asset, you can add additional `Input` and `Output` nodes to setup the IO for the subgraph - each with custom display names (by clicking the node title) and port types.

Once you've saved the graph, make an `Executable Graph` asset and add a `Exec Subgraph` node to it. Assign it to an instance of your subgraph to automatically populate IO ports of the new node.

Add anything else you want to the parent `Executable Graph` and then click **Execute** to test your subgraph.
