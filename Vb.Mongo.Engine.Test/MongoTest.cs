using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vb.Mongo.Engine.Db;
using Vb.Mongo.Engine.Find;
using Xunit;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using Vb.Mongo.Engine.IdGenerators;

namespace Vb.Mongo.Engine.Test
{
    public class MongoTest
    {
        readonly List<TestItem> testData;
        const string connectionString = "mongodb://localhost?replicaSet=rs0";//

        const string dbName = "test";
        const string dbSearchName = "testsearch";
        const string dbNameKeyVal = "testkeys";
        const string dbNameUknown = "testuknown";
        const string dbNameAsync = "testasync";
        const string dbNameUqIndx = "uniqidxtest";
        const string dbNameCustomId = "customId";
        const string dbNameSequence = "testseq";
        const string dbNameTransactions = "testtransactions";
        readonly MongoBuilder testBuilder;
        public MongoTest()
        {
            //Test data for search
            testData = new List<TestItem>
            {
                    new TestItem() { Id = new MongoDB.Bson.ObjectId(), Name = "beta", FieldA = "cccccc", Weight = 99 , TestId = 1 , Children=new List<Child>{ new Child() { Name ="Test"} } },
                    new TestItem() { Id = new MongoDB.Bson.ObjectId(), Name = "beta", FieldA = "dddddd", Weight = 20 , TestId = 2 ,Children=new List<Child>{ new Child() { Name ="A"} } },
                    new TestItem() { Id = new MongoDB.Bson.ObjectId(), Name = "beta", FieldA = "zzzzzz", Weight = 99 , TestId = 3 ,Children=new List<Child>{ new Child() { Name ="b"} } },
                    new TestItem() { Id = new MongoDB.Bson.ObjectId(), Name = "Alpha", FieldA = "item1", Weight = 10 , TestId = 4 ,Children=new List<Child>{ new Child() { Name ="c"} } },
                    new TestItem() { Id = new MongoDB.Bson.ObjectId(), Name = "beta", FieldA = "item2", Weight = 11 , TestId = 5 ,Children=new List<Child>{ new Child() { Name ="d"} } },
                    new TestItem() { Id = new MongoDB.Bson.ObjectId(), Name = "gamma", FieldA = "item3", Weight = 56 , TestId = 6 ,Children=new List<Child>{ new Child() { Name ="e"} } },
                    new TestItem() { Id = new MongoDB.Bson.ObjectId(), Name = "delta", FieldA = "item4", Weight = 65 , TestId = 7 ,Children=new List<Child>{ new Child() { Name ="f"} } },
                    new TestItem() { Id = new MongoDB.Bson.ObjectId(), Name = "epsilon", FieldA = "item5", Weight = 40, TestId = 8 ,Children=new List<Child>{ new Child() { Name ="g"} }  },
                    new TestItem() { Id = new MongoDB.Bson.ObjectId(), Name = "sdagddg", FieldA = "item5", Weight = 8 , TestId = 9 ,Children=new List<Child>{ new Child() { Name ="h"} } },
                    new TestItem() { Id = new MongoDB.Bson.ObjectId(), Name = "gfdgf", FieldA = "item5", Weight = 50 , TestId = 10 ,Children=new List<Child>{ new Child() { Name ="i"} } },
                    new TestItem() { Id = new MongoDB.Bson.ObjectId(), Name = "pbeyd", FieldA = "item2", Weight = 23 , TestId = 11 ,Children=new List<Child>{ new Child() { Name ="j"} } },
                    new TestItem() { Id = new MongoDB.Bson.ObjectId(), Name = "epsilon", FieldA = "aaaaa", Weight = 7, TestId = 12 ,Children=new List<Child>{ new Child() { Name ="k"} }  },
                    new TestItem() { Id = new MongoDB.Bson.ObjectId(), Name = "teran", FieldA = "qzetq", Weight = 1, TestId = 13 ,Children=new List<Child>{ new Child() { Name ="nest"} }  },
                    new TestItem() { Id = new MongoDB.Bson.ObjectId(), Name = "teran", FieldA = "aaaaa", Weight = 1, TestId = 14 ,Children=new List<Child>{ new Child() { Name ="lala"} }  }
           };

            testBuilder = new MongoBuilder(connectionString);

            testBuilder.AutoMap<TestItem>();
            testBuilder.AutoMap<Symbol>((a) =>
            {
                a.MapIdField(x => x.Code).SetIdGenerator(new StringObjectIdGenerator());
            });
            testBuilder.AutoMap<Protocol>((a) =>
            {
                a.MapIdField(x => x.Number).SetIdGenerator(testBuilder.CreateAutoIncrementGenerator(dbNameSequence));
            });

            using (var v = CreateSearchContext())
            {
                v.DropDatabase();
                var repo = v.CreateRepository<TestItem>(t => t.Id);
                repo.Index("weight", x => x.Weight);
                repo.Index("name", x => x.Name);
                repo.UniqueIndex("testid", x => x.TestId);
                repo.DeleteAll();
                repo.Store(testData);
            }
            using (var v = CreateContextCustomId())
            {
                v.DropDatabase();
            }
        }

