using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vb.Mongo.Engine.Db;

namespace Vb.Mongo.Engine.IdGenerators
{
    public class AutoIncrementGenerator : IIdGenerator
    {
        private string CollectionName { get; }
        MongoBuilder Builder { get; }
        //multi-threading lock
        private readonly object locker = new object();
        internal AutoIncrementGenerator(MongoBuilder builder,string collectionName)
        {
            CollectionName = collectionName;
            Builder = builder;
        }

        protected object ConvertToInt(BsonValue value)
        {
            return value.AsInt64;
        }

        public bool IsEmpty(object id)
        {
            return ((long)id) == 0;
        }

        public object GenerateId(object container, object document)
        {
            var generated = 0L;
            lock (locker)
            {
                using (var ctx = Builder.SequenceContext())
                {
                    var repo = ctx.CreateRepository<AutoIncrement>(x => x.Id);
                    repo.UniqueIndex("AutoIncrementCollection", x => x.CollectionName);
                    var sequence = repo.Find(x => x.CollectionName == CollectionName).FirstOrDefault();
                    if (sequence == null)
                    {
                        generated = 1;
                        sequence = new AutoIncrement
                        {
                            Id = new ObjectId(),
                            CollectionName = CollectionName,
                            Sequence = 2
                        };
                        repo.Store(sequence);
                        return generated;
                    }
                    else
                    {
                        generated = sequence.Sequence;
                        sequence.Sequence++;
                        repo.Replace(sequence);
                        return generated;
                    }
                }
            }
        }
    }
}
