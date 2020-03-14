using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
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
        MongoRepository<T> vRepository;

        int _page;
		/// <summary>
		/// Current query page
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
		/// Maximum result number
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
		internal FindRequest(MongoRepository<T> context)
		{
			Skip = null;
			Take = null;
            vRepository = context;

        }

        /// <summary>
        /// Custom constuctor
        /// </summary>
        /// <param name="page">Number of page paging starts from page 1</param>
        /// <param name="itemsPerPage">Items per page</param>
        /// <param name="limitUp">Limit search Up to items</param>
        internal FindRequest(MongoRepository<T> context, int page, int itemsPerPage, int limitUp)
		{
            vRepository = context;
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
        public FindRequest<T> Find(Expression<Func<T, object>> field, object value, EnComparator compare = EnComparator.EqualTo)
        {
            var fieldName = Reflection.GetMemberInfo(field).Member.Name;
            Fields.Add(new QueryField(fieldName, value, EnOperator.Find, compare));
            return this;
        }
        public FindRequest<T> Find(string fieldName, object value, EnComparator compare = EnComparator.EqualTo)
        {
            Fields.Add(new QueryField(fieldName, value, EnOperator.Find, compare));
            return this;
        }
        /// <summary>
        /// Insert a search OR condition into the find request
        /// </summary>
        /// <returns>FindRequest with new search operator</returns>
        /// <param name="field">The search field</param>
        /// <param name="value">The query value</param>
        /// <param name="compare">Comparison between data and value to satisfy the criteria</param>
        public FindRequest<T> Or(Expression<Func<T, object>> field, object value, EnComparator compare = EnComparator.EqualTo)
		{
			var fieldName = Reflection.GetMemberInfo(field).Member.Name;
			Fields.Add(new QueryField(fieldName, value, EnOperator.Or, compare));
			return this;
		}
        public FindRequest<T> Or(string fieldName, object value, EnComparator compare = EnComparator.EqualTo)
        {
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
			var fieldName = Reflection.GetMemberInfo(field).Member.Name;
			Fields.Add(new QueryField(fieldName, value, EnOperator.And, compare));
			return this;
		}
        public FindRequest<T> And(string fieldName, object value, EnComparator compare = EnComparator.EqualTo)
        {
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
			var fieldName = Reflection.GetMemberInfo(field).Member.Name;
			Fields.Add(new QueryField(fieldName, value, EnOperator.Not, compare));
			return this;
		}
        public FindRequest<T> Not(string fieldName, object value, EnComparator compare = EnComparator.EqualTo)
        {
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
			var fieldName = Reflection.GetMemberInfo(field).Member.Name;
			SortFields.Add(new Sorting(fieldName, ascending));
			return this;
		}
        public FindRequest<T> Sort(string fieldName, bool ascending = true)
        {
            SortFields.Add(new Sorting(fieldName, ascending));
            return this;
        }
        #endregion
        #region MongoDb Definitions (Filter Sort)

        /// <summary>
        /// Generate a filter definition from a Query Information
        /// </summary>
        /// <returns>MongoDB Filter definition for T</returns>
        internal FilterDefinition<T> buildFilterDefinition()
		{
			var filter = Builders<T>.Filter;
			FilterDefinition<T> filterDef = null;
			foreach (var criteria in Fields)
			{
				FilterDefinition<T> token = null;
				switch (criteria.Compare)
				{
					case EnComparator.Like:
						token = filter.Regex(criteria.Field, BsonRegularExpression.Create(criteria.Value));
						break;
					case EnComparator.GreaterThan:
						token = filter.Gt(criteria.Field, BsonValue.Create(criteria.Value));
						break;
					case EnComparator.LessThan:
						token = filter.Lt(criteria.Field, BsonValue.Create(criteria.Value));
						break;
					default:
						token = filter.Eq(criteria.Field, BsonValue.Create(criteria.Value));
						break;
				}
				switch (criteria.Operator)
				{
                    case EnOperator.And:
                    case EnOperator.Find:
                        {
                            if (filterDef == null)
							{
								filterDef = filter.And(token);
							}
							else
							{
								filterDef &= token;
							}
						}
						break;
					case EnOperator.Or:
						{
							if (filterDef == null)
							{
								filterDef = filter.Or(token);
							}
							else
							{
								filterDef |= token;
							}
						}
						break;
					case EnOperator.Not:
						{
							if (filterDef == null)
							{
								filterDef = filter.Not(token);
							}
							else
							{
								filterDef &= filter.Not(token);
							}
						}
						break;
				}
			}
			if (filterDef == null)
				filterDef = filter.Empty;
			return filterDef;
		}

		/// <summary>
		/// Generate a sort definition from a Query Information
		/// </summary>
		/// <returns>MongoDB Filter definition for T</returns>
		internal SortDefinition<T> buildSortingDefinition()
		{
			SortDefinition<T> sortDef = null;

			if (SortFields.Count > 0)
			{
				var sortBuilder = Builders<T>.Sort;
				foreach (var sortField in SortFields)
				{
					if (sortDef == null)
					{
						sortDef = (sortField.Ascending) ? sortBuilder.Ascending(sortField.Field) : sortBuilder.Descending(sortField.Field);
					}
					else
					{
						sortDef = (sortField.Ascending) ? sortDef.Ascending(sortField.Field) : sortDef.Descending(sortField.Field);
					}
				}
			}
			return sortDef;
		}
		#endregion

		#region Search
        /// <summary>
        /// Executes the find request async.
        /// </summary>
        /// <returns>Search result</returns>
        /// <param name="dbName">Db name.</param>
		public async Task<IList<T>> ExecuteAsync()
		{
			return await vRepository.SearchAsync(this);
		}

		/// <summary>
        /// Executes the find request.
        /// </summary>
        /// <returns>Search result</returns>
        /// <param name="dbName">Db name.</param>
		public IList<T> Execute()
        {
            return vRepository.Search(this);
        }
		#endregion
	}
}
