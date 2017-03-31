using MongoDB.Driver;
using System;
using System.Configuration;

namespace Repository.Mongo
{
    internal class Database<T> where T : IEntity
    {
        private Database()
        {
        }

        /// <summary>
        /// Creates and returns a MongoCollection from the specified type and connectionstring.
        /// </summary>
        /// <typeparam name="T">The type to get the collection of.</typeparam>
        /// <param name="connectionString">The connectionstring to use to get the collection from.</param>
        /// <returns>Returns a MongoCollection from the specified type and connectionstring.</returns>
        internal static IMongoCollection<T> GetCollectionFromConnectionString(string connectionString)
        {
            return GetCollectionFromConnectionString(connectionString, GetCollectionName());
        }

        /// <summary>
        /// Creates and returns a MongoCollection from the specified type and connectionstring.
        /// </summary>
        /// <typeparam name="T">The type to get the collection of.</typeparam>
        /// <param name="connectionString">The connectionstring to use to get the collection from.</param>
        /// <param name="collectionName">The name of the collection to use.</param>
        /// <returns>Returns a MongoCollection from the specified type and connectionstring.</returns>
        internal static IMongoCollection<T> GetCollectionFromConnectionString(string connectionString, string collectionName)
        {
            return GetCollectionFromUrl(new MongoUrl(connectionString), collectionName);
        }

        /// <summary>
        /// Creates and returns a MongoCollection from the specified type and url.
        /// </summary>
        /// <typeparam name="T">The type to get the collection of.</typeparam>
        /// <param name="url">The url to use to get the collection from.</param>
        /// <returns>Returns a MongoCollection from the specified type and url.</returns>
        internal static IMongoCollection<T> GetCollectionFromUrl(MongoUrl url)
        {
            return GetCollectionFromUrl(url, GetCollectionName());
        }

        /// <summary>
        /// Creates and returns a MongoCollection from the specified type and url.
        /// </summary>
        /// <typeparam name="T">The type to get the collection of.</typeparam>
        /// <param name="url">The url to use to get the collection from.</param>
        /// <param name="collectionName">The name of the collection to use.</param>
        /// <returns>Returns a MongoCollection from the specified type and url.</returns>
        internal static IMongoCollection<T> GetCollectionFromUrl(MongoUrl url, string collectionName)
        {
            return GetDatabaseFromUrl(url).GetCollection<T>(collectionName);
        }

        /// <summary>
        /// Creates and returns a MongoDatabase from the specified url.
        /// </summary>
        /// <param name="url">The url to use to get the database from.</param>
        /// <returns>Returns a MongoDatabase from the specified url.</returns>
        private static IMongoDatabase GetDatabaseFromUrl(MongoUrl url)
        {
            var client = new MongoClient(url);
            return client.GetDatabase(url.DatabaseName); // WriteConcern defaulted to Acknowledged
        }

        #region Collection Name

        /// <summary>
        /// Determines the collection name for T and assures it is not empty
        /// </summary>
        /// <typeparam name="T">The type to determine the collection name for.</typeparam>
        /// <returns>Returns the collection name for T.</returns>
        private static string GetCollectionName()
        {
            string collectionName;
            collectionName = typeof(T).BaseType.Equals(typeof(object)) ?
                                      GetCollectionNameFromInterface() :
                                      GetCollectionNameFromType();

            if (string.IsNullOrEmpty(collectionName))
            {
                collectionName = typeof(T).Name;
            }
            return collectionName.ToLowerInvariant();
        }

        /// <summary>
        /// Determines the collection name from the specified type.
        /// </summary>
        /// <typeparam name="T">The type to get the collection name from.</typeparam>
        /// <returns>Returns the collection name from the specified type.</returns>
        private static string GetCollectionNameFromInterface()
        {
            // Check to see if the object (inherited from Entity) has a CollectionName attribute
            var att = Attribute.GetCustomAttribute(typeof(T), typeof(CollectionNameAttribute));

            return att != null ? ((CollectionNameAttribute)att).Name : typeof(T).Name;
        }

        /// <summary>
        /// Determines the collectionname from the specified type.
        /// </summary>
        /// <param name="entitytype">The type of the entity to get the collectionname from.</param>
        /// <returns>Returns the collectionname from the specified type.</returns>
        private static string GetCollectionNameFromType()
        {
            Type entitytype = typeof(T);
            string collectionname;

            // Check to see if the object (inherited from Entity) has a CollectionName attribute
            var att = Attribute.GetCustomAttribute(entitytype, typeof(CollectionNameAttribute));
            if (att != null)
            {
                // It does! Return the value specified by the CollectionName attribute
                collectionname = ((CollectionNameAttribute)att).Name;
            }
            else
            {
                //if (typeof(Entity).IsAssignableFrom(entitytype))
                //{
                //    // No attribute found, get the basetype
                //    while (!entitytype.BaseType.Equals(typeof(Entity)))
                //    {
                //        entitytype = entitytype.BaseType;
                //    }
                //}
                collectionname = entitytype.Name;
            }

            return collectionname;
        }

        #endregion Collection Name

        #region Connection Name

        /// <summary>
        /// Determines the connection name for T and assures it is not empty
        /// </summary>
        /// <typeparam name="T">The type to determine the connection name for.</typeparam>
        /// <returns>Returns the connection name for T.</returns>
        private static string GetConnectionName()
        {
            string collectionName;
            collectionName = typeof(T).BaseType.Equals(typeof(object)) ?
                                      GetConnectionNameFromInterface() :
                                      GetConnectionNameFromType();

            if (string.IsNullOrEmpty(collectionName))
            {
                collectionName = typeof(T).Name;
            }
            return collectionName.ToLowerInvariant();
        }

        /// <summary>
        /// Determines the connection name from the specified type.
        /// </summary>
        /// <typeparam name="T">The type to get the connection name from.</typeparam>
        /// <returns>Returns the connection name from the specified type.</returns>
        private static string GetConnectionNameFromInterface()
        {
            // Check to see if the object (inherited from Entity) has a ConnectionName attribute
            var att = Attribute.GetCustomAttribute(typeof(T), typeof(ConnectionNameAttribute));

            return (att != null) ? ((ConnectionNameAttribute)att).Name : typeof(T).Name;
        }

        /// <summary>
        /// Determines the connection name from the specified type.
        /// </summary>
        /// <param name="entitytype">The type of the entity to get the connection name from.</param>
        /// <returns>Returns the connection name from the specified type.</returns>
        private static string GetConnectionNameFromType()
        {
            Type entitytype = typeof(T);
            string collectionname;

            // Check to see if the object (inherited from Entity) has a ConnectionName attribute
            var att = Attribute.GetCustomAttribute(entitytype, typeof(ConnectionNameAttribute));
            if (att != null)
            {
                // It does! Return the value specified by the ConnectionName attribute
                collectionname = ((ConnectionNameAttribute)att).Name;
            }
            else
            {
                if (typeof(Entity).IsAssignableFrom(entitytype))
                {
                    // No attribute found, get the basetype
                    while (!entitytype.BaseType.Equals(typeof(Entity)))
                    {
                        entitytype = entitytype.BaseType;
                    }
                }
                collectionname = entitytype.Name;
            }

            return collectionname;
        }

        #endregion Connection Name

    }
}