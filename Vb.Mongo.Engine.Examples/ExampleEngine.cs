using System;
using System.Collections.Generic;
using System.Text;
using Vb.Mongo.Engine.Db;
using Vb.Mongo.Engine.Examples.Data;
using MongoDB.Bson.Serialization.IdGenerators;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Vb.Mongo.Engine.Examples
{
    enum EnExampleMode
    {
        StartUp = 0,
        Crud = 1,
        RepositoryPattern = 2
    }
    /// <summary>
    /// Singleton Pattern class to handle examples execution
    /// </summary>
    class ExampleEngine
    {
        #region privates
        const string dbName = "vbmongoengineExamples";
        MongoBuilder ExamplesBuilder { get; }
        static ExampleEngine _instance = null;
        #endregion

        #region Singleton Instance
        /// <summary>
        /// Returns the singleton instance of examples
        /// </summary>
        public static ExampleEngine Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ExampleEngine();
                }
                return _instance;
            }
        }
        #endregion

        #region Constructor and StartUp
        //Example constructor
        ExampleEngine()
        {
            //Load examples configuration
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", optional: true, reloadOnChange: true);
            IConfigurationRoot config = builder.Build();
            //Create examples Mongo Builder
            ExamplesBuilder = new MongoBuilder(config["connectionString"]);
            Console.WriteLine("Starting up examples engine");
            Console.WriteLine("Mapping classes to mongoDB collection");
            MappingData();
            Console.WriteLine("Building Collections Indexes");
            BuildIndexes();
        }

        void MappingData()
        {
            ExamplesBuilder.AutoMap<Product>((map) =>
            {
                //We map the product entity into a mongodb collection with the same name
                //We want the Id attribute to be the object Id of the collection and to be an Auto Increment Number
                map.MapIdField(x => x.Id).SetIdGenerator(ExamplesBuilder.CreateAutoIncrementGenerator<Product>());
            });
        }

        /// <summary>
        /// How to create indexes in a mongo database
        /// </summary>
        void BuildIndexes()
        {
            //We create the context of the mongoDb database
            using var ctx = ExamplesBuilder.CreateContext(dbName);
            //we create the collection and we want to have the same name with the class
            ctx.CreateCollectionIfNotExist<Product>();
            //Get a mongdb repository iin order to create the indexes
            var repo = ctx.CreateRepository<Product>();
            //Each Index will be created only when it does not exit
            //Create a unique index for the Product Code
            repo.UniqueIndex("productCode", p => p.Code);
            //Create a unique index for Product Name
            repo.UniqueIndex("productName", p => p.Name);
            //Create an index for Produt Category
            repo.Index("productCategory", p => p.CategoryCode);
        }
        #endregion

        #region Examples Execution
        public void ExecuteExamples(EnExampleMode example)
        {
            switch (example)
            {
                case EnExampleMode.StartUp:
                    Console.WriteLine("Welcome to Vb Mongo Engine examples!");
                    break;
                case EnExampleMode.Crud:
                    CrudExample();
                    break;
                case EnExampleMode.RepositoryPattern:
                    Repository();
                    break;
            }
        }
        #endregion

        #region Crud
        public void CrudExample()
        {
            Console.WriteLine("");
            Console.WriteLine("Running CRUD example");
            Console.WriteLine("Storing a new Product with name: 'Shampoo Fluf', Category: Hair, Product Code: HPS245, Price: 10.99 €");
            StoreProduct("Hair", "Shampoo Fluf", "HPS245", 10.99);
            var created = SearchProcuct("HPS245");
            Console.WriteLine("Product Created with attributes");
            Console.WriteLine(created);
            created.Price = 9.99;
            Console.WriteLine("Update Product price to 9.99 €");
            UpdateProcuct(created);
            var updated = SearchProcuct("HPS245");
            Console.WriteLine("Product updated with attributes");
            Console.WriteLine(updated);
            UpdateProcuct(updated);
            Console.WriteLine("Product HPS245 will be deleted");
            DeleteProcuct(updated);
            var result = SearchProcuct("HPS245");
            if (result == null)
            {
                Console.WriteLine("Product HPS245 was not found");
            }
        }
        /// <summary>
        /// How to store a single entity into mongoDB
        /// </summary>
        /// <param name="category"></param>
        /// <param name="name"></param>
        /// <param name="price"></param>
        public void StoreProduct(string category, string name, string productCode, double price)
        {
            //create the new product instance
            var newProduct = new Product
            {
                CategoryCode = category,
                Name = name,
                Code = productCode,
                Price = price
            };
            using var ctx = ExamplesBuilder.CreateContext(dbName);
            var repo = ctx.CreateRepository<Product>(x => x.Id);
            repo.Store(newProduct);
        }

        /// <summary>
        /// Delete an Item
        /// </summary>
        /// <param name="product">Producto to be deleted</param>
        public void DeleteProcuct(Product product)
        {
            using var ctx = ExamplesBuilder.CreateContext(dbName);
            var repo = ctx.CreateRepository<Product>(x => x.Id);
            repo.Delete(x => x.Id == product.Id);
        }
        /// <summary>
        /// Update an Item
        /// </summary>
        /// <param name="product">Producto to be replaced</param>
        public void UpdateProcuct(Product product)
        {
            using var ctx = ExamplesBuilder.CreateContext(dbName);
            var repo = ctx.CreateRepository<Product>(x => x.Id);
            repo.Replace(product);
        }
        /// <summary>
        /// Search an item given a product code
        /// </summary>
        /// <param name="code">Code to search</param>
        /// <returns></returns>
        public Product SearchProcuct(string code)
        {
            using var ctx = ExamplesBuilder.CreateContext(dbName);
            var repo = ctx.CreateRepository<Product>(x => x.Id);
            var product = repo.Find(x => x.Code == code).FirstOrDefault();
            return product;
        }
        #endregion
        #region Repository
        public void Repository()
        {
            Console.WriteLine("");
            Console.WriteLine("Running RepositoryPattern example");
            //create the product repository
            var productRepo = new RepositoryPattern.ProductRepository();
            //Delete all existing data
            Console.WriteLine("Call ProductRepository DeleteAll to remove all existing objects in collection");
            //Delete all data
            productRepo.DeleteAll();
            //reset the sequence
            ExamplesBuilder.ResetSequence<Product>();
            //List of products to create
            var newProducts = new List<Product>
            {
                new Product{ CategoryCode="Furniture", Code="FS1", Name="Comfy sofa", Price=300 },
                new Product{ CategoryCode="Furniture", Code="FC2", Name="Desk chair", Price=100 },
                new Product{ CategoryCode="Workout", Code="WC1", Name="Kettlebell", Price=30 },
                new Product{ CategoryCode="Workout", Code="WC2", Name="Dumbbell", Price=100 },
                new Product{ CategoryCode="Hair", Code="HS1", Name="Hair Shampoo", Price=4 },
            };
            //Store new products
            Console.WriteLine("Call ProductRepository Create to create a set of new product");
            productRepo.Create(newProducts);
            //Get new Created products
            newProducts = productRepo.FindAll().ToList();
            Console.WriteLine("Created the product:");
            foreach (var p in newProducts)
            {
                Console.WriteLine(p);
            }
            Console.WriteLine("Create a new Product");
            var created = new Product { CategoryCode = "Motorcycles", Code = "MC1", Name = "FastCycle Xstream 122", Price = 15000 };
            //Create New product
            productRepo.Create(created);
            //search a new product
            created = productRepo.FindByCondition(x => x.Code == "MC1").FirstOrDefault();
            Console.WriteLine("Created the product:");
            Console.WriteLine(created);
            Console.WriteLine("Call ProductRepository Update to update the prices of products");
            var updateSet = new List<Product>
            {
                new Product
                {
                    Id=newProducts[0].Id,
                    CategoryCode=newProducts[0].CategoryCode,
                    Code=newProducts[0].Code,
                    Name=newProducts[0].Name,
                    Price=249.99
                },
                new Product
                {
                    Id=newProducts[2].Id,
                    CategoryCode=newProducts[2].CategoryCode,
                    Code=newProducts[2].Code,
                    Name=newProducts[2].Name,
                    Price=25.99
                },
                new Product
                {
                    Id=created.Id,
                    CategoryCode=created.CategoryCode,
                    Code=created.Code,
                    Name=created.Name,
                    Price=13999
                }
            };
            //Update the data of the products using updateSet collection
            productRepo.Update(updateSet);
            //search updated products
            var updatedProducts = productRepo.FindByCondition(x => x.Code == "MC1" || x.Code == "FS1" || x.Code == "WC1");
            Console.WriteLine("Updated the products:");
            foreach (var p in updatedProducts)
            {
                Console.WriteLine(p);
            }
            //update one object
            var toUpdate = newProducts[3];
            toUpdate.Name = "Dumbbell";
            toUpdate.Price = 111.99;
            //Update the data of the product
            productRepo.Update(toUpdate);
            //search updated product by its Id
            var singleUpdate = productRepo.FindById(toUpdate.Id);
            Console.WriteLine("Updated the product:");
            Console.WriteLine(singleUpdate);
            Console.WriteLine($"Delete product with code: '{toUpdate.Code}'");
            //delete the update product
            productRepo.Delete(toUpdate);
            //Get the rest of the products
            newProducts = productRepo.FindAll().ToList();
            Console.WriteLine("Products left after the deletion:");
            foreach (var p in newProducts)
            {
                Console.WriteLine(p);
            }
            Console.WriteLine("Delete products with codes: 'MC1,HS1'");
            //delete by condition
            productRepo.Delete(x=>x.Code== "MC1" || x.Code == "HS1");
            //Get the rest of the products
            newProducts = productRepo.FindAll().ToList();
            Console.WriteLine("Products left after the deletion:");
            foreach (var p in newProducts)
            {
                Console.WriteLine(p);
            }
            Console.WriteLine($"Delete products with codes: '{newProducts[0].Code},{newProducts[1].Code}'");
            productRepo.Delete(new List<Product> { newProducts[0], newProducts[1]});
            newProducts = productRepo.FindAll().ToList();
            Console.WriteLine("Products left after the deletion:");
            foreach (var p in newProducts)
            {
                Console.WriteLine(p);
            }
        }

        #endregion
        #region Context
        public MongoContext CreateContext()
        {
            return ExamplesBuilder.CreateContext(dbName);
        }
        #endregion
    }
}
