using RhinoPackager;
using RhinoPackager.Commands;

var app = App.Create(args);
Props props = new("Directory.Build.props");
Github github = new("visose", "RhinoPackager");

app.Add
(
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
);

await app.RunAsync();
