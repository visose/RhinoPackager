using RhinoPackager;
using RhinoPackager.Commands;

var app = App.Create(args);
var github = new Github("visose", "RhinoPackager");

app.Add(new ICommand[]
    {
        new CheckVersion(github),
        new Nuget
        (
            project: "src/RhinoPackager/RhinoPackager.csproj"
        ),
        new Release
        (
            github: github
        )
    });

await app.RunAsync();