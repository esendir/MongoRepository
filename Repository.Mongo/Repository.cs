using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Mongo
{
    public class Repository<T> : IRepository<T>
        where T : IEntity
    {
        #region MongoSpecific
        public Repository(string connectionString)
        {
            Collection = Database<T>.GetCollectionFromConnectionString(connectionString);
        }

        public IMongoCollection<T> Collection
        {
            get; private set;
        }

        public FilterDefinitionBuilder<T> Filter
        {
            get
            {
                return Builders<T>.Filter;
            }
        }

        public UpdateDefinitionBuilder<T> Updater
        {
            get
            {
                return Builders<T>.Update;
            }
        }

        public ProjectionDefinitionBuilder<T> Project
        {
            get
            {
                return Builders<T>.Projection;
            }
        }

        private IFindFluent<T, T> Query(Expression<Func<T, bool>> filter)
        {
            return Collection.Find(filter);
        }
        #endregion MongoSpecific

        #region CRUD
        public T Get(string id)
        {
            return Collection.Find(i => i.Id == id).FirstOrDefault();
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> filter)
        {
            return Query(filter).ToEnumerable();
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> filter, int pageIndex, int size)
        {
            return Find(filter, i => i.Id, pageIndex, size);
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> filter, Expression<Func<T, object>> order, int pageIndex, int size, bool descending = true)
        {
            var query = Query(filter).Skip(pageIndex * size).Limit(size);
            return (descending ? query.SortByDescending(order) : query.SortBy(order)).ToEnumerable();
        }

        public void Insert(T entity)
        {
            Collection.InsertOne(entity);
        }

        public void Insert(IEnumerable<T> entities)
        {
            Collection.InsertMany(entities);
        }

        public void Replace(T entity)
        {
            Collection.ReplaceOne(i => i.Id == entity.Id, entity);
        }

        public void Replace(IEnumerable<T> entities)
        {
            foreach (T entity in entities)
            {
                Replace(entity);
            }
        }

        public bool Update(T entity, UpdateDefinition<T> update)
        {
            return Update(Filter.Eq(i => i.Id, entity.Id), update);
        }

        public bool Update(FilterDefinition<T> filter, UpdateDefinition<T> update)
        {
            return Collection.UpdateMany(filter, update.CurrentDate(i => i.ModifiedOn)).IsAcknowledged;
        }

        public bool Update<TField>(T entity, Expression<Func<T, TField>> field, TField value)
        {
            return Update(Filter.Eq(i => i.Id, entity.Id), field, value);
        }

        public bool Update<TField>(FilterDefinition<T> filter, Expression<Func<T, TField>> field, TField value)
        {
            return Update(filter, Updater.Set(field, value));
        }

        public void Delete(string id)
        {
            Collection.DeleteOne(i => i.Id == id);
        }

        public void Delete(T entity)
        {
            Delete(entity.Id);
        }

        public void Delete(Expression<Func<T, bool>> filter)
        {
            Collection.DeleteMany(filter);
        }
        #endregion CRUD

        #region Simplicity
        public bool Any(Expression<Func<T, bool>> filter)
        {
            return Collection.AsQueryable<T>().Any(filter);
        }
        public FilterDefinition<T> EntityFilter(string id, Expression<Func<T, bool>> filter)
        {
            return Filter.And(Filter.Eq(i => i.Id, id), Filter.Where(filter));
        }
        #endregion Simplicity
    }
}
