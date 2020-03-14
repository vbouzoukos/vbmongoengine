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
        internal MongoClient Client { get; }
        internal string DatabaseName { get; }
        internal IClientSessionHandle Session { get; private set; }

        public MongoContext(string connectionString, string databaseName)
        {
            Client = new MongoClient(connectionString);
            DatabaseName = databaseName;
        }

        public void AutoMap<T>() where T : class, new()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(T)))
            {
                BsonClassMap.RegisterClassMap<T>(cm =>
                {
                    cm.AutoMap();
                });
            }
        }
        public void CreateCollectionIfNotExist(string collectionName)
        {
            var database = Client.GetDatabase(DatabaseName);
            if (!database.ListCollectionNames().ToList().Contains(collectionName))
            {
                database.CreateCollection(collectionName);
            }
        }

        public MongoRepository<T> CreateRepository<T>(Expression<Func<T, object>> idField) where T : class, new()
        {
            return new MongoRepository<T>(this, idField);
        }

        public MongoRepository<T> CreateRepository<T>(Expression<Func<T, object>> idField, string collectionName) where T : class, new()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(T)))
            {
                BsonClassMap.RegisterClassMap<T>(cm =>
                {
                    cm.AutoMap();
                });
            }
            return new MongoRepository<T>(this, collectionName, idField);
        }

        public MongoRepository<T> CreateRepository<T>(Expression<Func<T, object>> idField, string collectionName, Action settings) where T : class, new()
        {
            settings();
            return new MongoRepository<T>(this, collectionName, idField);
        }

        public void BeginTransaction()
        {
            if (Session == null)
            {
                Session = Client.StartSession();
            }
            Session.StartTransaction();
        }

        public void CommitTransaction()
        {
            if (Session == null)
            {
                throw new ArgumentNullException("Cannot commit transaction without starting one");
            }
            Session.CommitTransaction();
        }

        public void RollbackTransaction()
        {
            Session.AbortTransaction();
        }
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

        public void Purge()
        {
            Client.DropDatabase(DatabaseName);
        }
        /*
         internal String ConnectionString { get; set; }
         static MongoContext _instance = null;
         MongoClient _client = null;

         /// <summary>
         /// Mongo DB results limit
         /// </summary>
         public int ResultsLimit { get; set; } = 1000;

         /// <summary>
         /// Singleton instance of Engine Settings
         /// </summary>
         public static MongoContext Instance
         {
             get
             {
                 if (_instance == null)
                 {
                     throw new Exception("Mongo Db is not set you need to call Start Up");
                 }
                 return _instance;
             }
         }

         /// <summary>
         /// Instance of MongoDBClient
         /// </summary>
         public MongoClient Client
         {
             get
             {
                 if (_client == null)
                 {
                     _client = new MongoClient(MongoContext.Instance.ConnectionString);
                 }
                 return _client;
             }
         }

         /// <summary>
         /// Define mongoDB connection string
         /// </summary>
         /// <param name="connectionString">The connection string that will be used to connect 
         /// mongodb://[username:password@]hostname[:port][/[database][?options]]</param>
         public static void StartUp(string connectionString)
         {
             _instance = new MongoContext() { ConnectionString = connectionString };
             _instance.Connect(connectionString);
         }

         /// <summary>
         /// Connects to the MongoDB database used by the given connection string
         /// </summary>
         /// <param name="connectionString">The connection string that will be used to connect 
         /// mongodb://[username:password@]hostname[:port][/[database][?options]]</param>
         public void Connect(string connectionString)
         {
             _client = new MongoClient(connectionString);
         }

         /// <summary>
         /// Deletes a database from mongoDB
         /// </summary>
         /// <param name="dbName">The database name</param>
         public void DropDatabase(string dbName)
         {
             Client.DropDatabase(dbName);
         }
         */
    }
}
