using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Vb.Mongo.Engine.Entity;
using Vb.Mongo.Engine.Query;

namespace Vb.Mongo.Engine.Db
{
    public class Core<T> where T : class
    {
        private string _dbName;
        static IMongoDatabase _db = null;

        public Core(string pDbName)
        {
            _dbName = pDbName;
            _db = ConnectionManager.Client.GetDatabase(_dbName);
            BsonClassMap.RegisterClassMap<T>(cm =>
            {
                cm.AutoMap();
            });
        }

        public void DropDatabase()
        {
            ConnectionManager.Client.DropDatabase(_dbName);
        }

        public void Store(IList<T> pItems)
        {
            var collection = _db.GetCollection<T>(nameof(T));
            collection.InsertMany(pItems);
        }

        public IList<T> Search(FilterDefinition<T> query, SortDefinition<T> sorting = null)
        {

            var collection = _db.GetCollection<T>(nameof(T));
            var found = (sorting == null) ? collection.Find(query) : collection.Find(query).Sort(sorting);
            var result = found.ToList();
            return result;
        }

        public IList<T> Search(QueryInfo<T> pQuery)
        {
            var filter = Builders<T>.Filter;

            FilterDefinition<T> query = null;
            SortDefinition<T> sort = null;
            foreach (var criteria in pQuery.Fields)
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
                    case enOperator.And:
                        {
                            if (query == null)
                            {
                                query = filter.And(token); ;
                            }
                            else
                            {
                                query &= token;
                            }
                        }
                        break;
                    case enOperator.Or:
                        {
                            if (query == null)
                            {
                                query = filter.Or(token);
                            }
                            else
                            {
                                query |= token;
                            }
                        }
                        break;
                    case enOperator.Not:
                        {
                            if (query == null)
                            {
                                query = filter.Not(token);
                            }
                            else
                            {
                                query &= filter.Not(token);
                            }
                        }
                        break;
                }
            }
            if (pQuery.Sort.Count > 0)
            {
                var sortBuilder = Builders<T>.Sort;
                foreach (var sortField in pQuery.Sort)
                {
                    if (sort == null)
                    {
                        sort = (sortField.Ascending) ? sortBuilder.Ascending(sortField.Field) : sortBuilder.Descending(sortField.Field);
                    }
                    else
                    {
                        sort = (sortField.Ascending) ? sort.Ascending(sortField.Field) : sort.Descending(sortField.Field);
                    }
                }
            }
            return Search(query, sort);
        }
    }
}
