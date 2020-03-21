using System;
using System.Collections.Generic;
using System.Text;
using Vb.Mongo.Engine.Db;
using Vb.Mongo.Engine.Examples.Data;
using MongoDB.Bson.Serialization.IdGenerators;
namespace Vb.Mongo.Engine.Examples
{
    class ExampleEngine
    {
        const string dbName = "vbmongoengineExamples";
        MongoBuilder ExamplesBuilder { get; }
        public ExampleEngine(MongoBuilder builder)
        {
            ExamplesBuilder = builder;
        }

        public void MappingData()
        {
            ExamplesBuilder.AutoMap<Product>((map) =>
            {
                map.MapIdField(x => x.Id).SetIdGenerator(ExamplesBuilder.CreateAutoIncrementGenerator<Product>());
            });
        }

        /// <summary>
        /// How to create indexes in a mongo database
        /// </summary>
        public void BuildIndexes()
        {
            using var ctx = ExamplesBuilder.CreateContext(dbName);
            ctx.CreateCollectionIfNotExist<Product>();
            var repo = ctx.CreateRepository<Product>();
            //Each Index will be created only when it does not exit
            //Create a unique index for the Product Code
            repo.UniqueIndex("productCode", p => p.Code);
            //Create a unique index for Product Name
            repo.UniqueIndex("productName", p => p.Name);
            //Create an index for Produt Category
            repo.Index("productCategory", p => p.CategoryCode);
        }

        /// <summary>
        /// How to store a single entity into mongoDB
        /// </summary>
        /// <param name="category"></param>
        /// <param name="name"></param>
        /// <param name="price"></param>
        public void StoreProduct(string category, string name, string productCode, double price)
        {
            var newProduct = new Product
            {
                CategoryCode = category,
                Name = name,
                Code = productCode,
                Price = price
            };
            using var ctx = ExamplesBuilder.CreateContext(dbName);
            var repo = ctx.CreateRepository<Product>();
            repo.Store(newProduct);
        }
    }
}
