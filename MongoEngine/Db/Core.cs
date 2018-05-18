using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Vb.Mongo.Engine.Entity;
using Vb.Mongo.Engine.Find;

namespace Vb.Mongo.Engine.Db
{
    /// <summary>
    /// Core mongoDb manager Can search or insert items of type T into the database
    /// </summary>
    /// <typeparam name="T">The Entity stored in mongoDb</typeparam>
    public class Core<T> where T : class
    {
        private string _dbName;
        static IMongoDatabase _db = null;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pDbName">The database name</param>
        public Core(string pDbName)
        {
            _dbName = pDbName;
            _db = Settings.Instance.Client.GetDatabase(_dbName);
            BsonClassMap.RegisterClassMap<T>(cm =>
            {
                cm.AutoMap();
            });
        }
        /// <summary>
        /// Stores a set of Data in Data Base
        /// </summary>
        /// <param name="pItems">Data to store</param>
        public void Store(IList<T> pItems)
        {
            var collection = _db.GetCollection<T>(nameof(T));
            collection.InsertMany(pItems);
        }

        /// <summary>
        /// Stores a set of Data in Data Base async
        /// </summary>
        /// <param name="pItems">Data to store</param>
        public async Task StoreAsync(IList<T> pItems)
        {
            var collection = _db.GetCollection<T>(nameof(T));
            await collection.InsertManyAsync(pItems);
        }

        /// <summary>
        /// Core search of MongoDB
        /// </summary>
        /// <param name="query">The filter of the query that defines the search</param>
        /// <param name="sorting">The results sort</param>
        /// <returns>Results List</returns>
        public IList<T> Search(FilterDefinition<T> query, SortDefinition<T> sorting = null, int? skip = null, int? take=null)
        {
            return Task.Run(async () => { return await SearchAsync(query, sorting, skip, take); }).Result;
        }

        /// <summary>
        /// Search in Mongo Db Database
        /// </summary>
        /// <param name="pQuery">The query information that describes the requested search</param>
        /// <returns></returns>
        public IList<T> Search(FindRequest<T> request)
        {

            var filter = buildFilterDefinition(request);
            var sort = buildSortingDefinition(request);
            return Search(filter, sort, request.Skip, request.Take);
        }

        /// <summary>
        /// Core Async search in MongoDB
        /// </summary>
        /// <param name="query">The filter of the query that defines the search</param>
        /// <param name="sorting">The results sort</param>
        /// <returns>Results List</returns>
        public async Task<IList<T>> SearchAsync(FilterDefinition<T> query, SortDefinition<T> sorting = null,int ? skip = null, int? take = null)
        {
            var collection = _db.GetCollection<T>(nameof(T));
            var options = new FindOptions<T>
            {
                Sort = sorting,
                Limit=take,
                Skip=skip
            };
            using (var cursor = await collection.FindAsync(query, options))
            {
                var result = cursor.ToList();
                return result;
            }
        }

        /// <summary>
        /// Async call to get results from MongoDB
        /// </summary>
        /// <param name="request">The query information that describes the requested search<</param>
        /// <returns>The query information that describes the requested search<</returns>
        public async Task<IList<T>> SearchAsync(FindRequest<T> request)
        {

            var filter = buildFilterDefinition(request);
            var sort = buildSortingDefinition(request);
            return await SearchAsync(filter, sort, request.Skip,request.Take);
        }

        /// <summary>
        /// Generate a filter definition from a Query Information
        /// </summary>
        /// <param name="queryInfo">The query information that describes the requested search</param>
        /// <returns>MongoDB Filter definition for T</returns>
        FilterDefinition<T> buildFilterDefinition(FindRequest<T> queryInfo)
        {
            var filter = Builders<T>.Filter;
            FilterDefinition<T> filterDef = null;
            foreach (var criteria in queryInfo.Fields)
            {
                FilterDefinition<T> token = null;
                switch (criteria.Compare)
                {
                    case EnComparator.Like:
                        token = filter.Regex(criteria.Field, BsonRegularExpression.Create(criteria.Value));
                        break;
                    case EnComparator.GreaterThan:
                        token = filter.Gt(criteria.Field, BsonValue.Create(criteria.Value));
                        break;
                    case EnComparator.LessThan:
                        token = filter.Lt(criteria.Field, BsonValue.Create(criteria.Value));
                        break;
                    default:
                        token = filter.Eq(criteria.Field, BsonValue.Create(criteria.Value));
                        break;
                }
                switch (criteria.Operator)
                {
                    case EnOperator.And:
                        {
                            if (filterDef == null)
                            {
                                filterDef = filter.And(token); ;
                            }
                            else
                            {
                                filterDef &= token;
                            }
                        }
                        break;
                    case EnOperator.Or:
                        {
                            if (filterDef == null)
                            {
                                filterDef = filter.Or(token);
                            }
                            else
                            {
                                filterDef |= token;
                            }
                        }
                        break;
                    case EnOperator.Not:
                        {
                            if (filterDef == null)
                            {
                                filterDef = filter.Not(token);
                            }
                            else
                            {
                                filterDef &= filter.Not(token);
                            }
                        }
                        break;
                }
            }
            return filterDef;
        }
        /// <summary>
        /// Generate a sort definition from a Query Information
        /// </summary>
        /// <param name="queryInfo">The query information that describes the requested search</param>
        /// <returns>MongoDB Filter definition for T</returns>
        SortDefinition<T> buildSortingDefinition(FindRequest<T> queryInfo)
        {
            SortDefinition<T> sortDef = null;

            if (queryInfo.Sort.Count > 0)
            {
                var sortBuilder = Builders<T>.Sort;
                foreach (var sortField in queryInfo.Sort)
                {
                    if (sortDef == null)
                    {
                        sortDef = (sortField.Ascending) ? sortBuilder.Ascending(sortField.Field) : sortBuilder.Descending(sortField.Field);
                    }
                    else
                    {
                        sortDef = (sortField.Ascending) ? sortDef.Ascending(sortField.Field) : sortDef.Descending(sortField.Field);
                    }
                }
            }
            return sortDef;
        }
    }
}