        #region Contexts
        MongoContext CreateContext()
        {
            return testBuilder.CreateContext(dbName);
        }
        MongoContext CreateSearchContext()
        {
            return testBuilder.CreateContext(dbSearchName);
        }
        MongoContext CreateContextKeyValue()
        {
            return testBuilder.CreateContext(dbNameKeyVal);
        }
        MongoContext CreateContextUknown()
        {
            return testBuilder.CreateContext(dbNameUknown);
        }
        MongoContext CreateContextAsync()
        {
            return testBuilder.CreateContext(dbNameAsync);
        }
        MongoContext CreateContextUqIndx()
        {
            return testBuilder.CreateContext(dbNameUqIndx);
        }
        MongoContext CreateContextCustomId()
        {
            return testBuilder.CreateContext(dbNameCustomId);
        }
        MongoContext CreateContextSeq()
        {
            return testBuilder.CreateContext(dbNameCustomId);
        }
        MongoContext CreateContextTranactions()
        {
            return testBuilder.CreateContext(dbNameTransactions);
        }
        #endregion
        internal void SetObjectValue(string fieldName, object entity, object value)
        {

            var propertyInfo = entity.GetType().GetProperty(fieldName);
            propertyInfo.SetValue(entity, value, null);
        }
        public object ObjectValue(string fieldName, object entity)
        {
            var propertyInfo = entity.GetType().GetProperty(fieldName);
            var item = propertyInfo.GetValue(entity, null);
            return item;
        }

        [Fact]
        public void CRUD()
        {
            using (var dbCtx = CreateContext())
            {
                var repo = dbCtx.CreateRepository<TestItem>(t => t.Id);
                repo.DeleteAll();

                IList<TestItem> expected = new List<TestItem>
                    {
                        new TestItem()
                        {
                                Name = "beta",
                                FieldA = "cccccc",
                                Weight = 99 ,
                                TestId = 111 ,
                                Children =new List<Child>{ new Child() { Name ="Test"}
                        }
                        }
                    };

                //insert the item
                repo.Store(expected);
                //find the item
                {
                    var query = repo.CreateFindRequest();
                    query.Find(x => x.TestId, 111);
                    var result = query.Execute();
                    Assert.Equal(expected.Count, result.Count);
                    for (int i = 0; i < expected.Count; i++)
                    {
                        var original = expected[i];
                        var created = result[i];
                        Assert.Equal(original.Id, created.Id);
                        Assert.Equal(original.Name, created.Name);
                        Assert.Equal(original.FieldA, created.FieldA);
                        Assert.Equal(original.Weight, created.Weight);
                        Assert.Equal(original.Children[0].Name, created.Children[0].Name);
                    }
                    expected = result;
                }
                //update the item
                expected[0].Name = "omega";
                expected[0].FieldA = "omega";
                expected[0].Weight = 3;
                expected[0].Children = new List<Child>() { new Child() { Name = "Updated" }, new Child() { Name = "MoreChild" } };
                repo.Replace(expected[0]);
                {
                    var query = repo.CreateFindRequest();
                    query.Find(x => x.TestId, 111);
                    var result = query.Execute();
                    Assert.Equal(expected.Count, result.Count);
                    for (int i = 0; i < expected.Count; i++)
                    {
                        var original = expected[i];
                        var created = result[i];
                        Assert.Equal(original.Id, created.Id);
                        Assert.Equal(original.Name, created.Name);
                        Assert.Equal(original.FieldA, created.FieldA);
                        Assert.Equal(original.Weight, created.Weight);
                        Assert.Equal(original.Children[0].Name, created.Children[0].Name);
                    }
                }
                {
                    var query = repo.CreateFindRequest();
                    query.Find(x => x.TestId, 111);
                    repo.Delete(query);
                    var result = query.Execute();
                    Assert.Equal(0, result.Count);
                }
            }
        }

