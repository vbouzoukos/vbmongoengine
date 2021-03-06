﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
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
    /// Core mongoDb manager Can search or insert items of type T into the database
    /// </summary>
    /// <typeparam name="T">The Entity stored in mongoDb</typeparam>
    public class Container<T> where T : class
    {
        string _dbName;
        IMongoDatabase _db = null;

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pDbName">The database name</param>
        public Container(string pDbName)
        {
            _dbName = pDbName;
            _db = Settings.Instance.Client.GetDatabase(_dbName);
            if (!BsonClassMap.IsClassMapRegistered(typeof(T)))
            {
                BsonClassMap.RegisterClassMap<T>(cm =>
                {
                    cm.AutoMap();
                });
            }
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
                return _db.GetCollection<T>(nameof(T));
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

        #region Session and Transactions
        /// <summary>
        /// Execute a block of calls in mongodb in a transaction
        /// </summary>
        /// <param name="transactions">Transactions execution code</param>
        public void Transaction(Action transactions)
        {
            using (var session = Settings.Instance.Client.StartSession())
            {
                session.StartTransaction();
                transactions();
                session.CommitTransaction();
            }
        }
        /// <summary>
        /// Execute a block of calls in mongodb in a transaction and return the code call result
        /// </summary>
        /// <typeparam name="TResult">Return template of this transaction</typeparam>
        /// <param name="transactions">Transactions execution code</param>
        /// <returns>The execution result of transactions() call</returns>
        public TResult Transaction<TResult>(Func<TResult> transactions)
        {
            using (var session = Settings.Instance.Client.StartSession())
            {
                session.StartTransaction();
                // execute operations using the session
                var result = transactions();
                session.CommitTransaction();
                return result;
            }
        }
        /// <summary>
        /// Execute a block of calls in mongodb in a transaction asynchronous
        /// </summary>
        /// <param name="transactions">Transactions execution code</param>
        public async Task TransactionAsync(Action transactions)
        {
            using (var session = await Settings.Instance.Client.StartSessionAsync())
            {
                try
                {
                    transactions();
                }
                catch
                {
                    await session.AbortTransactionAsync();
                    throw;
                }
                await session.CommitTransactionAsync();
            }
        }
        /// <summary>
        /// Execute a block of calls in mongodb in a transaction and return the code call result asynchronous
        /// </summary>
        /// <typeparam name="TResult">Return template of this transaction</typeparam>
        /// <param name="transactions">Transactions execution code</param>
        /// <returns>The execution result of transactions() call</returns>
        public async Task<TResult> TransactionAsync<TResult>(Func<TResult> transactions)
        {
            using (var session = await Settings.Instance.Client.StartSessionAsync())
            {
                TResult result;
                try
                {
                    result = transactions();
                }
                catch
                {
                    await session.AbortTransactionAsync();
                    throw;
                }
                await session.CommitTransactionAsync();
                return result;
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
            Collection.InsertOne(item);
        }
        /// <summary>
        /// Stores an item asynchronous
        /// </summary>
        /// <param name="item">Data to store</param>
        public async Task StoreAsync(T item)
        {
            await Collection.InsertOneAsync(item);
        }
        /// <summary>
        /// Stores a set of Data in Data Base
        /// </summary>
        /// <param name="items">Data to store</param>
        public void Store(IList<T> items)
        {
            Collection.InsertMany(items);
        }

        /// <summary>
        /// Stores a set of Data in Data Base asynchrony
        /// </summary>
        /// <param name="items">Data to store</param>
        public async Task StoreAsync(IList<T> items)
        {
            await Collection.InsertManyAsync(items);
        }

        /// <summary>
        /// Replaces an item in database with the given one
        /// </summary>
        /// <param name="idField">Id Field expression</param>
        /// <param name="item">Data to insert or store</param>
        public long Replace(Expression<Func<T, object>> idField, T item)
        {
            var filter = Builders<T>.Filter.Eq(idField, Metadata.ObjectValue(idField, item));
            var options = new UpdateOptions { IsUpsert = false };
            var result = Collection.ReplaceOne(filter, item, options);
            return result.IsAcknowledged ? result.MatchedCount : 0;
        }

        /// <summary>
        /// Replaces an item in database with the given one asynchrony
        /// </summary>
        /// <param name="idField">Id Field expression</param>
        /// <param name="item">Data to insert or store</param>
        public async Task<long> ReplaceAsync(Expression<Func<T, object>> idField, T item)
        {
            var filter = Builders<T>.Filter.Eq(idField, Metadata.ObjectValue(idField, item));
            var options = new UpdateOptions { IsUpsert = false };
            var result = await Collection.ReplaceOneAsync(filter, item, options);
            return result.IsAcknowledged ? result.MatchedCount : 0;
        }

        /// <summary>
        /// Updates or inserts a collection of items in database
        /// </summary>
        /// <param name="idField">Id Field expression</param>
        /// <param name="items">Data to insert or store</param>
        public void Bulk(Expression<Func<T, object>> idField, IList<T> items)
        {
            var options = new BulkWriteOptions { IsOrdered = true };
            var writeModel = BulkCollection(idField, items);
            var result = Collection.BulkWrite(writeModel, options);
        }

        /// <summary>
        /// Updates or inserts a collection of items in database asynchrony
        /// </summary>
        /// <param name="idField">Id Field expression</param>
        /// <param name="items">Data to insert or store</param>
        public async Task BulkAsync(Expression<Func<T, object>> idField, IList<T> items)
        {
            var options = new BulkWriteOptions { IsOrdered = true };
            var writeModel = BulkCollection(idField, items);
            var result = await Collection.BulkWriteAsync(writeModel, options);
        }

        /// <summary>
        /// Generates writemodel for bulk operation
        /// </summary>
        /// <param name="idField">Expression for the mongoId Object</param>
        /// <param name="items">List for the items that will be inserted with a bulk operation</param>
        /// <returns>Write Model</returns>
        private IEnumerable<WriteModel<T>> BulkCollection(Expression<Func<T, object>> idField, IList<T> items)
        {
            var writeModel = new List<WriteModel<T>>();

            foreach (var c in items)
            {
                if (Metadata.EmptyMongoId(idField, c))
                {//Insert cause Upsert is retarded with Empty ObjectId and tries to update it
                    writeModel.Add(new InsertOneModel<T>(c));
                }
                else
                {
                    writeModel.Add(new ReplaceOneModel<T>(Builders<T>.Filter.Eq(idField, Metadata.ObjectValue(idField, c)), c) { IsUpsert = true });
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
            var result = Collection.DeleteMany(filter);
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
            var result = await Collection.DeleteManyAsync(filter);
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
        #endregion

    }
}
