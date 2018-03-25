using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using MongoDB.Bson;

namespace Vb.Mongo.Engine.Query
{
    interface IQueryField
    {
        string Field { get; set; }
        object Value { get; set; }
        enOperator Operator { get; set; }
        EnComparator Compare { get; set; }
    }
}
