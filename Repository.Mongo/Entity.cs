using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Repository.Mongo
{
    /// <summary>
    /// Entity wrapper for non-edittable models
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Entity<T> : Entity
    {
        /// <summary>
        /// Generic content
        /// </summary>
        public T Content { get; set; }
    }

    /// <summary>
    /// mongo entity
    /// </summary>
    [BsonIgnoreExtraElements(Inherited = true)]
    public class Entity : IEntity
    {
        private DateTime _createdOn;

        /// <summary>
        /// Constructor, assigns new id
        /// </summary>
        public Entity()
        {
            Id = ObjectId.GenerateNewId().ToString();
        }

        /// <summary>
        /// id in string format
        /// </summary>
        [BsonElement(Order = 0)]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }


        /// <summary>
        /// create date
        /// </summary>
        [BsonElement("_c", Order = 1)]
        public DateTime CreatedOn
        {
            get
            {
                if (_createdOn == null || _createdOn == DateTime.MinValue)
                    _createdOn = ObjectId.CreationTime;
                return _createdOn;
            }
            set
            {
                _createdOn = value;
            }
        }

        /// <summary>
        /// modify date
        /// </summary>
        [BsonElement("_m", Order = 2)]
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime ModifiedOn { get; set; }

        /// <summary>
        /// id in objectId format
        /// </summary>
        public ObjectId ObjectId => ObjectId.Parse(Id);

    }
}