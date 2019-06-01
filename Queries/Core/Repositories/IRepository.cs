using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Queries.Core.Repositories
{
    public interface IRepository<TEntity> where TEntity : class
    {
        // Here we have three groups of methods:

        // The first group is for finding objects;
        TEntity Get(int id);
        IEnumerable<TEntity> GetAll();
        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);
        TEntity SingleOrDefault(Expression<Func<TEntity, bool>> predicate);

        // The second group is for adding objects;
        void Add(TEntity entity);
        void AddRange(IEnumerable<TEntity> entities);

        // The third group is for removing objects;
        void Remove(TEntity entity);
        void RemoveRange(IEnumerable<TEntity> entities);
    }
}
