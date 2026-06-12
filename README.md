# RhinoPackager

RhinoPackager is a small build helper for packaging and publishing Rhino and Grasshopper plug-ins.

It provides composable commands for common release steps:

- build and test .NET projects
- create Yak packages
- create NuGet packages
- check GitHub release versions
- publish GitHub releases and release assets

Typical usage is through a repository-specific build project that references `RhinoPackager` and composes the required commands.
