var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Etkezes_Ellenor>("etkezes-ellenor");

builder.Build().Run();
