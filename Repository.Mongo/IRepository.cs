using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Mongo
{
    public interface IRepository<T> where T : IEntity
    {
        #region MongoSpecific
        IMongoCollection<T> Collection { get; }
        FilterDefinitionBuilder<T> Filter { get; }
        UpdateDefinitionBuilder<T> Updater { get; }
        ProjectionDefinitionBuilder<T> Project { get; }
        #endregion MongoSpesific

        #region CRUD
        T Get(string id);
        IEnumerable<T> Find(Expression<Func<T, bool>> filter);
        IEnumerable<T> Find(Expression<Func<T, bool>> filter, int pageIndex, int size);
        IEnumerable<T> Find(Expression<Func<T, bool>> filter, Expression<Func<T, object>> order, int pageIndex, int size, bool descending = true);
        void Insert(T entity);
        void Insert(IEnumerable<T> entities);
        void Replace(T entity);
        void Replace(IEnumerable<T> entities);
        bool Update(T entity, UpdateDefinition<T> update);
        bool Update(FilterDefinition<T> filter, UpdateDefinition<T> update);
        bool Update<TField>(T entity, Expression<Func<T, TField>> field, TField value);
        bool Update<TField>(FilterDefinition<T> filter, Expression<Func<T, TField>> field, TField value);
        void Delete(string id);
        void Delete(T entity);
        void Delete(Expression<Func<T, bool>> filter);
        #endregion CRUD

        #region Simplicity
        bool Any(Expression<Func<T, bool>> filter);
        FilterDefinition<T> EntityFilter(string id, Expression<Func<T, bool>> filter);
        #endregion Simplicity

    }
}