        [Fact]
        public void Bulk()
        {

            using (var dbCtx = CreateContext())
            {
                var repo = dbCtx.CreateRepository<TestItem>(t => t.Id);
                repo.DeleteAll();

                var bulk = new List<TestItem>();
                for (int i = 0; i < 5; i++)
                {
                    bulk.Add(new TestItem()
                    {
                        Name = "Insert",
                        FieldA = $"Insert{i + 1}",
                        Weight = 5001 + i,
                        TestId = i + 1,
                        Children = new List<Child> { new Child() { Name = string.Format("Test {0}", i + 1) } }
                    });
                }
                repo.Bulk(bulk);

                var query = repo.CreateFindRequest();
                query.And(x => x.Name, "Insert").Sort(x => x.Weight);
                var result = query.Execute();
                Assert.Equal(bulk.Count, result.Count);
                for (int i = 0; i < 5; i++)
                {
                    var original = bulk[i];
                    var created = result[i];
                    Assert.Equal(original.Id, created.Id);
                    Assert.Equal(original.Name, created.Name);
                    Assert.Equal(original.FieldA, created.FieldA);
                    Assert.Equal(original.Weight, created.Weight);
                    Assert.Equal(original.Children[0].Name, created.Children[0].Name);
                }

                var upsert = new List<TestItem>();
                result[2].Name = "Upsert";
                result[2].FieldA = "Updated 3";
                result[3].Name = "Upsert";
                result[3].FieldA = "Updated 4";
                upsert.Add(result[2]);
                upsert.Add(result[3]);
                for (int i = 5; i < 10; i++)
                {
                    upsert.Add(new TestItem()
                    {
                        Name = "Upsert",
                        FieldA = $"New Insert{i + 1}",
                        Weight = 5001 + i,
                        TestId = i + 1,
                        Children = new List<Child> { new Child() { Name = string.Format("Test {0}", i + 1) } }
                    });
                }
                repo.Bulk(upsert);

                query = repo.CreateFindRequest();
                query.And(x => x.Name, "Upsert").Sort(x => x.Weight);
                result = query.Execute();
                Assert.Equal(upsert.Count, result.Count);
                for (int i = 0; i < 7; i++)
                {
                    var original = upsert[i];
                    var created = result[i];
                    Assert.Equal(original.Id, created.Id);
                    Assert.Equal(original.Name, created.Name);
                    Assert.Equal(original.FieldA, created.FieldA);
                    Assert.Equal(original.Weight, created.Weight);
                    Assert.Equal(original.Children[0].Name, created.Children[0].Name);
                }
            }

        }

