﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Vb.Mongo.Engine.Db;
using Vb.Mongo.Engine.Entity;

namespace Vb.Mongo.Engine.Find
{
    /// <summary>
    /// Query Information used to search in mongoDb Database
    /// </summary>
    /// <typeparam name="T">Stored entity</typeparam>
    public class FindRequest<T> where T : class
    {
        #region Properties
        internal IList<QueryField> Fields { get; set; } = new List<QueryField>();
		internal IList<Sorting> SortFields { get; set; } = new List<Sorting>();

        internal int? Skip { get; set; }
        internal int? Take { get; set; }


        int _page;
        /// <summary>
        /// 
        /// </summary>
        public int Page
        {
            get { return _page; }
            set
            {
                _page = value;
                pagingCalculations();
            }
        }

        int _itemsPerPage;
        /// <summary>
        /// 
        /// </summary>
        public int ItemsPerPage
        {
            get { return _itemsPerPage; }
            set
            {
                _itemsPerPage = value;
                pagingCalculations();
            }
        }

        int _limitUp;
        /// <summary>
        /// 
        /// </summary>
        public int LimitUp
        {
            get { return _limitUp; }
            set
            {
                _limitUp = value;
                pagingCalculations();
            }
        }
        int _pages;
        public int Pages { get { return _pages; } }

        #endregion

        #region Constructor
        /// <summary>
        /// Default Constructor (no paging or limits)
        /// </summary>
        public FindRequest()
        {
            Skip = null;
            Take = null;
        }
        /// <summary>
        /// Paging default limit constructor
        /// </summary>
        /// <param name="page">Number of page paging starts from page 1</param>
        /// <param name="itemsPerPage">Items per page</param>
        public FindRequest(int page, int itemsPerPage)
        {
            setPaging(page, itemsPerPage, Settings.Instance.ResultsLimit);
        }

        /// <summary>
        /// Custom constuctor
        /// </summary>
        /// <param name="page">Number of page paging starts from page 1</param>
        /// <param name="itemsPerPage">Items per page</param>
        /// <param name="limitUp">Limit search Up to items</param>
        public FindRequest(int page, int itemsPerPage, int limitUp)
        {
            setPaging(page, itemsPerPage, limitUp);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// set search request paging info
        /// </summary>
        /// <param name="page"></param>
        /// <param name="itemsPerPage"></param>
        /// <param name="limitUp"></param>
        void setPaging(int page, int itemsPerPage, int limitUp)
        {
            _page = page;
            _itemsPerPage = itemsPerPage;
            _limitUp = limitUp;
            pagingCalculations();
        }

        /// <summary>
        /// Calculate values of skip and take that will be send to MongoDB find
        /// </summary>
        void pagingCalculations()
        {
            var pg = _page - 1;
            if (pg < 0)
                return;
            Skip = pg * _itemsPerPage;
            _pages = (int)Math.Ceiling((double)_limitUp / _itemsPerPage);
            if (_pages == _page)
            {
                Take = _limitUp - (_pages - 1) * _itemsPerPage;
            }
            else
            {
                Take = _itemsPerPage;
            }
        }
        #endregion

        #region Search Information functionality
        /// <summary>
		/// Insert a search OR condition into the find request
        /// </summary>
        /// <returns>FindRequest with new search operator</returns>
		/// <param name="field">The search field</param>
		/// <param name="value">The query value</param>
		/// <param name="compare">Comparison between data and value to satisfy the criteria</param>
		public FindRequest<T> Or(Expression<Func<T, object>> field, object value, EnComparator compare = EnComparator.EqualTo)
        {
            var fieldName = Metadata.GetMemberInfo(field).Member.Name;
            Fields.Add(new QueryField(fieldName, value, EnOperator.Or, compare));
            return this;
        }
		/// <summary>
        /// Insert a search OR condition into the find request
        /// </summary>
        /// <returns>FindRequest with new search operator</returns>
        /// <param name="field">The search field</param>
        /// <param name="value">The query value</param>
        /// <param name="compare">Comparison between data and value to satisfy the criteria</param>
		public FindRequest<T> And(Expression<Func<T, object>> field, object value, EnComparator compare = EnComparator.EqualTo)
        {
            var fieldName = Metadata.GetMemberInfo(field).Member.Name;
            Fields.Add(new QueryField(fieldName, value, EnOperator.And, compare));
            return this;
        }
		/// <summary>
        /// Insert a search OR condition into the find request
        /// </summary>
        /// <returns>FindRequest with new search operator</returns>
        /// <param name="field">The search field</param>
        /// <param name="value">The query value</param>
        /// <param name="compare">Comparison between data and value to satisfy the criteria</param>
		public FindRequest<T> Not(Expression<Func<T, object>> field, object value, EnComparator compare = EnComparator.EqualTo)
        {
            var fieldName = Metadata.GetMemberInfo(field).Member.Name;
            Fields.Add(new QueryField(fieldName, value, EnOperator.Not, compare));
            return this;
        }
		/// <summary>
        /// Add a field into the sorting of result
        /// </summary>
		/// <returns>FindRequest with new sorting option</returns>
        /// <param name="field">The sort field</param>
        /// <param name="ascending">True if direction of sort is Asceding use false for Descending(Default is True)</param>
		public FindRequest<T> Sort(Expression<Func<T, object>> field, bool ascending = true)
        {
            var fieldName = Metadata.GetMemberInfo(field).Member.Name;
			SortFields.Add(new Sorting(fieldName, ascending));
            return this;
        }
        #endregion
    }
}
