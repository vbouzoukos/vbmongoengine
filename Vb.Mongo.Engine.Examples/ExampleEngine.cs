using System;
using System.Collections.Generic;
using System.Text;
using Vb.Mongo.Engine.Db;
using Vb.Mongo.Engine.Examples.Data;
using MongoDB.Bson.Serialization.IdGenerators;
using System.Linq;

namespace Vb.Mongo.Engine.Examples
{
    enum enExampleMode
    {
        Crud = 1,
        RepositoryPattern = 2
    }
    class ExampleEngine
    {
        #region privates
        const string dbName = "vbmongoengineExamples";
        MongoBuilder ExamplesBuilder { get; }
        #endregion

        //Example constructor
        public ExampleEngine(MongoBuilder builder)
        {
            ExamplesBuilder = builder;
        }

        public void MappingData()
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
        public void BuildIndexes()
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
        public void ExecuteExamples(int example)
        {
            var exampleCode = (enExampleMode)example;
            switch (exampleCode)
            {
                case enExampleMode.Crud:
                    CrudExample();
                    break;
            }
        }
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
            var repo = ctx.CreateRepository<Product>(x=>x.Id);
            repo.Store(newProduct);
        }

        /// <summary>
        /// Delete an Item
        /// </summary>
        /// <param name="product">Producto to be deleted</param>
        public void DeleteProcuct(Product product)
        {
            using var ctx = ExamplesBuilder.CreateContext(dbName);
            var repo = ctx.CreateRepository<Product>(x=>x.Id);
            repo.Delete(x => x.Id == product.Id);
        }
        /// <summary>
        /// Update an Item
        /// </summary>
        /// <param name="product">Producto to be replaced</param>
        public void UpdateProcuct(Product product)
        {
            using var ctx = ExamplesBuilder.CreateContext(dbName);
            var repo = ctx.CreateRepository<Product>(x=>x.Id);
            repo.Replace(product);
        }
        public Product SearchProcuct(string code)
        {
            using var ctx = ExamplesBuilder.CreateContext(dbName);
            var repo = ctx.CreateRepository<Product>(x=>x.Id);
            var product = repo.Find(x => x.Code == code).FirstOrDefault();
            return product;
        }
        #endregion
    }
}
