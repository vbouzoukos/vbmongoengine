using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Vb.Mongo.Engine.Find;

namespace Vb.Mongo.Engine.Db
{
    /// <summary>
    /// MongoDb context manages connection with mongoDB
    /// </summary>
    public class MongoContext : IDisposable
    {
        #region Attributes
        internal MongoClient Client { get; }
        internal string DatabaseName { get; }
        internal IClientSessionHandle Session { get; private set; }
        internal MongoBuilder Builder { get; }
        #endregion

        #region Constractor
        /// <summary>
        /// Constractor
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="connectionString"></param>
        /// <param name="databaseName"></param>
        internal MongoContext(MongoBuilder builder, string connectionString, string databaseName)
        {
            Client = new MongoClient(connectionString);
            DatabaseName = databaseName;
            Builder = builder;
        }
        #endregion

        #region Repositories
        /// <summary>
        /// Create a collection if the collection does not exist (use before starting transactions on empty data)
        /// </summary>
        /// <typeparam name="T">Class which name will be used for indexing</typeparam>
        public void CreateCollectionIfNotExist<T>() where T : class
        {
            CreateCollectionIfNotExist(typeof(T).Name);
        }
        /// <summary>
        /// Create a collection if the collection does not exist (use before starting transactions on empty data)
        /// </summary>
        /// <param name="collectionName">Collection Name</param>
        public void CreateCollectionIfNotExist(string collectionName)
        {
            var database = Client.GetDatabase(DatabaseName);
            var collectionFilter = new ListCollectionNamesOptions
            {
                Filter = Builders<MongoDB.Bson.BsonDocument>.Filter.Eq("name", collectionName)
            };
            if (!database.ListCollectionNames(collectionFilter).Any())
            {
                database.CreateCollection(collectionName);
            }
        }

        /// <summary>
        /// Creates a repository for class of type T, this class has as id the default id attribute of mongoDB (_id)
        /// </summary>
        /// <typeparam name="T">T class</typeparam>
        /// <returns>Repository instance</returns>
        public MongoRepository<T> CreateRepository<T>() where T : class
        {
            return new MongoRepository<T>(this, null) { ResultsLimit = Builder.ResultsLimit };
        }

        /// <summary>
        /// Creates a repository for class of type T, this class has as id the default id attribute of mongoDB (_id)
        /// </summary>
        /// <typeparam name="T">T class</typeparam>
        /// <param name="collectionName">the collection name</param>
        /// <returns>Repository instance</returns>
        public MongoRepository<T> CreateRepository<T>(string collectionName) where T : class
        {
            return new MongoRepository<T>(this, collectionName, null) { ResultsLimit = Builder.ResultsLimit };
        }

        /// <summary>
        /// Creates a repository for class of type T, this class has as id the default id attribute of mongoDB (_id)
        /// </summary>
        /// <typeparam name="T">T class</typeparam>
        /// <param name="collectionName">the collection name</param>
        /// <param name="settings"></param>
        /// <returns>Repository instance</returns>
        public MongoRepository<T> CreateRepository<T>(string collectionName, Action settings) where T : class
        {
            settings();
            return new MongoRepository<T>(this, collectionName, null) { ResultsLimit = Builder.ResultsLimit };
        }
        /// <summary>
        /// Creates a repository for class of type T that has an annotation mapping of data
        /// </summary>
        /// <typeparam name="T">T class</typeparam>
        /// <param name="idField">The id field of the class</param>
        /// <returns>Repository instance</returns>
        public MongoRepository<T> CreateRepository<T>(Expression<Func<T, object>> idField) where T : class
        {
            return new MongoRepository<T>(this, idField) { ResultsLimit = Builder.ResultsLimit };
        }

        /// <summary>
        /// Creates a repository for class of type T that has an annotation mapping of data
        /// </summary>
        /// <typeparam name="T">T class</typeparam>
        /// <param name="idField">The id field of the class</param>
        /// <param name="collectionName">the collection name</param>
        /// <returns>Repository instance</returns>
        public MongoRepository<T> CreateRepository<T>(Expression<Func<T, object>> idField, string collectionName) where T : class
        {
            return new MongoRepository<T>(this, collectionName, idField) { ResultsLimit = Builder.ResultsLimit };
        }

        /// <summary>
        /// Creates a repository for class of type T that has an annotation mapping of data
        /// </summary>
        /// <typeparam name="T">T class</typeparam>
        /// <param name="idField">The id field of the class</param>
        /// <param name="collectionName">the collection name</param>
        /// <param name="settings"></param>
        /// <returns>Repository instance</returns>
        public MongoRepository<T> CreateRepository<T>(Expression<Func<T, object>> idField, string collectionName, Action settings) where T : class
        {
            settings();
            return new MongoRepository<T>(this, collectionName, idField) { ResultsLimit = Builder.ResultsLimit };
        }
        #endregion

        #region Transactions
        /// <summary>
        /// Starts a transaction an error will raise if the collection does not exists or not in a replica set
        /// </summary>
        public void BeginTransaction()
        {
            if (Session == null)
            {
                Session = Client.StartSession();
            }
            Session.StartTransaction();
        }
        /// <summary>
        /// Commits the changes to mongodb
        /// </summary>
        public void CommitTransaction()
        {
            if (Session == null)
            {
                throw new ArgumentNullException("Cannot commit transaction without starting one");
            }
            Session.CommitTransaction();
        }

        /// <summary>
        /// rollbacks the transaction
        /// </summary>
        public void RollbackTransaction()
        {
            Session.AbortTransaction();
        }
        #endregion

        #region Maintenance
        /// <summary>
        /// Drops database
        /// </summary>
        public void DropDatabase()
        {
            Client.DropDatabase(DatabaseName);
        }
        #endregion

        #region IDisposable
        /// <summary>
        /// Disposes the instance
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // NOTE: Leave out the finalizer altogether if this class doesn't 
        // own unmanaged resources itself, but leave the other methods
        // exactly as they are. 
        ~MongoContext()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }

        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                if (Session != null)
                {
                    Session.Dispose();
                }
            }
            // free native resources here if there are any
        }
        #endregion
    }
}
