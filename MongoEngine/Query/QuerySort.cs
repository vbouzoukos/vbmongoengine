using System;
namespace Vb.Mongo.Engine.Query
{
    /// <summary>
    /// 
    /// </summary>
    public class QuerySort
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldName">The sort field</param>
        /// <param name="ascending">True if direction of sort is Ascending use false for Descending(Default is True)</param>
        public QuerySort(string fieldName, bool ascending=true)
        {
            Field = fieldName;
            Ascending = ascending;
        }
        /// <summary>
        /// The Field to Sort
        /// </summary>
        public string Field { get; set; }
        /// <summary>
        /// Sorting Direction Ascending or descending
        /// </summary>
        public bool Ascending { get; set; }
    }
}
