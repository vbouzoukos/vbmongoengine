using MongoDB.Bson.Serialization;
using System;

namespace Vb.Mongo.Engine.Db
{
    /// <summary>
    /// Used to create Context instances to mongoDB NoSql Databases
    /// </summary>
    public class Orchestrator
    {
        #region Constants
        /// <summary>
        /// Engine default results limit
        /// </summary>
        public const int cResultsLimit = 1000;
        #endregion

        #region Attributes
        internal string ConnectionString { get; set; }
        static Orchestrator _instance = null;
        internal int ResultsLimit { get; private set; }
        #endregion

        #region Constractor
        /// <summary>
        /// constractor
        /// </summary>
        /// <param name="connectionString">connection string to mongoDB</param>
        public Orchestrator(string connectionString, int resultsLimit = cResultsLimit)
        {
            ConnectionString = connectionString;
            ResultsLimit = resultsLimit;
        }
        #endregion

        #region Context Creation
        /// <summary>
        /// Creates a Context instance with the database
        /// </summary>
        /// <param name="databaseName">database to be used by the context</param>
        /// <returns></returns>
        public MongoContext CreateContext(string databaseName)
        {
            return new MongoContext(this, ConnectionString, databaseName);
        }
        #endregion
        #region Mapping
        /// <summary>
        /// Automap for Mongodb
        /// </summary>
        /// <typeparam name="T">Class to automap</typeparam>
        public void AutoMap<T>() where T : class
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(T)))
            {
                BsonClassMap.RegisterClassMap<T>(cm =>
                {
                    cm.AutoMap();
                });
            }
        }

        /// <summary>
        /// Automap for Mongodb
        /// </summary>
        /// <typeparam name="T">Class to automap</typeparam>
        /// <param name="attributesMapping">call that maps to attributes</param>
        public void AutoMap<T>(Action<BsonClassMap<T>> attributesMapping) where T : class
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(T)))
            {
                BsonClassMap.RegisterClassMap<T>(cm =>
                {
                    cm.AutoMap();
                    attributesMapping(cm);
                });
            }
        }
        #endregion
    }
}
