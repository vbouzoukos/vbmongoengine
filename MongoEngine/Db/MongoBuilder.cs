﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System;
using System.Linq;
using Vb.Mongo.Engine.IdGenerators;

namespace Vb.Mongo.Engine.Db
{
    /// <summary>
    /// Used to create Context instances to mongoDB NoSql Databases
    /// </summary>
    public class MongoBuilder
    {
        #region Constants
        /// <summary>
        /// Engine default results limit
        /// </summary>
        public const int cResultsLimit = 1000;
        public const string cSequenceDatabase = "vbenginesequence";
        #endregion

        #region Attributes
        internal string ConnectionString { get; set; }
        internal int ResultsLimit { get; private set; }
        #endregion

        #region Constractor
        /// <summary>
        /// constractor
        /// </summary>
        /// <param name="connectionString">connection string to mongoDB</param>
        public MongoBuilder(string connectionString, int resultsLimit = cResultsLimit)
        {
            ConnectionString = connectionString;
            ResultsLimit = resultsLimit;
            //map IdGenerators
            AutoMap<AutoIncrement>();
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
            if (databaseName == cSequenceDatabase)
            {
                throw new Exception(string.Format("{0} is used internally for sequence generator", cSequenceDatabase));
            }
            return new MongoContext(this, ConnectionString, databaseName);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal MongoContext SequenceContext()
        {
            return new MongoContext(this, ConnectionString, cSequenceDatabase);
        }
        #endregion

        #region Sequence Generator
        /// <summary>
        /// 
        /// </summary>
        /// <param name="collectionName"></param>
        /// <returns></returns>
        public AutoIncrementGenerator CreateAutoIncrementGenerator(string collectionName)
        {
            return new AutoIncrementGenerator(this, collectionName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public AutoIncrementGenerator CreateAutoIncrementGenerator<T>() where T : class
        {
            return new AutoIncrementGenerator(this, typeof(T).Name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void ResetSequence<T>() where T : class
        {
            ResetSequence(typeof(T).Name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collectionName"></param>
        public void ResetSequence(string collectionName)
        {
            using (var ctx = SequenceContext())
            {
                var repo = ctx.CreateRepository<AutoIncrement>(x => x.Id);
                repo.UniqueIndex("AutoIncrementCollection", x => x.CollectionName);
                var sequence = repo.Find(x => x.CollectionName == collectionName).FirstOrDefault();
                if (sequence == null)
                {
                    sequence = new AutoIncrement
                    {
                        Id = new ObjectId(),
                        CollectionName = collectionName,
                        Sequence = 1
                    };
                }
                else
                {
                    sequence.Sequence = 1;
                }
                repo.Replace(sequence);
            }

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