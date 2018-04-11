using System;
namespace Vb.Mongo.Engine.Query
{
    public class QuerySort:IQuerySort
    {
        public QuerySort(string fieldName, bool ascending=true)
        {
            Field = fieldName;
            Ascending = ascending;
        }

        public string Field { get; set; }
        public bool Ascending { get; set; }
    }
}
