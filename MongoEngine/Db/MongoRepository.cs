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
        #region Attributes
        /// <summary>
        /// Owner context
        /// </summary>
        MongoContext Context { get; }

        /// <summary>
        /// Mongo Client
        /// </summary>
        MongoClient Client { get; }
        /// <summary>
        /// MongoDB database used by repositroy
        /// </summary>
        /// 
        IMongoDatabase Database { get; }
        /// <summary>
        /// Collection name
        /// </summary>
        string CollectionName { get; }

        /// <summary>
        /// The id field expression
        /// </summary>
        Expression<Func<T, object>> IdField { get; }

        /// <summary>
        /// Results limits for Find Requests
        /// </summary>
        internal int ResultsLimit { get; set; }
        #endregion

        #region Constants
        /// <summary>
        /// The native id of the mongo objects (when user does not wish to set an attribute as id)
        /// </summary>
        const string mongoId = "_id";
        #endregion

        #region Constructor
        /// <summary>
        /// constractor of repository uses as collection name the name of the class T 
        /// </summary>
        /// <param name="context">Mongo Context</param>
        /// <param name="idField">The Id Field of the repository</param>
        internal MongoRepository(MongoContext context, Expression<Func<T, object>> idField)
        {
            Context = context;
            Client = context.Client;
            Database = Client.GetDatabase(context.DatabaseName);
            CollectionName = typeof(T).Name;
            IdField = idField;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context">Mongo Context</param>
        /// <param name="collectionName">Collection Name used by repository</param>
        /// <param name="idField">The Id Field of the repository</param>
        internal MongoRepository(MongoContext context, string collectionName, Expression<Func<T, object>> idField)
        {
            Context = context;
            Client = context.Client;
            Database = Client.GetDatabase(context.DatabaseName);
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
                return Database.GetCollection<T>(CollectionName);
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
        /// Offers the LinQ capabilities of MongoDB Driver can be used to return all data as queryable (recommend to use with repository pattern)
        /// </summary>
        /// <returns>Results set as IQueryable</returns>
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
        public void Index(string name, Expression<Func<T, object>> field)
        {

            var query = Collection.Indexes.List();
            var bsonIndexes = query.ToList();
            var indexNames = bsonIndexes.Select(i => i["name"]).ToList();

            if (!indexNames.Contains(name))
            {
                CreateIndexOptions options = new CreateIndexOptions
                {
                    Name = name,
                    Unique = false
                };
                var builder = Builders<T>.IndexKeys;
                var indexModel = new CreateIndexModel<T>(builder.Ascending(field), options);
                Collection.Indexes.CreateOne(indexModel);
            }
        }
        #endregion

        #region Store Data
        /// <summary>
        /// Stores an item (Use with repository pattern to create a document)
        /// </summary>
        /// <param name="item">Data to store</param>
        public void Store(T item)
        {
            if (Context.Session == null)
            {
                Collection.InsertOne(item);
            }
            else
            {
                Collection.InsertOne(Context.Session, item);
            }
        }
        /// <summary>
        /// Stores an item asynchronous
        /// </summary>
        /// <param name="item">Data to store</param>
        public async Task StoreAsync(T item)
        {
            if (Context.Session == null)
            {
                await Collection.InsertOneAsync(item);
            }
            else
            {
                await Collection.InsertOneAsync(Context.Session, item);
            }
        }
        /// <summary>
        /// Stores a set of Data in Data Base
        /// </summary>
        /// <param name="items">Data to store</param>
        public void Store(IEnumerable<T> items)
        {
            if (Context.Session == null)
            {
                Collection.InsertMany(items);
            }
            else
            {
                Collection.InsertMany(Context.Session, items);
            }
        }

        /// <summary>
        /// Stores a set of Data in Data Base asynchrony
        /// </summary>
        /// <param name="items">Data to store</param>
        public async Task StoreAsync(IEnumerable<T> items)
        {
            if (Context.Session == null)
            {
                await Collection.InsertManyAsync(items);
            }
            else
            {
                await Collection.InsertManyAsync(Context.Session, items);
            }
        }

        /// <summary>
        /// Replaces an item in database with the given one (Use with repository pattern to Update a document)
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
#if NET451
            var options = new UpdateOptions  { IsUpsert = false };
#else
            var options = new ReplaceOptions { IsUpsert = false };
#endif
            ReplaceOneResult result = null;
            if (Context.Session == null)
            {
                result = Collection.ReplaceOne(filter, item, options);
            }
            else
            {
                result = Collection.ReplaceOne(Context.Session, filter, item, options);
            }
            return result.IsAcknowledged ? result.MatchedCount : 0;

        }

        /// <summary>
        /// Replaces an item in database with the given one asynchrony (Use with repository pattern to Update a document)
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
#if NET451
            var options = new UpdateOptions  { IsUpsert = false };
#else
            var options = new ReplaceOptions { IsUpsert = false };
#endif
            ReplaceOneResult result = null;
            if (Context.Session == null)
            {
                result = await Collection.ReplaceOneAsync(filter, item, options);
            }
            else
            {
                result = await Collection.ReplaceOneAsync(Context.Session, filter, item, options);
            }
            return result.IsAcknowledged ? result.MatchedCount : 0;
        }

        /// <summary>
        /// Updates or inserts a collection of items in database (Use with repository pattern to Update a collection)
        /// </summary>
        /// <param name="items">Data to insert or store</param>
        public void Bulk(IEnumerable<T> items)
        {
            var options = new BulkWriteOptions { IsOrdered = true };
            var writeModel = BulkCollection(items);
            if (Context.Session == null)
            {
                Collection.BulkWrite(writeModel, options);
            }
            else
            {
                Collection.BulkWrite(Context.Session, writeModel, options);
            }
        }

        /// <summary>
        /// Updates or inserts a collection of items in database asynchrony (Use with repository pattern to Update a collection)
        /// </summary>
        /// <param name="items">Data to insert or store</param>
        public async Task BulkAsync(IEnumerable<T> items)
        {
            var options = new BulkWriteOptions { IsOrdered = true };
            var writeModel = BulkCollection(items);
            if (Context.Session == null)
            {
                await Collection.BulkWriteAsync(writeModel, options);
            }
            else
            {
                await Collection.BulkWriteAsync(Context.Session, writeModel, options);
            }
        }

        /// <summary>
        /// Generates writemodel for bulk operation
        /// </summary>
        /// <param name="items">List for the items that will be inserted with a bulk operation</param>
        /// <returns>Write Model</returns>
        private IEnumerable<WriteModel<T>> BulkCollection(IEnumerable<T> items)
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
        private IEnumerable<WriteModel<T>> DeletionCollection(IEnumerable<T> items)
        {
            var deleteModel = new List<WriteModel<T>>();

            foreach (var c in items)
            {
                if (IdField == null)
                {
                        deleteModel.Add(new DeleteOneModel<T>(Builders<T>.Filter.Eq(mongoId, Reflection.ObjectValue(mongoId, c))));
                }
                else
                {
                    deleteModel.Add(new DeleteOneModel<T>(Builders<T>.Filter.Eq(IdField, Reflection.ObjectValue(IdField, c))));
                 }
            }
            return deleteModel;
        }

        /// <summary>
        /// Deletes items that are defined by the expression
        /// </summary>
        /// <param name="expression">Delete criterion</param>
        /// <returns>Count of deleted items</returns>
        public long Delete(Expression<Func<T, bool>> expression)
        {
            DeleteResult result;
            if (Context.Session == null)
            {
                result = Collection.DeleteMany(expression);
            }
            else
            {
                result = Collection.DeleteMany(Context.Session, expression);
            }
            return result.DeletedCount;
        }

        /// <summary>
        /// Deletes items that are defined by the expression async
        /// </summary>
        /// <param name="expression">Delete criterion</param>
        /// <returns>Count of deleted items</returns>
        public async Task<long> DeleteAsync(Expression<Func<T, bool>> expression)
        {
            DeleteResult result;
            if (Context.Session == null)
            {
                result = await Collection.DeleteManyAsync(expression);
            }
            else
            {
                result = await Collection.DeleteManyAsync(Context.Session, expression);
            }
            return result.DeletedCount;
        }

        /// <summary>
        /// Deletes the given items (Use with repository pattern)
        /// </summary>
        /// <param name="items">List for the items that will be deleted with a bulk operation</param>
        public void BulkDelete(IEnumerable<T> items)
        {
            var options = new BulkWriteOptions { IsOrdered = true };
            var delModel = DeletionCollection(items);
            if (Context.Session == null)
            {
                Collection.BulkWrite(delModel, options);
            }
            else
            {
                Collection.BulkWrite(Context.Session, delModel, options);
            }
        }

        /// <summary>
        /// Deletes the given items async(Use with repository pattern)
        /// </summary>
        /// <param name="items"></param>
        public async Task BulkDeleteAsync(IEnumerable<T> items)
        {
            var options = new BulkWriteOptions { IsOrdered = true };
            var delModel = DeletionCollection(items);
            if (Context.Session == null)
            {
                await Collection.BulkWriteAsync(delModel, options);
            }
            else
            {
                await Collection.BulkWriteAsync(Context.Session, delModel, options);
            }
        }
        /// <summary>
        /// Deletes given item (Use with repository pattern)
        /// </summary>
        /// <param name="entity">Entity to be deleted</param>
        /// <returns></returns>
        public long Delete(T item)
        {
            DeleteResult result;
            FilterDefinition<T> filter;
            if (IdField == null)
            {
                filter = Builders<T>.Filter.Eq(mongoId, Reflection.ObjectValue(mongoId, item));
            }
            else
            {
                filter = Builders<T>.Filter.Eq(IdField, Reflection.ObjectValue(IdField, item));
            }
            if (Context.Session == null)
            {
                result = Collection.DeleteOne(filter);
            }
            else
            {
                result = Collection.DeleteOne(Context.Session, filter);
            }
            return
                result.DeletedCount;
        }

        /// <summary>
        /// Deletes given item async (Use with repository pattern)
        /// </summary>
        /// <param name="entity">Entity to be deleted</param>
        /// <returns></returns>
        public async Task<long> DeleteAsync(T item)
        {
            DeleteResult result;
            FilterDefinition<T> filter;
            if (IdField == null)
            {
                filter = Builders<T>.Filter.Eq(mongoId, Reflection.ObjectValue(mongoId, item));
            }
            else
            {
                filter = Builders<T>.Filter.Eq(IdField, Reflection.ObjectValue(IdField, item));
            }
            if (Context.Session == null)
            {
                result = await Collection.DeleteOneAsync(filter);
            }
            else
            {
                result = await Collection.DeleteOneAsync(Context.Session, filter);
            }
            return result.DeletedCount;
        }

        /// <summary>
        /// Deletes items that are defined in the find request
        /// </summary>
        /// <param name="request">Find request to get items to delete</param>
        /// <returns>Count of deleted items</returns>
        public long Delete(FindRequest<T> request)
        {
            var filter = request.BuildFilterDefinition();
            DeleteResult result;
            if (Context.Session == null)
            {
                result = Collection.DeleteMany(filter);
            }
            else
            {
                result = Collection.DeleteMany(Context.Session, filter);
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
            var filter = request.BuildFilterDefinition();
            DeleteResult result;
            if (Context.Session == null)
            {
                result = await Collection.DeleteManyAsync(filter);
            }
            else
            {
                result = await Collection.DeleteManyAsync(Context.Session, filter);
            }
            return result.DeletedCount;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public long DeleteAll()
        {
            var filter = Builders<T>.Filter.Empty;
            DeleteResult result;
            if (Context.Session == null)
            {
                result = Collection.DeleteMany(filter);
            }
            else
            {
                result = Collection.DeleteMany(Context.Session, filter);
            }
            return result.DeletedCount;
        }

        /// <summary>
        ///  Async Deletes all doucments in collection use with repository pattern
        /// </summary>
        /// <returns>Deleted documents count</returns>
        public async Task<long> DeleteAllAsync()
        {
            var filter = Builders<T>.Filter.Empty;
            DeleteResult result;
            if (Context.Session == null)
            {
                result = await Collection.DeleteManyAsync(filter);
            }
            else
            {
                result = await Collection.DeleteManyAsync(Context.Session, filter);
            }
            return result.DeletedCount;
        }
        #endregion

        #region Search
        /// <summary>
        /// Async get all data of Collection
        /// </summary>
        public async Task<IEnumerable<T>> AllDataAsync()
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
        public IEnumerable<T> AllData()
        {
            return Task.Run(async () => { return await AllDataAsync(); }).Result;
        }

        /// <summary>
        /// Core Async search in MongoDB
        /// </summary>
        /// <param name="filter">The filter of the query that defines the search</param>
        /// <param name="sorting">The results sort</param>
        /// <returns>Results List</returns>
        internal async Task<IList<T>> SearchAsync(FilterDefinition<T> filter, SortDefinition<T> sorting = null, int? skip = null, int? take = null)
        {
            var options = new FindOptions<T>
            {
                Sort = sorting,
                Limit = take,
                Skip = skip,
            };
            if (Context.Session != null)
            {
                using (var cursor = await Collection.FindAsync(Context.Session, filter, options))
                {
                    var result = cursor.ToList();
                    return result;
                }
            }
            else
            {
                using (var cursor = await Collection.FindAsync(filter, options))
                {
                    var result = cursor.ToList();
                    return result;
                }
            }
        }

        /// <summary>
        /// Async call to get results from MongoDB
        /// </summary>
        /// <param name="request">The query information that describes the requested search<</param>
        /// <returns>The query information that describes the requested search<</returns>
        internal async Task<IEnumerable<T>> SearchAsync(FindRequest<T> request)
        {

            var filter = request.BuildFilterDefinition();
            var sort = request.BuildSortingDefinition();
            return await SearchAsync(filter, sort, request.Skip, request.Take);
        }

        /// <summary>
        /// Core search of MongoDB
        /// </summary>
        /// <param name="query">The filter of the query that defines the search</param>
        /// <param name="sorting">The results sort</param>
        /// <returns>Results List</returns>
        internal IList<T> Search(FilterDefinition<T> query, SortDefinition<T> sorting = null, int? skip = null, int? take = null)
        {
            return Task.Run(async () => { return await SearchAsync(query, sorting, skip, take); }).Result;
        }

        /// <summary>
        /// Search in Mongo Db Database
        /// </summary>
        /// <param name="request">The query information that describes the requested search</param>
        /// <returns>Results List</returns>
        internal IEnumerable<T> Search(FindRequest<T> request)
        {
            return Task.Run(async () => { return await SearchAsync(request); }).Result;
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
        public FindRequest<T> CreateFindRequest(int page, int itemsPerPage)
        {
            return new FindRequest<T>(this, page, itemsPerPage, ResultsLimit);
        }

        /// <summary>
        /// Return T document by given id value use with repository pattern
        /// </summary>
        /// <param name="id">The id we look for</param>
        /// <returns>T doucment</returns>
        public T FindById(object id)
        {
            var filter = (IdField == null) ? Builders<T>.Filter.Eq(mongoId, id) : Builders<T>.Filter.Eq(IdField, id);
            return Collection.Find(filter).FirstOrDefault();
        }

        /// <summary>
        /// Returns a result of ducuments that satisfies expression condition use with repository pattern
        /// </summary>
        /// <param name="expression">query expression</param>
        /// <returns>Results set as IQueryable</returns>
        public IQueryable<T> Find(Expression<Func<T, bool>> expression)
        {
            return Collection.AsQueryable().Where(expression);
        }
        #endregion
    }
}
