using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Vb.Mongo.Engine.Entity;

namespace Vb.Mongo.Engine.Query
{
    public class QueryInfo<T> where T : class
    {
        internal IList<IQueryField> Fields { get; set; } = new List<IQueryField>();
        internal IList<IQuerySort> Sort { get; set; } = new List<IQuerySort>();

        public void AddCriteria(string field, object value, enOperator logicOperator = enOperator.And, EnComparator compare = EnComparator.EqualTo)
        {
            Fields.Add(new QueryField(field, value, logicOperator, compare));
        }

        public void AddSort(string field, bool ascending = true)
        {
            Sort.Add(new QuerySort(field,ascending));
        }
    }
}
