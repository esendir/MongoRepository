using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Repository.Mongo
{
    /// <summary>
    /// mongo entity
    /// </summary>
    [BsonIgnoreExtraElements(Inherited = true)]
    public class Entity : IEntity
    {
        /// <summary>
        /// create date
        /// </summary>
        public DateTime CreatedOn
        {
            get
            {
                return ObjectId.CreationTime;
            }
        }

        /// <summary>
        /// id in string format
        /// </summary>
        [BsonElement(Order = 0)]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        /// <summary>
        /// modify date
        /// </summary>
        [BsonElement("_m", Order = 1)]
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime ModifiedOn { get; set; }

        /// <summary>
        /// id in objectId format
        /// </summary>
        public ObjectId ObjectId
        {
            get
            {
                //Incase, this is required before inserted into db
                if (Id == null)
                    Id = ObjectId.GenerateNewId().ToString();
                return ObjectId.Parse(Id);
            }
        }
    }
}