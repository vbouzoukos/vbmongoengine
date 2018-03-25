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


        public void AddCriteria(string pField, object pValue, enOperator logicOperator = enOperator.And, EnComparator compare = EnComparator.EqualTo)
        {
            Fields.Add(new QueryField(pField, pValue, logicOperator, compare));
        }
    }
}
