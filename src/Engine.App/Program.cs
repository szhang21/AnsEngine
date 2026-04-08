using Engine.App;

IRuntimeBootstrap bootstrap = new RuntimeBootstrap();
IApplication app = bootstrap.Build();
int exitCode = app.Run();

return exitCode;
