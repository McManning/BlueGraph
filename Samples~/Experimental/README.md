
# Experimental Samples

Nodes and graphs still in development and not ready for release.

Experimental samples are not guaranteed to be complete / functional.

You need to add the Basic & Advanced samples alongside these.


## Break Struct

A work in progress in dynamically converting ports back and forth between a single `struct` and independent ports that make up the fields in the struct. 

Ideally - this can become a general plugin/pattern that can be applied to any type of struct port easily through a tag or wrapper component.


## To Float Array

WIP on dynamically adding new ports as previous ports are filled up to create an array of values. I'm not a fan of a port accepting multiple input values at once (except for some edge cases, like flow control) so this is a more concrete variation. 

Ideally - this can also become a general pattern for quickly adding your own dynamic port logic. Or possibly integrated into the main library as default behaviour when a port is an array of values (e.g. `[Input] public int[] values`)


## Func Node

Legacy node wrapping static functions in an overly complex lambda call tree in order to attempt to quickly generate libraries of nodes without having to create a new class and manual map for each and every one. E.g. to access something like `Mathf.Min` without a lot of boilerplate.

Ultimately, this is superseded by the WIP transpiler concept, since that would always provide cleaner/faster runtime performance and AOT support.
