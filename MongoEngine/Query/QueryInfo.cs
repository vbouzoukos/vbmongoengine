using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Vb.Mongo.Engine.Entity;

namespace Vb.Mongo.Engine.Query
{
    /// <summary>
    /// Query Information used to search in mongoDb Database
    /// </summary>
    /// <typeparam name="T">Stored entity</typeparam>
    public class QueryInfo<T> where T : class
    {
        internal IList<QueryField> Fields { get; set; } = new List<QueryField>();
        internal IList<QuerySort> Sort { get; set; } = new List<QuerySort>();
        /// <summary>
        /// Insert a search criteria condition into the query information
        /// </summary>
        /// <param name="field">The search field</param>
        /// <param name="value">The query value</param>
        /// <param name="logicOperator">The logical operator AND, Or , NOT</param>
        /// <param name="compare">Comparison between data and value to satisfy the criteria</param>
        public void AddCriteria(string field, object value, EnOperator logicOperator = EnOperator.And, EnComparator compare = EnComparator.EqualTo)
        {
            Fields.Add(new QueryField(field, value, logicOperator, compare));
        }
        /// <summary>
        /// Add a field into the sorting of result
        /// </summary>
        /// <param name="field">The sort field</param>
        /// <param name="ascending">True if direction of sort is Asceding use false for Descending(Default is True)</param>
        public void AddSort(string field, bool ascending = true)
        {
            Sort.Add(new QuerySort(field,ascending));
        }
    }
}
