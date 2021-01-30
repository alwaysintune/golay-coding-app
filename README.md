# Golay Coding App
An app written for Coding Theory course to show the effectiveness of Golay code C23. It can correct three mistakes in a vector (e.g. First Scenario -> vector: '111111111111', probability: '0.01' inputted without clauses).

## Scenario 1
Encodes a single vector, sends it through the channel and, on click, decodes it. Optionally, it is possible to edit received vector's values before decoding.

## Scenario 2
An implementation of Golay code C23 by encoding text input.

## Scenario 3
It is not recommended to use huge image files, because processing is done on cpu. Implementation utilizes threads where appropriate. Main concern is with RAM usage - Bitmaps are, in a way, bloated, and the way C# Garbage Collector handles [LOH](https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/large-object-heap).
