﻿using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Generic;
// Nuget Microsoft
using Microsoft.Extensions.Configuration;
// Nuget Polly
using Polly;
using Polly.Retry;
// Nuget MongoDB
using MongoDB.Driver;

namespace Repository.Mongo
{
    /// <summary>
    /// repository implementation for mongo
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Repository<T> : IRepository<T>
        where T : IEntity
    {
        #region MongoSpecific
        private IMongoCollection<T> _collection;
        private readonly IMongoDatabase _mongoDatabase;
        private readonly string _connectionString;
        private readonly string _collectionName;
        private readonly IConfiguration _config;
        private readonly IMongoClient _mongoClient;
        private readonly string _databaseName;

        /// <summary>
        /// if you already have mongo database and where collection name will be name of the repository
        /// </summary>
        /// <param name="mongoDatabase"></param>
        public Repository(IMongoDatabase mongoDatabase): this(mongoDatabase, null)
        {
        }

        /// <summary>
        /// if you already have mongo database
        /// </summary>
        /// <param name="mongoDatabase"></param>
        /// <param name="collectionName"></param>
        public Repository(IMongoDatabase mongoDatabase, string collectionName)
        {
            _mongoDatabase = mongoDatabase;
            _collectionName = collectionName;
        }

        /// <summary>
        /// if you already have mongo client and where collection name will be name of the repository
        /// </summary>
        /// <param name="mongoClient">mongo client object</param>
        /// <param name="databaseName">database name</param>
        public Repository(IMongoClient mongoClient, string databaseName) : this(mongoClient, databaseName, null)
        {
        }

        /// <summary>
        /// if you already have mongo client
        /// </summary>
        /// <param name="mongoClient">mongo client object</param>
        /// <param name="databaseName">database name</param>
        /// <param name="collectionName">collection name</param>
        public Repository(IMongoClient mongoClient, string databaseName, string collectionName)
        {
            _mongoClient = mongoClient;
            _databaseName = databaseName;
            _collectionName = collectionName;
        }

        /// <summary>
        /// where you need to define a connectionString with the name of repository
        /// </summary>
        /// <param name="config">config interface to read default settings</param>
        public Repository(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// where collection name will be name of the repository
        /// </summary>
        /// <param name="connectionString">connection string</param>
        public Repository(string connectionString): this(connectionString, null)
        {
        }

        /// <summary>
        /// with custom settings
        /// </summary>
        /// <param name="connectionString">connection string</param>
        /// <param name="collectionName">collection name</param>
        public Repository(string connectionString, string collectionName)
        {
            _connectionString = connectionString;
            _collectionName = collectionName;
        }

        private IMongoCollection<T> GetCollection()
        {
            if(_mongoDatabase != null)
            {
                return Database<T>.GetCollection(_mongoDatabase, _collectionName);
            }

            if(_mongoClient != null)
            {
                return Database<T>.GetCollection(_mongoClient, _databaseName, _collectionName);
            }

            if(_config != null)
            {
                return Database<T>.GetCollection(_config);
            }

            return Database<T>.GetCollectionFromConnectionString(_connectionString, _collectionName);
        }

        /// <summary>
        /// mongo collection
        /// </summary>
        public IMongoCollection<T> Collection
        {
            get
            {
                if (_collection == null)
                    _collection = GetCollection();
                return _collection;

            }
        }

        /// <summary>
        /// filter for collection
        /// </summary>
        public FilterDefinitionBuilder<T> Filter
        {
            get
            {
                return Builders<T>.Filter;
            }
        }

        /// <summary>
        /// projector for collection
        /// </summary>
        public ProjectionDefinitionBuilder<T> Project
        {
            get
            {
                return Builders<T>.Projection;
            }
        }

        /// <summary>
        /// updater for collection
        /// </summary>
        public UpdateDefinitionBuilder<T> Updater
        {
            get
            {
                return Builders<T>.Update;
            }
        }

        private IFindFluent<T, T> Query(FilterDefinition<T> filter)
        {
            return Collection.Find(filter);
        }

        private IFindFluent<T, T> Query(Expression<Func<T, bool>> filter)
        {
            return Collection.Find(filter);
        }

        private IFindFluent<T, T> Query()
        {
            return Collection.Find(Filter.Empty);
        }

        #endregion MongoSpecific

        #region CRUD

        #region Delete

        /// <summary>
        /// delete entity
        /// </summary>
        /// <param name="entity">entity</param>
        public bool Delete(T entity)
        {
            return Delete(entity.Id);
        }

        /// <summary>
        /// delete entity
        /// </summary>
        /// <param name="entity">entity</param>
        public Task<bool> DeleteAsync(T entity)
        {
            return Task.Run(() =>
            {
                return Delete(entity);
            });
        }

        /// <summary>
        /// delete by id
        /// </summary>
        /// <param name="id">id</param>
        public virtual bool Delete(string id)
        {
            return Retry(() =>
            {
                return Collection.DeleteOne(i => i.Id == id).IsAcknowledged;
            });
        }

        /// <summary>
        /// delete by id
        /// </summary>
        /// <param name="id">id</param>
        public virtual Task<bool> DeleteAsync(string id)
        {
            return Retry(() =>
            {
                return Task.Run(() =>
                {
                    return Delete(id);
                });
            });
        }

        /// <summary>
        /// delete items with filter
        /// </summary>
        /// <param name="filter">expression filter</param>
        public bool Delete(Expression<Func<T, bool>> filter)
        {
            return Retry(() =>
            {
                return Collection.DeleteMany(filter).IsAcknowledged;
            });
        }

        /// <summary>
        /// delete items with filter
        /// </summary>
        /// <param name="filter">expression filter</param>
        public Task<bool> DeleteAsync(Expression<Func<T, bool>> filter)
        {
            return Retry(() =>
            {
                return Task.Run(() =>
                {
                    return Delete(filter);
                });
            });
        }

        /// <summary>
        /// delete all documents
        /// </summary>
        public virtual bool DeleteAll()
        {
            return Retry(() =>
            {
                return Collection.DeleteMany(Filter.Empty).IsAcknowledged;
            });
        }

        /// <summary>
        /// delete all documents
        /// </summary>
        public virtual Task<bool> DeleteAllAsync()
        {
            return Retry(() =>
            {
                return Task.Run(() =>
                {
                    return DeleteAll();
                });
            });
        }
        #endregion Delete
        
        #region Find
        /// <summary>
        /// find entities
        /// </summary>
        /// <param name="filter">expression filter</param>
        /// <returns>collection of entity</returns>
        public virtual IEnumerable<T> Find(FilterDefinition<T> filter)
        {
            return Query(filter).ToEnumerable();
        }

        /// <summary>
        /// find entities
        /// </summary>
        /// <param name="filter">expression filter</param>
        /// <returns>collection of entity</returns>
        public virtual IEnumerable<T> Find(Expression<Func<T, bool>> filter)
        {
            return Query(filter).ToEnumerable();
        }

        /// <summary>
        /// find entities with paging
        /// </summary>
        /// <param name="filter">expression filter</param>
        /// <param name="pageIndex">page index, based on 0</param>
        /// <param name="size">number of items in page</param>
        /// <returns>collection of entity</returns>
        public IEnumerable<T> Find(Expression<Func<T, bool>> filter, int pageIndex, int size)
        {
            return Find(filter, i => i.Id, pageIndex, size);
        }

        /// <summary>
        /// find entities with paging and ordering
        /// default ordering is descending
        /// </summary>
        /// <param name="filter">expression filter</param>
        /// <param name="order">ordering parameters</param>
        /// <param name="pageIndex">page index, based on 0</param>
        /// <param name="size">number of items in page</param>
        /// <returns>collection of entity</returns>
        public IEnumerable<T> Find(Expression<Func<T, bool>> filter, Expression<Func<T, object>> order, int pageIndex, int size)
        {
            return Find(filter, order, pageIndex, size, true);
        }

        /// <summary>
        /// find entities with paging and ordering in direction
        /// </summary>
        /// <param name="filter">expression filter</param>
        /// <param name="order">ordering parameters</param>
        /// <param name="pageIndex">page index, based on 0</param>
        /// <param name="size">number of items in page</param>
        /// <param name="isDescending">ordering direction</param>
        /// <returns>collection of entity</returns>
        public virtual IEnumerable<T> Find(Expression<Func<T, bool>> filter, Expression<Func<T, object>> order, int pageIndex, int size, bool isDescending)
        {
            return Retry(() =>
            {
                var query = Query(filter).Skip(pageIndex * size).Limit(size);
                return (isDescending ? query.SortByDescending(order) : query.SortBy(order)).ToEnumerable();
            });
        }

        #endregion Find

        #region FindAll

        /// <summary>
        /// fetch all items in collection
        /// </summary>
        /// <returns>collection of entity</returns>
        public virtual IEnumerable<T> FindAll()
        {
            return Retry(() =>
            {
                return Query().ToEnumerable();
            });
        }

        /// <summary>
        /// fetch all items in collection with paging
        /// </summary>
        /// <param name="pageIndex">page index, based on 0</param>
        /// <param name="size">number of items in page</param>
        /// <returns>collection of entity</returns>
        public IEnumerable<T> FindAll(int pageIndex, int size)
        {
            return FindAll(i => i.Id, pageIndex, size);
        }

        /// <summary>
        /// fetch all items in collection with paging and ordering
        /// default ordering is descending
        /// </summary>
        /// <param name="order">ordering parameters</param>
        /// <param name="pageIndex">page index, based on 0</param>
        /// <param name="size">number of items in page</param>
        /// <returns>collection of entity</returns>
        public IEnumerable<T> FindAll(Expression<Func<T, object>> order, int pageIndex, int size)
        {
            return FindAll(order, pageIndex, size, true);
        }

        /// <summary>
        /// fetch all items in collection with paging and ordering in direction
        /// </summary>
        /// <param name="order">ordering parameters</param>
        /// <param name="pageIndex">page index, based on 0</param>
        /// <param name="size">number of items in page</param>
        /// <param name="isDescending">ordering direction</param>
        /// <returns>collection of entity</returns>
        public virtual IEnumerable<T> FindAll(Expression<Func<T, object>> order, int pageIndex, int size, bool isDescending)
        {
            return Retry(() =>
            {
                var query = Query().Skip(pageIndex * size).Limit(size);
                return (isDescending ? query.SortByDescending(order) : query.SortBy(order)).ToEnumerable();
            });
        }

        #endregion FindAll

        #region First

        /// <summary>
        /// get first item in collection
        /// </summary>
        /// <returns>entity of <typeparamref name="T"/></returns>
        public T First()
        {
            return FindAll(i => i.Id, 0, 1, false).FirstOrDefault();
        }

        /// <summary>
        /// get first item in query
        /// </summary>
        /// <param name="filter">expression filter</param>
        /// <returns>entity of <typeparamref name="T"/></returns>
        public T First(FilterDefinition<T> filter)
        {
            return Find(filter).FirstOrDefault();
        }

        /// <summary>
        /// get first item in query
        /// </summary>
        /// <param name="filter">expression filter</param>
        /// <returns>entity of <typeparamref name="T"/></returns>
        public T First(Expression<Func<T, bool>> filter)
        {
            return First(filter, i => i.Id);
        }

        /// <summary>
        /// get first item in query with order
        /// </summary>
        /// <param name="filter">expression filter</param>
        /// <param name="order">ordering parameters</param>
        /// <returns>entity of <typeparamref name="T"/></returns>
        public T First(Expression<Func<T, bool>> filter, Expression<Func<T, object>> order)
        {
            return First(filter, order, false);
        }

        /// <summary>
        /// get first item in query with order and direction
        /// </summary>
        /// <param name="filter">expression filter</param>
        /// <param name="order">ordering parameters</param>
        /// <param name="isDescending">ordering direction</param>
        /// <returns>entity of <typeparamref name="T"/></returns>
        public T First(Expression<Func<T, bool>> filter, Expression<Func<T, object>> order, bool isDescending)
        {
            return Find(filter, order, 0, 1, isDescending).FirstOrDefault();
        }

        #endregion First

        #region Get

        /// <summary>
        /// get by id
        /// </summary>
        /// <param name="id">id value</param>
        /// <returns>entity of <typeparamref name="T"/></returns>
        public virtual T Get(string id)
        {
            return Retry(() =>
            {
                return Find(i => i.Id == id).FirstOrDefault();
            });
        }

        #endregion Get

        #region Insert

        /// <summary>
        /// insert entity
        /// </summary>
        /// <param name="entity">entity</param>
        public virtual void Insert(T entity)
        {
            Retry(() =>
            {
                Collection.InsertOne(entity);
                return true;
            });
        }

        /// <summary>
        /// insert entity
        /// </summary>
        /// <param name="entity">entity</param>
        public virtual Task InsertAsync(T entity)
        {
            return Retry(() =>
            {
                return Collection.InsertOneAsync(entity);
            });
        }

        /// <summary>
        /// insert entity collection
        /// </summary>
        /// <param name="entities">collection of entities</param>
        public virtual void Insert(IEnumerable<T> entities)
        {
            Retry(() =>
            {
                Collection.InsertMany(entities);
                return true;
            });
        }

        /// <summary>
        /// insert entity collection
        /// </summary>
        /// <param name="entities">collection of entities</param>
        public virtual Task InsertAsync(IEnumerable<T> entities)
        {
            return Retry(() =>
            {
                return Collection.InsertManyAsync(entities);
            });
        }
        #endregion Insert

        #region Last

        /// <summary>
        /// get first item in collection
        /// </summary>
        /// <returns>entity of <typeparamref name="T"/></returns>
        public T Last()
        {
            return FindAll(i => i.Id, 0, 1, true).FirstOrDefault();
        }

        /// <summary>
        /// get last item in query
        /// </summary>
        /// <param name="filter">expression filter</param>
        /// <returns>entity of <typeparamref name="T"/></returns>
        public T Last(FilterDefinition<T> filter)
        {
            return Query(filter).SortByDescending(i => i.Id).FirstOrDefault();
        }

        /// <summary>
        /// get last item in query
        /// </summary>
        /// <param name="filter">expression filter</param>
        /// <returns>entity of <typeparamref name="T"/></returns>
        public T Last(Expression<Func<T, bool>> filter)
        {
            return Last(filter, i => i.Id);
        }

        /// <summary>
        /// get last item in query with order
        /// </summary>
        /// <param name="filter">expression filter</param>
        /// <param name="order">ordering parameters</param>
        /// <returns>entity of <typeparamref name="T"/></returns>
        public T Last(Expression<Func<T, bool>> filter, Expression<Func<T, object>> order)
        {
            return Last(filter, order, false);
        }

        /// <summary>
        /// get last item in query with order and direction
        /// </summary>
        /// <param name="filter">expression filter</param>
        /// <param name="order">ordering parameters</param>
        /// <param name="isDescending">ordering direction</param>
        /// <returns>entity of <typeparamref name="T"/></returns>
        public T Last(Expression<Func<T, bool>> filter, Expression<Func<T, object>> order, bool isDescending)
        {
            return First(filter, order, !isDescending);
        }

        #endregion Last

        #region Replace

        /// <summary>
        /// replace an existing entity
        /// </summary>
        /// <param name="entity">entity</param>
        public virtual bool Replace(T entity)
        {
            return Retry(() =>
            {
                return Collection.ReplaceOne(i => i.Id == entity.Id, entity).IsAcknowledged;
            });
        }

        /// <summary>
        /// replace an existing entity
        /// </summary>
        /// <param name="entity">entity</param>
        public virtual Task<bool> ReplaceAsync(T entity)
        {
            return Retry(() =>
            {
                return Task.Run(() =>
                {
                    return Replace(entity);
                });
            });
        }

        /// <summary>
        /// replace collection of entities
        /// </summary>
        /// <param name="entities">collection of entities</param>
        public void Replace(IEnumerable<T> entities)
        {
            foreach (T entity in entities)
            {
                Replace(entity);
            }
        }

        #endregion Replace

        #region FindOneAndUpdate
        
        /// <summary>
        /// find one and update
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="update"></param>
        /// <param name="options"></param>
        /// <returns>return updated entity</returns>
        public T FindOneAndUpdate(FilterDefinition<T> filter, UpdateDefinition<T> update, FindOneAndUpdateOptions<T> options = null)
        {
            return Collection.FindOneAndUpdate(filter, update, options);
        }

        #endregion FindOneAndUpdate

        #region Update

        /// <summary>
        /// update a property field in an entity
        /// </summary>
        /// <typeparam name="TField">field type</typeparam>
        /// <param name="entity">entity</param>
        /// <param name="field">field</param>
        /// <param name="value">new value</param>
        /// <returns>true if successful, otherwise false</returns>
        public bool Update<TField>(T entity, Expression<Func<T, TField>> field, TField value)
        {
            return Update(entity, Updater.Set(field, value));
        }

        /// <summary>
        /// update a property field in an entity
        /// </summary>
        /// <typeparam name="TField">field type</typeparam>
        /// <param name="entity">entity</param>
        /// <param name="field">field</param>
        /// <param name="value">new value</param>
        public Task<bool> UpdateAsync<TField>(T entity, Expression<Func<T, TField>> field, TField value)
        {
            return Task.Run(() =>
            {
                return Update(entity, Updater.Set(field, value));
            });
        }

        /// <summary>
        /// update an entity with updated fields
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="updates">updated field(s)</param>
        /// <returns>true if successful, otherwise false</returns>
        public virtual bool Update(string id, params UpdateDefinition<T>[] updates)
        {
            return Update(Filter.Eq(i => i.Id, id), updates);
        }

        /// <summary>
        /// update an entity with updated fields
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="updates">updated field(s)</param>
        public virtual Task<bool> UpdateAsync(string id, params UpdateDefinition<T>[] updates)
        {
            return Task.Run(() =>
            {
                return Update(Filter.Eq(i => i.Id, id), updates);
            });
        }

        /// <summary>
        /// update an entity with updated fields
        /// </summary>
        /// <param name="entity">entity</param>
        /// <param name="updates">updated field(s)</param>
        /// <returns>true if successful, otherwise false</returns>
        public virtual bool Update(T entity, params UpdateDefinition<T>[] updates)
        {
            return Update(entity.Id, updates);
        }

        /// <summary>
        /// update an entity with updated fields
        /// </summary>
        /// <param name="entity">entity</param>
        /// <param name="updates">updated field(s)</param>
        public virtual Task<bool> UpdateAsync(T entity, params UpdateDefinition<T>[] updates)
        {
            return Task.Run(() =>
            {
                return Update(entity.Id, updates);
            });
        }

        /// <summary>
        /// update a property field in entities
        /// </summary>
        /// <typeparam name="TField">field type</typeparam>
        /// <param name="filter">filter</param>
        /// <param name="field">field</param>
        /// <param name="value">new value</param>
        /// <returns>true if successful, otherwise false</returns>
        public bool Update<TField>(FilterDefinition<T> filter, Expression<Func<T, TField>> field, TField value)
        {
            return Update(filter, Updater.Set(field, value));
        }

        /// <summary>
        /// update a property field in entities
        /// </summary>
        /// <typeparam name="TField">field type</typeparam>
        /// <param name="filter">filter</param>
        /// <param name="field">field</param>
        /// <param name="value">new value</param>
        public Task<bool> UpdateAsync<TField>(FilterDefinition<T> filter, Expression<Func<T, TField>> field, TField value)
        {
            return Task.Run(() =>
            {
                return Update(filter, Updater.Set(field, value));
            });
        }

        /// <summary>
        /// update found entities by filter with updated fields
        /// </summary>
        /// <param name="filter">collection filter</param>
        /// <param name="updates">updated field(s)</param>
        /// <returns>true if successful, otherwise false</returns>
        public bool Update(FilterDefinition<T> filter, params UpdateDefinition<T>[] updates)
        {
            return Retry(() =>
            {
                var update = Updater.Combine(updates).CurrentDate(i => i.ModifiedOn);
                return Collection.UpdateMany(filter, update.CurrentDate(i => i.ModifiedOn)).IsAcknowledged;
            });
        }

        /// <summary>
        /// update found entities by filter with updated fields
        /// </summary>
        /// <param name="filter">collection filter</param>
        /// <param name="updates">updated field(s)</param>
        public Task<bool> UpdateAsync(FilterDefinition<T> filter, params UpdateDefinition<T>[] updates)
        {
            return Retry(() =>
            {
                return Task.Run(() =>
                {
                    return Update(filter, updates);
                });
            });
        }

        /// <summary>
        /// update found entities by filter with updated fields
        /// </summary>
        /// <param name="filter">collection filter</param>
        /// <param name="updates">updated field(s)</param>
        /// <returns>true if successful, otherwise false</returns>
        public bool Update(Expression<Func<T, bool>> filter, params UpdateDefinition<T>[] updates)
        {
            return Retry(() =>
            {
                var update = Updater.Combine(updates).CurrentDate(i => i.ModifiedOn);
                return Collection.UpdateMany(filter, update).IsAcknowledged;
            });
        }

        /// <summary>
        /// update found entities by filter with updated fields
        /// </summary>
        /// <param name="filter">collection filter</param>
        /// <param name="updates">updated field(s)</param>
        public Task<bool> UpdateAsync(Expression<Func<T, bool>> filter, params UpdateDefinition<T>[] updates)
        {
            return Retry(() =>
            {
                return Task.Run(() =>
                {
                    return Update(filter, updates);
                });
            });
        }

        #endregion Update

        #endregion CRUD

        #region Utils

        /// <summary>
        /// validate if filter result exists
        /// </summary>
        /// <param name="filter">expression filter</param>
        /// <returns>true if exists, otherwise false</returns>
        public bool Any(Expression<Func<T, bool>> filter)
        {
            return Retry(() =>
            {
                return First(filter) != null;
            });
        }

        #region Count
        /// <summary>
        /// get number of filtered documents
        /// </summary>
        /// <param name="filter">expression filter</param>
        /// <returns>returns the count of documents that match the query for a collection or view.</returns>
        public long Count(Expression<Func<T, bool>> filter)
        {
            return Retry(() =>
            {
                return Collection.CountDocuments(filter);
            });
        }

        /// <summary>
        /// get number of filtered documents async
        /// </summary>
        /// <param name="filter">expression filter</param>
        /// <returns>returns the count of documents that match the query for a collection or view.</returns>
        public Task<long> CountAsync(Expression<Func<T, bool>> filter)
        {
            return Retry(() =>
            {
                return Collection.CountDocumentsAsync(filter);
            });
        }
        #endregion Count

        #region EstimatedCount
        /// <summary>
        /// get number of all documents in collection
        /// </summary>
        /// <param name="options">count options</param>
        /// <returns>returns the count of all documents in a collection or view with count options.</returns>        
        public long EstimatedCount(EstimatedDocumentCountOptions options)
        {
            return Retry(() =>
            {
                return Collection.EstimatedDocumentCount(options);
            });
        }

        /// <summary>
        /// get number of all documents in collection
        /// </summary>
        /// <param name="options">count options</param>
        /// <returns>returns the count of all documents in a collection or view with count options.</returns>        
        public Task<long> EstimatedCountAsync(EstimatedDocumentCountOptions options)
        {
            return Retry(() =>
            {
                return Collection.EstimatedDocumentCountAsync(options);
            });
        }

        /// <summary>
        /// get number of all documents in collection
        /// </summary>
        /// <returns>returns the count of all documents in a collection or view.</returns>
        public long EstimatedCount()
        {
            return Retry(() =>
            {
                return Collection.EstimatedDocumentCount();
            });
        }

        /// <summary>
        /// get number of all documents in collection
        /// </summary>
        /// <returns>returns the count of all documents in a collection or view.</returns>
        public Task<long> EstimatedCountAsync()
        {
            return Retry(() =>
            {
                return Collection.EstimatedDocumentCountAsync();
            });
        }
        #endregion Estimated Count

        #endregion Utils

        #region RetryPolicy
        /// <summary>
        /// retry operation for three times if IOException occurs
        /// </summary>
        /// <typeparam name="TResult">return type</typeparam>
        /// <param name="action">action</param>
        /// <returns>action result</returns>
        /// <example>
        /// return Retry(() => 
        /// { 
        ///     do_something;
        ///     return something;
        /// });
        /// </example>
        protected virtual TResult Retry<TResult>(Func<TResult> action)
        {
            return RetryPolicy
                .Handle<MongoConnectionException>(i => i.InnerException.GetType() == typeof(IOException) ||
                                                       i.InnerException.GetType() == typeof(SocketException))
                .Retry(3)
                .Execute(action);
        }
        #endregion
    }
}