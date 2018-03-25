using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Vb.Mongo.Engine.Query
{
    class QueryField : IQueryField
    {
        public string Field { get; set; }
        public object Value { get; set; }
        public enOperator Operator { get; set; }
        public EnComparator Compare { get; set; }

        public QueryField(string pField, object pValue, enOperator logicOperator, EnComparator compare)
        {
            Field = pField;
            Value = pValue;
            Operator = logicOperator;
            Compare = compare;
        }
    }
}
