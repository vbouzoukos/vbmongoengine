using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Vb.Mongo.Engine.Db
{
    public class ConnectionManager
    {
        static MongoClient _client = null;

        /// <summary>
        /// Instance of MongoDBClient
        /// </summary>
        public static MongoClient Client
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
        /// Connects to the MongoDB database used by the given connection string
        /// </summary>
        /// <param name="connectionString">The connection string that will be used to connect 
        /// mongodb://[username:password@]hostname[:port][/[database][?options]]</param>
        public static void Connect(string connectionString)
        {
            _client = new MongoClient(connectionString);
        }
        /// <summary>
        /// Deletes a database from mongoDB
        /// </summary>
        /// <param name="dbName"></param>
        public static void DropDatabase(string dbName)
        {
            Client.DropDatabase(dbName);
        }
    }
}
