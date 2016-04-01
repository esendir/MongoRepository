using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Repository.Mongo
{
    /// <summary>
    /// mongo based repository interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRepository<T> where T : IEntity
    {
        #region MongoSpecific

        /// <summary>
        /// mongo collection
        /// </summary>
        IMongoCollection<T> Collection { get; }

        /// <summary>
        /// filter for collection
        /// </summary>
        FilterDefinitionBuilder<T> Filter { get; }

        /// <summary>
        /// projector for collection
        /// </summary>
        ProjectionDefinitionBuilder<T> Project { get; }

        /// <summary>
        /// updater for collection
        /// </summary>
        UpdateDefinitionBuilder<T> Updater { get; }

        #endregion MongoSpecific

        #region CRUD

        /// <summary>
        /// delete by id
        /// </summary>
        /// <param name="id">id</param>
        void Delete(string id);

        /// <summary>
        /// delete entity
        /// </summary>
        /// <param name="entity">entity</param>
        void Delete(T entity);

        /// <summary>
        /// delete items with filter
        /// </summary>
        /// <param name="filter">expression filter</param>
        void Delete(Expression<Func<T, bool>> filter);

        /// <summary>
        /// find entities
        /// </summary>
        /// <param name="filter">expression filter</param>
        /// <returns>collection of entity</returns>
        IEnumerable<T> Find(Expression<Func<T, bool>> filter);

        /// <summary>
        /// find entities with paging
        /// </summary>
        /// <param name="filter">expression filter</param>
        /// <param name="pageIndex">page index, based on 0</param>
        /// <param name="size">number of items in page</param>
        /// <returns>collection of entity</returns>
        IEnumerable<T> Find(Expression<Func<T, bool>> filter, int pageIndex, int size);

        /// <summary>
        /// find entities with paging and ordering
        /// </summary>
        /// <param name="filter">expression filter</param>
        /// <param name="order">ordering parameters</param>
        /// <param name="pageIndex">page index, based on 0</param>
        /// <param name="size">number of items in page</param>
        /// <returns>collection of entity</returns>
        IEnumerable<T> Find(Expression<Func<T, bool>> filter, Expression<Func<T, object>> order, int pageIndex, int size);

        /// <summary>
        /// find entities with paging and ordering in direction
        /// </summary>
        /// <param name="filter">expression filter</param>
        /// <param name="order">ordering parameters</param>
        /// <param name="pageIndex">page index, based on 0</param>
        /// <param name="size">number of items in page</param>
        /// <param name="isDescending">ordering direction</param>
        /// <returns>collection of entity</returns>
        IEnumerable<T> Find(Expression<Func<T, bool>> filter, Expression<Func<T, object>> order, int pageIndex, int size, bool isDescending);

        /// <summary>
        /// get by id
        /// </summary>
        /// <param name="id">id value</param>
        /// <returns>entity of <typeparamref name="T"/></returns>
        T Get(string id);

        /// <summary>
        /// insert entity
        /// </summary>
        /// <param name="entity">entity</param>
        void Insert(T entity);

        /// <summary>
        /// insert entity collection
        /// </summary>
        /// <param name="entities">collection of entities</param>
        void Insert(IEnumerable<T> entities);

        /// <summary>
        /// replace an existing entity
        /// </summary>
        /// <param name="entity">entity</param>
        void Replace(T entity);

        /// <summary>
        /// replace collection of entities
        /// </summary>
        /// <param name="entities">collection of entities</param>
        void Replace(IEnumerable<T> entities);

        /// <summary>
        /// update an entity with updated fields
        /// </summary>
        /// <param name="entity">entity</param>
        /// <param name="update">updated field(s)</param>
        /// <returns>true if successful, otherwise false</returns>
        bool Update(T entity, UpdateDefinition<T> update);

        /// <summary>
        /// update found entities by filter with updated fields
        /// </summary>
        /// <param name="filter">collection filter</param>
        /// <param name="update">updated field(s)</param>
        /// <returns>true if successful, otherwise false</returns>
        bool Update(FilterDefinition<T> filter, UpdateDefinition<T> update);

        /// <summary>
        /// update a property field in an entity
        /// </summary>
        /// <typeparam name="TField">field type</typeparam>
        /// <param name="entity">entity</param>
        /// <param name="field">field</param>
        /// <param name="value">new value</param>
        /// <returns>true if successful, otherwise false</returns>
        bool Update<TField>(T entity, Expression<Func<T, TField>> field, TField value);

        /// <summary>
        /// update a property field in entities
        /// </summary>
        /// <typeparam name="TField">field type</typeparam>
        /// <param name="filter">filter</param>
        /// <param name="field">field</param>
        /// <param name="value">new value</param>
        /// <returns>true if successful, otherwise false</returns>
        bool Update<TField>(FilterDefinition<T> filter, Expression<Func<T, TField>> field, TField value);

        #endregion CRUD

        #region Simplicity

        /// <summary>
        /// validate if filter result exists
        /// </summary>
        /// <param name="filter"></param>
        /// <returns>true if exists, otherwise false</returns>
        bool Any(Expression<Func<T, bool>> filter);

        #endregion Simplicity
    }
}