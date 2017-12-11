Task("Build").Does(() => {
    DotNetCoreBuild("src/Hello/Hello.csproj", new DotNetCoreBuildSettings{
        ToolPath = "/usr/bin/dotnet"
    });
    Information("Builded!");


});

Task("Publish").Does(() => {
    var settings = new DotNetCorePublishSettings
     {
        ToolPath = "/usr/bin/dotnet",
        Framework = "netcoreapp2.0",
        Configuration = "Release",
        OutputDirectory = "./publish/"
     };

    DotNetCorePublish("./test/Heroes.csproj", settings);
    Information("Publish!");
});

Task("Default").Does(() => {
    Information("Default --");
});

var target = Argument("target", "default");

RunTarget(target);