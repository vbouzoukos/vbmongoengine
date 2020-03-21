using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace Vb.Mongo.Engine.IdGenerators
{
    internal class AutoIncrement
    {
        public ObjectId Id { get; set; }
        public long Sequence { get; set; }
        public string CollectionName { get; set; }
    }
}
