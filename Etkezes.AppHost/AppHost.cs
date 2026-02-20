var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Etkezes_Ellenor>("etkezes-ellenor");

builder.AddProject<Projects.Etkezes_API>("etkezes-api");

builder.AddProject<Projects.Etkezes_Nyilvantarto>("etkezes-nyilvantarto");

builder.Build().Run();
