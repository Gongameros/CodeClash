var builder = DistributedApplication.CreateBuilder(args);

var mongodb = builder.AddMongoDB("mongodb")
    .WithMongoExpress()
    .AddDatabase("codeclash-db");

builder.Build().Run();
