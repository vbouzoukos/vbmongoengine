using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Configuration.FileExtensions;
using Microsoft.Extensions.Configuration.Json;
using System;
using System.IO;
using Vb.Mongo.Engine.Db;

namespace Vb.Mongo.Engine.Examples
{
    class Program
    {
        static void Main()
        {

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", optional: true, reloadOnChange: true);

            IConfigurationRoot config = builder.Build();
            var builderMongo = new MongoBuilder(config["connectionString"]);
            var examples = new ExampleEngine(builderMongo);
            examples.MappingData();
            Console.WriteLine("Welcome to Vb Mongo Engine examples!");
        }
    }
}
