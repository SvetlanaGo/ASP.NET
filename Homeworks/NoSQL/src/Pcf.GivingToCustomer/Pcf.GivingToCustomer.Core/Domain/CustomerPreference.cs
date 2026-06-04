using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Pcf.GivingToCustomer.Core.Domain
{
    public class CustomerPreference
    {
        [BsonRepresentation(BsonType.String)]
        public Guid CustomerId { get; set; }

        [BsonIgnore]
        public virtual Customer Customer { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Guid PreferenceId { get; set; }
        public virtual Preference Preference { get; set; }
    }
}