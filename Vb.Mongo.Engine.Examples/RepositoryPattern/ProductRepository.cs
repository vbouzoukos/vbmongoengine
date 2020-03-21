using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Vb.Mongo.Engine.Examples.Data;

namespace Vb.Mongo.Engine.Examples.RepositoryPattern
{
    class ProductRepository : IRepository<Product>
    {
        public void Create(Product entity)
        {
            throw new NotImplementedException();
        }

        public void Create(IEnumerable<Product> entities)
        {
            throw new NotImplementedException();
        }

        public void Delete(Expression<Func<Product, bool>> expression)
        {
            throw new NotImplementedException();
        }

        public void Delete(Product entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(IEnumerable<Product> entities)
        {
            throw new NotImplementedException();
        }

        public void DeleteAll()
        {
            throw new NotImplementedException();
        }

        public IQueryable<Product> FindAll()
        {
            throw new NotImplementedException();
        }

        public IQueryable<Product> FindByCondition(Expression<Func<Product, bool>> expression)
        {
            throw new NotImplementedException();
        }

        public Product FindById(object id)
        {
            throw new NotImplementedException();
        }

        public void Update(Product entity)
        {
            throw new NotImplementedException();
        }

        public void Update(IEnumerable<Product> entities)
        {
            throw new NotImplementedException();
        }
    }
}
