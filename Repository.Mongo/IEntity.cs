using System;
// Nuget MongoDB
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Repository.Mongo
{
    /// <summary>
    /// mongo entity interface
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// create date
        /// </summary>
        DateTime CreatedOn { get; }

        /// <summary>
        /// id in string format
        /// </summary>
        [BsonId]
        string Id { get; set; }

        /// <summary>
        /// modify date
        /// </summary>
        DateTime ModifiedOn { get; }

        /// <summary>
        /// id in objectId format
        /// </summary>
        [BsonIgnore]
        ObjectId ObjectId { get; }
    }
}