        [Fact]
        public void SearchWithAnd()
        {
            using (var dbCtx = CreateSearchContext())
            {
                var repo = dbCtx.CreateRepository<TestItem>(t => t.Id);

                var query = repo.CreateFindRequest();
                query.Find(x => x.Name, "beta");
                query.And(x => x.FieldA, "item2");
                query.Sort(x => x.Name);
                query.Sort(x => x.FieldA);
                var expected = testData.Where(x => x.Name == "beta" && x.FieldA == "item2").OrderBy(x => x.Name).ThenBy(x => x.FieldA).ToList();
                var result = query.Execute();

                Assert.Equal(expected.Count, result.Count);
                for (int i = 0; i < expected.Count; i++)
                {
                    var original = expected[i];
                    var created = result[i];
                    Assert.Equal(original.Id, created.Id);
                    Assert.Equal(original.Name, created.Name);
                    Assert.Equal(original.FieldA, created.FieldA);
                    Assert.Equal(original.Weight, created.Weight);
                    Assert.Equal(original.Children[0].Name, created.Children[0].Name);
                }
            }
        }

        [Fact]
        public void SearchWithOr()
        {
            using (var dbCtx = CreateSearchContext())
            {
                var repo = dbCtx.CreateRepository<TestItem>(t => t.Id);

                var query = repo.CreateFindRequest();
                query.Or(x => x.Name, "beta");
                query.Or(x => x.FieldA, "item5");
                query.Sort(x => x.Name);
                query.Sort(x => x.FieldA);
                var expected = testData.Where(x => x.Name == "beta" || x.FieldA == "item5").OrderBy(x => x.Name).ThenBy(x => x.FieldA).ToList();
                var result = query.Execute();
                Assert.Equal(expected.Count, result.Count);
                for (int i = 0; i < expected.Count; i++)
                {
                    var original = expected[i];
                    var created = result[i];
                    Assert.Equal(original.Id, created.Id);
                    Assert.Equal(original.Name, created.Name);
                    Assert.Equal(original.FieldA, created.FieldA);
                    Assert.Equal(original.Weight, created.Weight);
                    Assert.Equal(original.Children[0].Name, created.Children[0].Name);
                }
            }
        }

        [Fact]
        public void SearchWithNot()
        {
            using (var dbCtx = CreateSearchContext())
            {
                var repo = dbCtx.CreateRepository<TestItem>(t => t.Id);

                var query = repo.CreateFindRequest();
                query.Not(x => x.Name, "beta");
                query.Not(x => x.FieldA, "item5");
                query.Sort(x => x.TestId);
                var expected = testData.Where(x => x.Name != "beta" && x.FieldA != "item5").OrderBy(x => x.TestId).ToList();
                var result = query.Execute();
                Assert.Equal(expected.Count, result.Count);
                for (int i = 0; i < expected.Count; i++)
                {
                    var original = expected[i];
                    var created = result[i];
                    Assert.Equal(original.Id, created.Id);
                    Assert.Equal(original.Name, created.Name);
                    Assert.Equal(original.FieldA, created.FieldA);
                    Assert.Equal(original.Weight, created.Weight);
                    Assert.Equal(original.Children[0].Name, created.Children[0].Name);
                }
            }
        }

        [Fact]
        public void SearchWithLike()
        {
            using (var dbCtx = CreateSearchContext())
            {
                var repo = dbCtx.CreateRepository<TestItem>(t => t.Id);

                var query = repo.CreateFindRequest();
                query.Find(x => x.Name, "be", EnComparator.Like);
                query.Sort(x => x.Name);
                query.Sort(x => x.FieldA);
                var expected = testData.Where(x => x.Name.Contains("be")).OrderBy(x => x.Name).ThenBy(x => x.FieldA).ToList();
                var result = query.Execute();
                Assert.Equal(expected.Count, result.Count);
                for (int i = 0; i < expected.Count; i++)
                {
                    var original = expected[i];
                    var created = result[i];
                    Assert.Equal(original.Id, created.Id);
                    Assert.Equal(original.Name, created.Name);
                    Assert.Equal(original.FieldA, created.FieldA);
                    Assert.Equal(original.Weight, created.Weight);
                    Assert.Equal(original.Children[0].Name, created.Children[0].Name);
                }
            }
        }

