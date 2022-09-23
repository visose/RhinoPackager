using RhinoPackager;
using RhinoPackager.Commands;

var app = App.Create(args);
var props = new Props("Directory.Build.props");
var github = new Github("visose", "RhinoPackager");

app.Add(new ICommand[]
    {
        new CheckVersion
        (
            props: props,
            github: github
        ),
        new Nuget
        (
            props: props,
            project: "src/RhinoPackager/RhinoPackager.csproj"
        ),
        new Release
        (
            props: props,
            github: github
        )
    });

await app.RunAsync();