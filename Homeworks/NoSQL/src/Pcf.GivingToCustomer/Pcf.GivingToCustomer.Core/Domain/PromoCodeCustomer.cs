using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Pcf.GivingToCustomer.Core.Domain
{
    public class PromoCodeCustomer : BaseEntity
    {
        [BsonRepresentation(BsonType.String)]
        public Guid PromoCodeId { get; set; }

        [BsonIgnore]
        public virtual PromoCode PromoCode { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Guid CustomerId { get; set; }

        [BsonIgnore]
        public virtual Customer Customer { get; set; }
    }
}