        [Fact]
        public void SearchWithGreaterThan()
        {

            using (var dbCtx = CreateSearchContext())
            {
                var repo = dbCtx.CreateRepository<TestItem>(t => t.Id);

                var query = repo.CreateFindRequest();
                query.Find(x => x.Weight, 50, EnComparator.GreaterThan);
                query.Sort(x => x.Name);
                query.Sort(x => x.FieldA);
                var expected = testData.Where(x => x.Weight > 50).OrderBy(x => x.Name).ThenBy(x => x.FieldA).ToList();
                var result = query.Execute();
                Assert.Equal(expected.Count, result.Count);
                for (int i = 0; i < expected.Count; i++)
                {
                    var original = expected[i];
                    var created = result[i];
                    Assert.Equal(original.Id, created.Id);
                    Assert.Equal(original.Name, created.Name);
                    Assert.Equal(original.FieldA, created.FieldA);
                    Assert.Equal(original.Weight, created.Weight);
                    Assert.Equal(original.Children[0].Name, created.Children[0].Name);
                }
            }
        }

        [Fact]
        public void SearchWithLessThan()
        {
            using (var dbCtx = CreateSearchContext())
            {
                var repo = dbCtx.CreateRepository<TestItem>(t => t.Id);

                var query = repo.CreateFindRequest();
                query.Find(x => x.Weight, 50, EnComparator.LessThan);
                query.Sort(x => x.Name);
                query.Sort(x => x.FieldA);
                var expected = testData.Where(x => x.Weight < 50).OrderBy(x => x.Name).ThenBy(x => x.FieldA).ToList();
                var result = query.Execute();
                Assert.Equal(expected.Count, result.Count);
                for (int i = 0; i < expected.Count; i++)
                {
                    var original = expected[i];
                    var created = result[i];
                    Assert.Equal(original.Id, created.Id);
                    Assert.Equal(original.Name, created.Name);
                    Assert.Equal(original.FieldA, created.FieldA);
                    Assert.Equal(original.Weight, created.Weight);
                    Assert.Equal(original.Children[0].Name, created.Children[0].Name);
                }
            }
        }

        [Fact]
        public void SearchNested()
        {
            using (var dbCtx = CreateSearchContext())
            {
                var repo = dbCtx.CreateRepository<TestItem>(t => t.Id);

                var query = repo.CreateFindRequest();
                query.Find(x => x.Children[0].Name, "Test");
                var expected = testData.Where(x => x.Children[0].Name == "Test").OrderBy(x => x.Name).ThenBy(x => x.FieldA).ToList();
                var result = query.Execute();
                Assert.Equal(expected.Count, result.Count);
                for (int i = 0; i < expected.Count; i++)
                {
                    var original = expected[i];
                    var created = result[i];
                    Assert.Equal(original.Id, created.Id);
                    Assert.Equal(original.Name, created.Name);
                    Assert.Equal(original.FieldA, created.FieldA);
                    Assert.Equal(original.Weight, created.Weight);
                    Assert.Equal(original.Children[0].Name, created.Children[0].Name);
                }
            }
        }

        [Fact]
        public void SearchMixed()
        {
            using (var dbCtx = CreateSearchContext())
            {
                var repo = dbCtx.CreateRepository<TestItem>(t => t.Id);

                var query = repo.CreateFindRequest();
                query.Find(x => x.Name, "teran");
                query.And(x => x.Children[0].Name, "nest");
                query.Or(x => x.FieldA, "zet", EnComparator.Like);
                query.Or(x => x.TestId, 12, EnComparator.GreaterThan);
                query.Or(x => x.Weight, 10, EnComparator.LessThan);
                query.Sort(x => x.TestId);
                var expected = testData.Where(x => (x.Name == "teran" && x.Children[0].Name == "nest") || x.FieldA.Contains("zet") || x.TestId > 12 || x.Weight < 10).OrderBy(x => x.TestId).ToList();
                var result = query.Execute();
                Assert.Equal(expected.Count, result.Count);
                for (int i = 0; i < expected.Count; i++)
                {
                    var original = expected[i];
                    var created = result[i];
                    Assert.Equal(original.Id, created.Id);
                    Assert.Equal(original.Name, created.Name);
                    Assert.Equal(original.FieldA, created.FieldA);
                    Assert.Equal(original.Weight, created.Weight);
                    Assert.Equal(original.Children[0].Name, created.Children[0].Name);
                }
            }

        }

