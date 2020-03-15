using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Vb.Mongo.Engine.Entity;
using Vb.Mongo.Engine.Find;

namespace Vb.Mongo.Engine.Db
{
    /// <summary>
    /// Core mongoDb repository can search or insert items of type T into the database
    /// </summary>
    /// <typeparam name="T">The Entity stored in mongoDb</typeparam>
    public class MongoRepository<T> where T : class
    {
        MongoContext vContext { get; }
        MongoClient vClient { get; }
        IMongoDatabase vDatabase { get; }
        string CollectionName { get; }
        Expression<Func<T, object>> IdField { get; }
        const string mongoId = "_id";
        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context">Mongo Context</param>
        /// <param name="idField">The Id Field of the repository</param>
        internal MongoRepository(MongoContext context, Expression<Func<T, object>> idField)
        {
            vContext = context;
            vClient = context.Client;
            vDatabase = vClient.GetDatabase(context.DatabaseName);
            CollectionName = typeof(T).Name;
            IdField = idField;
        }
        internal MongoRepository(MongoContext context, string collectionName, Expression<Func<T, object>> idField)
        {
            vContext = context;
            vClient = context.Client;
            vDatabase = vClient.GetDatabase(context.DatabaseName);
            CollectionName = collectionName;
            IdField = idField;
        }
        #endregion

        #region MongoDB Native
        /// <summary>
        /// Get Mongo Db Collection for T instance
        /// </summary>
        public IMongoCollection<T> Collection
        {
            get
            {
                return vDatabase.GetCollection<T>(CollectionName);
            }
        }

        /// <summary>
        /// Filter Builder for T instance
        /// </summary>
        public FilterDefinitionBuilder<T> FilterBuilder
        {
            get
            {
                return Builders<T>.Filter;
            }
        }
        /// <summary>
        /// LinQ capabilities of MongoDB Driver
        /// </summary>
        public IMongoQueryable<T> Queryable
        {
            get
            {
                return Collection.AsQueryable();
            }
        }
        #endregion

        #region Indexing
        /// <summary>
        /// Creates a unique index
        /// </summary>
        /// <param name="name">The name of the unique index</param>
        /// <param name="field">The field where we set the unique index</param>
        public void UniqueIndex(string name, Expression<Func<T, object>> field)
        {

            var query = Collection.Indexes.List();
            var bsonIndexes = query.ToList();
            var indexNames = bsonIndexes.Select(i => i["name"]).ToList();

            if (!indexNames.Contains(name))
            {
                CreateIndexOptions options = new CreateIndexOptions
                {
                    Name = name,
                    Unique = true
                };
                var builder = Builders<T>.IndexKeys;
                var indexModel = new CreateIndexModel<T>(builder.Ascending(field), options);
                Collection.Indexes.CreateOne(indexModel);
            }
        }
        #endregion

        #region Store Data
        /// <summary>
        /// Stores an item
        /// </summary>
        /// <param name="item">Data to store</param>
        public void Store(T item)
        {
            if (vContext.Session == null)
            {
                Collection.InsertOne(item);
            }
            else
            {
                Collection.InsertOne(vContext.Session, item);
            }
        }
        /// <summary>
        /// Stores an item asynchronous
        /// </summary>
        /// <param name="item">Data to store</param>
        public async Task StoreAsync(T item)
        {
            if (vContext.Session == null)
            {
                await Collection.InsertOneAsync(item);
            }
            else
            {
                await Collection.InsertOneAsync(vContext.Session, item);
            }
        }
        /// <summary>
        /// Stores a set of Data in Data Base
        /// </summary>
        /// <param name="items">Data to store</param>
        public void Store(IList<T> items)
        {
            if (vContext.Session == null)
            {
                Collection.InsertMany(items);
            }
            else
            {
                Collection.InsertMany(vContext.Session, items);
            }
        }

        /// <summary>
        /// Stores a set of Data in Data Base asynchrony
        /// </summary>
        /// <param name="items">Data to store</param>
        public async Task StoreAsync(IList<T> items)
        {
            if (vContext.Session == null)
            {
                await Collection.InsertManyAsync(items);
            }
            else
            {
                await Collection.InsertManyAsync(vContext.Session, items);
            }
        }

