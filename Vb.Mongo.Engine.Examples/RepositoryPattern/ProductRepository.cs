using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Vb.Mongo.Engine.Db;
using Vb.Mongo.Engine.Examples.Data;

namespace Vb.Mongo.Engine.Examples.RepositoryPattern
{
    class ProductRepository : IRepository<Product>
    {
        #region MongoRepository access
        /// <summary>
        /// MongoDB Repository access for CRUD operation
        /// </summary>
        /// <param name="crudOperation">Crud code block</param>
        void RepositoryAccess(Action<MongoRepository<Product>> crudOperation)
        {
            //Get examples engine context
            using (var ctx = ExampleEngine.Instance.CreateContext())
            {
                //Create the repository
                var repo = ctx.CreateRepository<Product>(x => x.Id);
                //execute the crud operation
                crudOperation(repo);
            }
        }
        /// <summary>
        /// MongoDB Repository access for search operation
        /// </summary>
        /// <param name="seekOperation">Search code block</param>
        /// <returns>Result set</returns>
        IQueryable<Product> RepositoryAccess(Func<MongoRepository<Product>, IQueryable<Product>> seekOperation)
        {
            //Get examples engine context
            using (var ctx = ExampleEngine.Instance.CreateContext())
            {
                //Create the repository
                var repo = ctx.CreateRepository<Product>(x => x.Id);
                return seekOperation(repo);
            }
        }
        /// <summary>
        /// MongoDB Repository access for search operation
        /// </summary>
        /// <param name="seekOperation">Search code block</param>
        /// <returns>Result product</returns>
        Product RepositoryAccess(Func<MongoRepository<Product>, Product> seekOperation)
        {
            //Get examples engine context
            using (var ctx = ExampleEngine.Instance.CreateContext())
            {
                //Create the repository
                var repo = ctx.CreateRepository<Product>(x => x.Id);
                return seekOperation(repo);
            }
        }
        #endregion

        #region Search

        /// <summary>
        /// Returns all items in collection
        /// </summary>
        /// <returns>Result set</returns>
        public IQueryable<Product> FindAll()
        {
            return RepositoryAccess((repo) =>
            {
                //MongoDB Driver Queryable return all the items stored in collection
                return repo.Queryable;
            });
        }

        /// <summary>
        /// Returns all items that match the query expression
        /// </summary>
        /// <param name="expression">Query expression</param>
        /// <returns>Result set</returns>
        public IQueryable<Product> FindByCondition(Expression<Func<Product, bool>> expression)
        {
            return RepositoryAccess((repo) =>
            {
                //Use find operation to search with an expression
                return repo.Find(expression);
            });
        }

        /// <summary>
        /// Finds an Object by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Result Product</returns>
        public Product FindById(object id)
        {
            return RepositoryAccess((repo) =>
            {
                //Use find operation to search with a given Id
                return repo.FindById(id);
            });
        }
        #endregion

        #region Creation
        /// <summary>
        /// Create method of repository Creates one entity
        /// </summary>
        /// <param name="entity">Entity to create</param>
        public void Create(Product entity)
        {
            RepositoryAccess((repo) =>
            {
                //Use store operation to store a single entity
                repo.Store(entity);
            });
        }
        /// <summary>
        /// Create method of repository Creates a set of entity
        /// </summary>
        /// <param name="entities">Set of entities to create</param>
        public void Create(IEnumerable<Product> entities)
        {
            RepositoryAccess((repo) =>
            {
                //Use store operation to store multiple entities
                repo.Store(entities);
            });
        }
        #endregion

        #region Update
        /// <summary>
        /// Updates a single entity
        /// </summary>
        /// <param name="entity">Entity with updated data</param>
        public void Update(Product entity)
        {
            RepositoryAccess((repo) =>
            {
                //Use replace operation to update an entity
                repo.Replace(entity);
            });
        }

        /// <summary>
        /// Updates a set of entities
        /// </summary>
        /// <param name="entity">Set of entities with updated data</param>
        public void Update(IEnumerable<Product> entities)
        {
            RepositoryAccess((repo) =>
            {
                //Use bulk operation to update multiple entities (this can be used to create new items too)
                repo.Bulk(entities);
            });
        }
        #endregion

        #region Deletion
        /// <summary>
        /// Deletes all entities that satisfy the condition of the goven expression
        /// </summary>
        /// <param name="expression">Query expression</param>
        public void Delete(Expression<Func<Product, bool>> expression)
        {
            RepositoryAccess((repo) =>
            {
                //Use delete operation to delete by an expression
                repo.Delete(expression);
            });
        }
        /// <summary>
        /// Deletes a given entity
        /// </summary>
        /// <param name="entity">Entity to delete</param>
        public void Delete(Product entity)
        {
            RepositoryAccess((repo) =>
            {
                //Use delete operation to delete a give entity
                repo.Delete(entity);
            });
        }
        /// <summary>
        /// Deletes a set of entities
        /// </summary>
        /// <param name="entities"></param>
        public void Delete(IEnumerable<Product> entities)
        {
            RepositoryAccess((repo) =>
            {
                //Use delete operation to delete a set of entities
                repo.BulkDelete(entities);
            });
        }

        /// <summary>
        /// Deletes all entities
        /// </summary>
        public void DeleteAll()
        {
            RepositoryAccess((repo) =>
            {
                //Use delete all operation to delete all entities in mongodb collection
                repo.DeleteAll();
            });
        }
        #endregion

    }
}
