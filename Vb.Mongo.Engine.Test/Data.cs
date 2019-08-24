using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Vb.Mongo.Engine.Test
{
    class TestItem
    {
        [BsonIgnoreIfDefault]
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public string FieldA { get; set; }
        public int Weight { get; set; }
        public int TestId { get; set; }
        public IList<Child> Children { get; set; }
    }
    class Child
    {
        public string Name { get; set; }
    }
}