        /// <summary>
        /// Replaces an item in database with the given one
        /// </summary>
        /// <param name="item">Data to insert or store</param>
        public long Replace(T item)
        {
            FilterDefinition<T> filter;
            if (IdField == null)
            {
                filter = Builders<T>.Filter.Eq(mongoId, Reflection.ObjectValue(mongoId, item));
            }
            else
            { 
                 filter = Builders<T>.Filter.Eq(IdField, Reflection.ObjectValue(IdField, item));
            }
            var options = new UpdateOptions { IsUpsert = false };
            ReplaceOneResult result = null;
            if (vContext.Session == null)
            {
                result = Collection.ReplaceOne(filter, item, options);
            }
            else
            {
                result = Collection.ReplaceOne(vContext.Session, filter, item, options);
            }
            return result.IsAcknowledged ? result.MatchedCount : 0;
        }

        /// <summary>
        /// Replaces an item in database with the given one asynchrony
        /// </summary>
        /// <param name="item">Data to insert or store</param>
        public async Task<long> ReplaceAsync(T item)
        {
            FilterDefinition<T> filter;
            if (IdField == null)
            {
                filter = Builders<T>.Filter.Eq(mongoId, Reflection.ObjectValue(mongoId, item));
            }
            else
            {
                filter = Builders<T>.Filter.Eq(IdField, Reflection.ObjectValue(IdField, item));
            }
            var options = new UpdateOptions { IsUpsert = false };
            ReplaceOneResult result = null;
            if (vContext.Session == null)
            {
                result = await Collection.ReplaceOneAsync(filter, item, options);
            }
            else
            {
                result = await Collection.ReplaceOneAsync(vContext.Session, filter, item, options);
            }
            return result.IsAcknowledged ? result.MatchedCount : 0;
        }

        /// <summary>
        /// Updates or inserts a collection of items in database
        /// </summary>
        /// <param name="items">Data to insert or store</param>
        public void Bulk(IList<T> items)
        {
            var options = new BulkWriteOptions { IsOrdered = true };
            var writeModel = BulkCollection(items);
            if (vContext.Session == null)
            {
                Collection.BulkWrite(writeModel, options);
            }
            else
            {
                Collection.BulkWrite(vContext.Session, writeModel, options);
            }
        }

        /// <summary>
        /// Updates or inserts a collection of items in database asynchrony
        /// </summary>
        /// <param name="items">Data to insert or store</param>
        public async Task BulkAsync(IList<T> items)
        {
            var options = new BulkWriteOptions { IsOrdered = true };
            var writeModel = BulkCollection(items);
            if (vContext.Session == null)
            {
                await Collection.BulkWriteAsync(writeModel, options);
            }
            else
            {
                await Collection.BulkWriteAsync(vContext.Session, writeModel, options);
            }
        }

        /// <summary>
        /// Generates writemodel for bulk operation
        /// </summary>
        /// <param name="items">List for the items that will be inserted with a bulk operation</param>
        /// <returns>Write Model</returns>
        private IEnumerable<WriteModel<T>> BulkCollection(IList<T> items)
        {
            var writeModel = new List<WriteModel<T>>();

            foreach (var c in items)
            {
                if (IdField == null)
                {
                    if (Reflection.EmptyMongoId(mongoId, c))
                    {//Insert cause Upsert is retarded with Empty ObjectId and tries to update it
                        writeModel.Add(new InsertOneModel<T>(c));
                    }
                    else
                    {
                        writeModel.Add(new ReplaceOneModel<T>(Builders<T>.Filter.Eq(mongoId, Reflection.ObjectValue(mongoId, c)), c) { IsUpsert = true });
                    }
                }
                else
                {
                    if (Reflection.EmptyMongoId(IdField, c))
                    {//Insert cause Upsert is retarded with Empty ObjectId and tries to update it
                        writeModel.Add(new InsertOneModel<T>(c));
                    }
                    else
                    {
                        writeModel.Add(new ReplaceOneModel<T>(Builders<T>.Filter.Eq(IdField, Reflection.ObjectValue(IdField, c)), c) { IsUpsert = true });
                    }
                }
            }

            return writeModel;
        }
        #endregion

