using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CotikBotik
{
    public class User
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string login { get; set; }
        public long chatId { get; set; }
    }
}