        [Fact]
        public void UniqueIndex()
        {
            Assert.Throws<MongoBulkWriteException<TestItem>>(() =>
            {
                using (var dbCtx = CreateContextUqIndx())
                {
                    dbCtx.DropDatabase();
                    dbCtx.CreateCollectionIfNotExist(nameof(TestItem));

                    var repo = dbCtx.CreateRepository<TestItem>(t => t.Id);

                    repo.UniqueIndex("TestId", x => x.TestId);
                    repo.Store(testData);
                    var dbl = new List<TestItem>
                    {
                      new TestItem()
                      {
                          Id = new MongoDB.Bson.ObjectId(),
                          Name = "fail",
                          FieldA = "fail",
                          Weight = 1,
                          TestId = 14,
                          Children =new List<Child>
                          {
                              new Child()
                              {
                                  Name ="lala"
                              }
                          }
                      }
                    };
                    repo.Store(dbl);
                }
            });
        }

        [Fact]
        public void StoreKeyValue()
        {
            var result = Record.Exception(() =>
            {
                using (var dbCtx = CreateContextKeyValue())
                {
                    var repo = dbCtx.CreateRepository<object>(dbNameKeyVal);
                    repo.DeleteAll();
                    var obj = new System.Dynamic.ExpandoObject();
                    var props = ((IDictionary<string, object>)obj);
                    props.Add("LogType", "Info");
                    props.Add("Message", "Test key value 2");
                    repo.Store(new List<object> { obj });
                }
            });
            Assert.Null(result);
        }

        [Fact]
        public void StoreUnknownType()
        {
            var result = Record.Exception(() =>
            {
                using (var dbCtx = CreateContextKeyValue())
                {
                    var repo = dbCtx.CreateRepository<object>(dbNameKeyVal);
                    repo.DeleteAll();
                    repo.Store(new List<object> { new { LogType = "Info", Message = "This is a test" } });
                }
            });
            Assert.Null(result);
        }

        [Fact]
        public void SearchInUkknown()
        {
            using (var dbCtx = CreateContextKeyValue())
            {
                var repo = dbCtx.CreateRepository<object>(dbNameKeyVal);
                repo.DeleteAll();
                var obj = new { Message = "This is a test" };
                repo.Store(new List<object> { obj });
                var query = repo.CreateFindRequest();
                query.Find("Message", "This is a test");
                var result = query.Execute();
                Assert.Equal(1, result.Count);
                var created = result[0];
                var val = ExpressionGererator.ObjectValue("Message", created);
                Assert.Equal("This is a test", val);
            }
        }

        [Fact]
        public void CRUDUknown()
        {
            using (var dbCtx = CreateContextUknown())
            {
                dbCtx.CreateCollectionIfNotExist(dbNameUknown);
                var repo = dbCtx.CreateRepository<dynamic>(dbNameUknown);
                repo.DeleteAll();
                //this is the final result
                var expected = new List<dynamic>
                {
                    new { LogType = "no change", Message = "first", TestId = 111 },
                    new { LogType = "Replaced", Message = "second", TestId = 111 },
                    new { LogType = "New", Message = "third", TestId = 111 }
                };

                var init = new List<dynamic>
                {
                    new {LogType = "no change", Message = "first", TestId = 111 },
                    new { LogType = "lallala", Message = "second", TestId = 111 }
                };

                //insert the item
                repo.Store(init);
                // dbCtx.CommitTransaction();
                var q2 = repo.CreateFindRequest();
                q2.Find("TestId", 111);
                var replace = q2.Execute();
                Assert.Equal(init.Count, replace.Count);
                try
                {
                    dbCtx.BeginTransaction();
                    replace[1].LogType = "Replaced";
                    repo.Replace(init[1]);
                    var newobj = new System.Dynamic.ExpandoObject();
                    var props = ((IDictionary<string, object>)newobj);
                    props.Add("LogType", "New");
                    props.Add("Message", "third");
                    props.Add("TestId", 111);

                    replace.Add(newobj);
                    repo.Bulk(replace);
                    dbCtx.CommitTransaction();
                }
                catch
                {
                    dbCtx.RollbackTransaction();
                    throw;
                }

                var query = repo.CreateFindRequest();
                query.Find("TestId", 111);
                var result = query.Execute();
                Assert.Equal(expected.Count, result.Count);
                for (int i = 0; i < expected.Count; i++)
                {
                    var original = ObjectValue("LogType", expected[i]);
                    var created = result[i].LogType;
                    Assert.Equal(original, created);
                }
                repo.Delete(query);
                result = query.Execute();
                Assert.Equal(0, result.Count);
            }
        }

