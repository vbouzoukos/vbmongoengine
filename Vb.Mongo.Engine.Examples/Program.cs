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
            bool ExitConfirmation = false;
            string Help = "Commands are:\r\n1: Simple CRUD example\r\n2: Use with repository pattern \r\nH:Help\r\nEsc: Exit examples\r\n";
            //Start up examples engine
            //Map examples entities to mongoDB collectiona
            ExampleEngine.Instance.ExecuteExamples(EnExampleMode.StartUp);

            Console.Write(Help);

            ConsoleKeyInfo cki;
            do
            {
                if (ExitConfirmation)
                {
                    ExitConfirmation = false;
                    Console.WriteLine("Exit the examples program? (Y/N)");
                }
                cki = Console.ReadKey(true);
                //terminate examples with escape
                if (promptTermination)
                {
                    switch (cki.Key)
                    {
                        case ConsoleKey.Escape:
                        case ConsoleKey.N:
                            promptTermination = false;
                            Console.WriteLine("Vb.Mongo.Engine Examples");
                            break;
                        case ConsoleKey.Y:
                            terminate = promptTermination;
                            break;
                        default:
                            ExitConfirmation = true;
                            break;
                    }
                }
                else
                {
                    switch (cki.Key)
                    {
                        case ConsoleKey.Escape:
                            promptTermination = !promptTermination;
                            ExitConfirmation = true;
                            break;
                        case ConsoleKey.Y:
                            terminate = promptTermination;
                            break;
                        case ConsoleKey.D1:
                            ExampleEngine.Instance.ExecuteExamples(EnExampleMode.Crud);
                            break;
                        case ConsoleKey.D2:
                            ExampleEngine.Instance.ExecuteExamples(EnExampleMode.RepositoryPattern);
                            break;
                        case ConsoleKey.H:
                            Console.WriteLine(Help);
                            break;
                        default:
                            Console.WriteLine("Unknown command");
                            break;
                    }

                }
            } while (!terminate);
        }
    }
}
