﻿using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Vb.Mongo.Engine.Find
{
    /// <summary>
    /// Hold information for searching a value of a field in mongoDb
    /// </summary>
    class QueryField
    {
        /// <summary>
        /// The search field
        /// </summary>
		internal string Field { get; set; }
        /// <summary>
        /// 
        /// </summary>
		internal object Value { get; set; }
        /// <summary>
        /// The logical operator AND, Or, NOT
        /// </summary>
        internal EnOperator Operator { get; set; }
        /// <summary>
        /// Comparison between data and value to satisfy the criteria
        /// </summary>
		internal EnComparator Compare { get; set; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="field">The search field</param>
        /// <param name="value">The query value</param>
        /// <param name="logicOperator">The logical operator AND, Or, NOT</param>
        /// <param name="compare">Comparison between data and value to satisfy the criteria</param>
		internal QueryField(string field, object value, EnOperator logicOperator, EnComparator compare)
        {
            Field = field;
            Value = value;
            Operator = logicOperator;
            Compare = compare;
        }
    }
}
