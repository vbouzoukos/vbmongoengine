using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace Vb.Mongo.Engine.Db
{
    /// <summary>
    /// MongoDb settings manages Connection with mongoDB
    /// </summary>
    public class Settings
    {

        internal String ConnectionString { get; set; }
        static Settings _instance = null;
        MongoClient _client = null;

        /// <summary>
        /// Mongo DB results limit
        /// </summary>
        public int ResultsLimit { get; set; } = 1000;

        /// <summary>
        /// Singleton instance of Engine Settings
        /// </summary>
        public static Settings Instance
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
                    _client = new MongoClient(Settings.Instance.ConnectionString);
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
            _instance = new Settings() { ConnectionString = connectionString };
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
    }
}
