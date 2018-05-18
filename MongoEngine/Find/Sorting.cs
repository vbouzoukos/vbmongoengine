using System;
namespace Vb.Mongo.Engine.Find
{
    /// <summary>
    /// 
    /// </summary>
    class Sorting
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldName">The sort field</param>
        /// <param name="ascending">True if direction of sort is Ascending use false for Descending(Default is True)</param>
        public Sorting(string fieldName, bool ascending=true)
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
