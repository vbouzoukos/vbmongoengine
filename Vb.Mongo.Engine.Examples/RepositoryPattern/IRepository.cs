using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Vb.Mongo.Engine.Examples.RepositoryPattern
{
    /// <summary>
    /// Interface for repository pattern
    /// </summary>
    /// <typeparam name="T">Generic class</typeparam>
    public interface IRepository<T>
    {
        T FindById(object id);
        IQueryable<T> FindAll();
        IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression);
        void Create(T entity);
        void Create(IEnumerable<T> entities);
        void Update(T entity);
        void Update(IEnumerable<T> entities);
        void Delete(Expression<Func<T, bool>> expression);
        void Delete(T entity);
        void Delete(IEnumerable<T> entities);
        void DeleteAll();
    }
}