        [Fact]
        public void CRUDCustomId()
        {
            using (var dbCtx = CreateContextCustomId())
            {
                dbCtx.CreateCollectionIfNotExist(dbNameCustomId);
                var repo = dbCtx.CreateRepository<Symbol>(x => x.Code, dbNameCustomId);
                //this is the final result
                var expected = new List<Symbol>
                {
                    new Symbol{ Code = "A", Caption = "Alpha" },
                    new Symbol{ Code = "B", Caption = "Beta" },
                    new Symbol{ Code = "C", Caption = "Ce" }
                };

                var init = new List<Symbol>
                {
                    new Symbol{ Code = "A", Caption = "Alpha" },
                    new Symbol{ Code = "B", Caption = "Be" },
                };

                //insert the item
                try
                {
                    dbCtx.BeginTransaction();
                    repo.Store(init);
                    init[1].Caption = "Beta";
                    init.Add(new Symbol { Code = "C", Caption = "Ce" });
                    repo.Bulk(init);
                    dbCtx.CommitTransaction();
                }
                catch
                {
                    dbCtx.RollbackTransaction();
                    throw;
                }

                var result = repo.AllData().ToList();
                Assert.Equal(expected.Count, result.Count);
                for (int i = 0; i < expected.Count; i++)
                {
                    var original = expected[i];
                    var created = result[i];
                    Assert.Equal(original.Caption, created.Caption);
                    Assert.Equal(original.Code, created.Code);
                }
                repo.DeleteAll();
                result = repo.AllData().ToList();
                Assert.Empty(result);
            }
        }

        [Fact]
        public void AsyncTest()
        {
            var task = new Task(async () =>
            {
                using (var dbCtx = CreateContextAsync())
                {
                    dbCtx.CreateCollectionIfNotExist<TestItem>();
                    var repo = dbCtx.CreateRepository<TestItem>(t => t.Id);
                    try
                    {
                        dbCtx.BeginTransaction();
                        await repo.DeleteAllAsync();
                        await repo.StoreAsync(testData);
                        dbCtx.CommitTransaction();
                    }
                    catch
                    {
                        dbCtx.RollbackTransaction();
                        throw;
                    }
                    var query = repo.CreateFindRequest();
                    query.Find(x => x.Weight, 50, EnComparator.LessThan);
                    query.Sort(x => x.Name);
                    query.Sort(x => x.FieldA);
                    var expected = testData.Where(x => x.Weight < 50).OrderBy(x => x.Name).ThenBy(x => x.FieldA).ToList();
                    var result = await query.ExecuteAsync();
                    Assert.Equal(expected.Count, result.Count);
                    for (int i = 0; i < expected.Count; i++)
                    {
                        var original = expected[i];
                        var created = result[i];
                        Assert.Equal(original.Id, created.Id);
                        Assert.Equal(original.Name, created.Name);
                        Assert.Equal(original.FieldA, created.FieldA);
                        Assert.Equal(original.Weight, created.Weight);
                        Assert.Equal(original.Children[0].Name, created.Children[0].Name);
                    }
                }
            });
            task.RunSynchronously();
        }