        #region Delete Data
        /// <summary>
        /// Deletes items that are defined in the find request
        /// </summary>
        /// <param name="request">Find request to get items to delete</param>
        /// <returns>Count of deleted items</returns>
        public long Delete(FindRequest<T> request)
        {
            var filter = request.buildFilterDefinition();
            DeleteResult result;
            if (vContext.Session == null)
            {
                result = Collection.DeleteMany(filter);
            }
            else
            {
                result = Collection.DeleteMany(vContext.Session, filter);
            }
            return result.DeletedCount;
        }

        /// <summary>
        /// Deletes items that are defined in the find request asynchrony
        /// </summary>
        /// <param name="request">Find request to get items to delete</param>
        /// <returns>Count of deleted items</returns>
        public async Task<long> DeleteAsync(FindRequest<T> request)
        {
            var filter = request.buildFilterDefinition();
            DeleteResult result;
            if (vContext.Session == null)
            {
                result = await Collection.DeleteManyAsync(filter);
            }
            else
            {
                result = await Collection.DeleteManyAsync(vContext.Session, filter);
            }
            return result.DeletedCount;
        }
        #endregion

        #region Search
        /// <summary>
        /// Async get all data of Collection
        /// </summary>
        public async Task<IList<T>> AllDataAsync()
        {
            using (var cursor = await Collection.FindAsync(_ => true))
            {
                var result = cursor.ToList();
                return result;
            }
        }

        /// <summary>
        /// Get all data of collection
        /// </summary>
        public IList<T> AllData()
        {
            return Task.Run(async () => { return await AllDataAsync(); }).Result;
        }

        /// <summary>
        /// Core Async search in MongoDB
        /// </summary>
        /// <param name="filter">The filter of the query that defines the search</param>
        /// <param name="sorting">The results sort</param>
        /// <returns>Results List</returns>
        public async Task<IList<T>> SearchAsync(FilterDefinition<T> filter, SortDefinition<T> sorting = null, int? skip = null, int? take = null)
        {
            var options = new FindOptions<T>
            {
                Sort = sorting,
                Limit = take,
                Skip = skip
            };
            using (var cursor = await Collection.FindAsync(filter, options))
            {
                var result = cursor.ToList();
                return result;
            }
        }

        /// <summary>
        /// Core search of MongoDB
        /// </summary>
        /// <param name="query">The filter of the query that defines the search</param>
        /// <param name="sorting">The results sort</param>
        /// <returns>Results List</returns>
        public IList<T> Search(FilterDefinition<T> query, SortDefinition<T> sorting = null, int? skip = null, int? take = null)
        {
            return Task.Run(async () => { return await SearchAsync(query, sorting, skip, take); }).Result;
        }

        /// <summary>
        /// Search in Mongo Db Database
        /// </summary>
        /// <param name="request">The query information that describes the requested search</param>
        /// <returns>Results List</returns>
        public IList<T> Search(FindRequest<T> request)
        {
            return Task.Run(async () => { return await SearchAsync(request); }).Result;
        }

        /// <summary>
        /// Async call to get results from MongoDB
        /// </summary>
        /// <param name="request">The query information that describes the requested search<</param>
        /// <returns>The query information that describes the requested search<</returns>
        public async Task<IList<T>> SearchAsync(FindRequest<T> request)
        {

            var filter = request.buildFilterDefinition();
            var sort = request.buildSortingDefinition();
            return await SearchAsync(filter, sort, request.Skip, request.Take);
        }

        /// <summary>
        /// Creates a dynamic find request for search
        /// </summary>
        /// <returns></returns>
        public FindRequest<T> CreateFindRequest()
        {
            return new FindRequest<T>(this);
        }

        /// <summary>
        /// Creates a dynamic find request for search with paging options
        /// </summary>
        /// <param name="page">Results page number</param>
        /// <param name="itemsPerPage">Items per result page</param>
        /// <param name="limitUp">Max Items to return</param>
        /// <returns></returns>
        public FindRequest<T> CreateFindRequest(int page, int itemsPerPage, int limitUp = 1000)
        {
            return new FindRequest<T>(this, page, itemsPerPage, limitUp);
        }
        #endregion

    }
}
