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
            bool terminate = false;
            bool promptTermination = false;
            string Help = @"
Commands are:
1: Simple CRUD example
H:Help
Esc: Exit examples";
            //Load examples configuration
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", optional: true, reloadOnChange: true);
            IConfigurationRoot config = builder.Build();
            //Create examples Mongo Builder
            var builderMongo = new MongoBuilder(config["connectionString"]);
            //Start up examples engine
            var examples = new ExampleEngine(builderMongo);
            //Map examples entities to mongoDB collectiona
            examples.MappingData();

            Console.WriteLine("Welcome to Vb Mongo Engine examples!");
            Console.WriteLine(Help);
            ConsoleKeyInfo cki;
            do
            {
                cki = Console.ReadKey();
                //terminate examples with escape
                if (promptTermination)
                {
                    switch (cki.Key)
                    {
                        case ConsoleKey.Escape:
                        case ConsoleKey.N:
                            promptTermination = false;
                            Console.WriteLine("");
                            break;
                        case ConsoleKey.Y:
                            terminate = promptTermination;
                            break;
                        default:
                            Console.WriteLine("");
                            Console.WriteLine("Do you wish to terminate the examples program (Y/N)");
                            break;
                    }
                }
                else
                {
                    switch (cki.Key)
                    {
                        case ConsoleKey.Escape:
                                promptTermination = !promptTermination;
                                Console.WriteLine("DDo you wish to terminate the examples program (Y/N)");
                            break;
                        case ConsoleKey.Y:
                            terminate = promptTermination;
                            break;
                        case ConsoleKey.D1:
                            examples.ExecuteExamples(1);
                            break;
                        default:
                            Console.WriteLine("");
                            break;
                    }
                }
            } while (!terminate);
        }
    }
}