        [Fact]
        public void SequenceTest()
        {
            using (var dbCtx = CreateContextSeq())
            {
                var repo = dbCtx.CreateRepository<Protocol>(x => x.Number, dbNameSequence);
                testBuilder.ResetSequence(dbNameSequence);
                //this is the final result
                var data =
                new List<Protocol>
                {
                    new Protocol{ Caption = "Alpha" },
                    new Protocol{ Caption = "Beta" },
                    new Protocol{ Caption = "Gamma" },
                    new Protocol{ Caption = "Delta" },
                    new Protocol{ Caption = "Epsilon" },
                    new Protocol{ Caption = "Zeta" },
                    new Protocol{ Caption = "Ita" },
                    new Protocol{ Caption = "Theta" },
               };


                //insert the item
                repo.Store(data);
                var query = repo.CreateFindRequest();
                query.Sort(x => x.Number);
                var result = query.Execute();
                for (int i = 1; i <= result.Count; i++)
                {
                    Assert.Equal(i, result[i - 1].Number);
                }
                repo.DeleteAll();

                testBuilder.ResetSequence(dbNameSequence);
                repo.Store(data);
                query = repo.CreateFindRequest();
                query.Sort(x => x.Number);
                result = query.Execute();
                for (int i = 1; i <= result.Count; i++)
                {
                    Assert.Equal(i, result[i - 1].Number);
                }
                repo.DeleteAll();
            }
        }
        [Fact]
        public void Transactions()
        {
            var task = new Task(async () =>
            {
                using (var dbCtx = CreateContext())
                {
                    var repo = dbCtx.CreateRepository<TestItem>(t => t.Id);
                    dbCtx.CreateCollectionIfNotExist<TestItem>();
                    try
                    {
                        IList<TestItem> expected = new List<TestItem>
                        {
                            new TestItem()
                            {
                                    Name = "beta",
                                    FieldA = "cccccc",
                                    Weight = 99 ,
                                    TestId = 111 ,
                                    Children =new List<Child>{ new Child() { Name ="Test"}
                            }
                            }
                        };
                        dbCtx.BeginTransaction();
                        //insert the item
                        await repo.StoreAsync(expected);
                        //find the item
                        {
                            var query = repo.CreateFindRequest();
                            query.Find(x => x.TestId, 111);
                            var result = await query.ExecuteAsync();
                            Assert.Equal(expected.Count, result.Count);
                            for (int i = 0; i < expected.Count; i++)
                            {
                                var original = expected[i];
                                var created = result[i];
                                Assert.Equal(original.Id, created.Id);
                                Assert.Equal(original.Name, created.Name);
                                Assert.Equal(original.FieldA, created.FieldA);
                                Assert.Equal(original.Weight, created.Weight);
                                Assert.Equal(original.Children[0].Name, created.Children[0].Name);
                            }
                            expected = result;
                        }
                        //update the item
                        expected[0].Name = "omega";
                        expected[0].FieldA = "omega";
                        expected[0].Weight = 3;
                        expected[0].Children = new List<Child>() { new Child() { Name = "Updated" }, new Child() { Name = "MoreChild" } };
                        await repo.ReplaceAsync(expected[0]);
                        {
                            var query = repo.CreateFindRequest();
                            query.Find(x => x.TestId, 111);
                            var result = query.Execute();
                            Assert.Equal(expected.Count, result.Count);
                            for (int i = 0; i < expected.Count; i++)
                            {
                                var original = expected[i];
                                var created = result[i];
                                Assert.Equal(original.Id, created.Id);
                                Assert.Equal(original.Name, created.Name);
                                Assert.Equal(original.FieldA, created.FieldA);
                                Assert.Equal(original.Weight, created.Weight);
                                Assert.Equal(original.Children[0].Name, created.Children[0].Name);
                            }
                        }
                        {
                            var query = repo.CreateFindRequest();
                            query.Find(x => x.TestId, 111);
                            await repo.DeleteAsync(query);
                            var result = await query.ExecuteAsync();
                            Assert.Equal(0, result.Count);
                        }
                        dbCtx.CommitTransaction();
                    }
                    catch
                    {
                        dbCtx.RollbackTransaction();
                    }
                }
            });
            task.RunSynchronously();
        }
    }
}
