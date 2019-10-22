using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vb.Mongo.Engine.Db;
using Vb.Mongo.Engine.Find;
using Xunit;
using MongoDB.Driver;

namespace Vb.Mongo.Engine.Test
{
    public class MongoTest
    {
        Container<TestItem> test;
        List<TestItem> testData;
        public MongoTest()
        {
            Settings.StartUp("mongodb://localhost");
            test = new Container<TestItem>("test");
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
                   };//
        }

        [Fact]
        public void CRUD()
        {
            Settings.Instance.DropDatabase("test");
            var task = new Task(async () =>
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

                //insert the item
                await test.StoreAsync(expected);
                //find the item
                {
                    var query = new FindRequest<TestItem>();
                    query.Find(x => x.TestId, 111);
                    var result = await test.SearchAsync(query);
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
                await test.ReplaceAsync(x => x.Id, expected[0]);
                {
                    var query = new FindRequest<TestItem>();
                    query.Find(x => x.TestId, 111);
                    var result = await test.SearchAsync(query);
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
                    var query = new FindRequest<TestItem>();
                    query.Find(x => x.TestId, 111);
                    await test.DeleteAsync(query);
                    var result = await test.SearchAsync(query);
                    Assert.Equal(0, result.Count);
                }
            });
            task.Start();
        }
        [Fact]
        public void Bulk()
        {
            Settings.Instance.DropDatabase("test");
            var task = new Task(async () =>
            {
                var bulk = new List<TestItem>();
                for (int i = 0; i < 5; i++)
                {
                    bulk.Add(new TestItem()
                    {
                        Name = "Upsert",
                        FieldA = $"Insert{i + 1}",
                        Weight = 5001 + i,
                        Children = new List<Child> { new Child() { Name = string.Format("Test {0}", i + 1) } }
                    });
                }
                await test.BulkAsync(x => x.Id, bulk);
                var query = new FindRequest<TestItem>();
                query.And(x => x.Name, "Upsert").Sort(x => x.Weight);
                var result = await test.SearchAsync(query);
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
                        Children = new List<Child> { new Child() { Name = string.Format("Test {0}", i + 1) } }
                    });
                }
                query = new FindRequest<TestItem>();
                query.And(x => x.Name, "Upsert").Sort(x => x.Weight);
                result = await test.SearchAsync(query);
                Assert.Equal(upsert.Count, result.Count);
                for (int i = 0; i < 7; i++)
                {
                    var original = upsert[i];
                    var created = result[i];
                    if (i > 1)
                    {
                        Assert.Equal(original.Id, created.Id);
                        Assert.Equal(original.Name, created.Name);
                        Assert.Equal(original.FieldA, created.FieldA);
                        Assert.Equal(original.Weight, created.Weight);
                        Assert.Equal(original.Children[0].Name, created.Children[0].Name);
                    }
                    else
                    {
                        Assert.Equal(original.Name, created.Name);
                        Assert.Equal(original.FieldA, created.FieldA);
                        Assert.Equal("Updated " + (i + 1), created.FieldA);
                        Assert.Equal("Upsert", created.Name);
                    }
                }
            });
            task.Start();
        }
        [Fact]
        public void SearchWithAnd()
        {
            Settings.Instance.DropDatabase("test");
            var task = new Task(async () =>
            {
                await test.StoreAsync(testData);
                var query = new FindRequest<TestItem>();
                query.Find(x => x.Name, "beta");
                query.And(x => x.FieldA, "item2");
                query.Sort(x => x.Name);
                query.Sort(x => x.FieldA);
                var expectedIds = new List<int> { 1, 2, 3, 5, 11 };
                var expected = testData.Where(x => expectedIds.Contains(x.TestId)).OrderBy(x => x.Name).ThenBy(x => x.FieldA).ToList();
                var result = await test.SearchAsync(query);
                Assert.Equal(expected.Count, result.Count);
                for (int i = 0; i < expectedIds.Count; i++)
                {
                    var original = expected[i];
                    var created = result[i];
                    Assert.Equal(original.Id, created.Id);
                    Assert.Equal(original.Name, created.Name);
                    Assert.Equal(original.FieldA, created.FieldA);
                    Assert.Equal(original.Weight, created.Weight);
                    Assert.Equal(original.Children[0].Name, created.Children[0].Name);
                }
            });
            task.Start();
        }
        [Fact]
        public void SearchWithOr()
        {
            Settings.Instance.DropDatabase("test");
            var task = new Task(async () =>
            {
                await test.StoreAsync(testData);

                var query = new FindRequest<TestItem>();
                query.Or(x => x.Name, "beta");
                query.Or(x => x.FieldA, "item5");
                query.Sort(x => x.Name);
                query.Sort(x => x.FieldA);
                var expectedIds = new List<int> { 1, 2, 3, 5, 8, 9, 10 };
                var expected = testData.Where(x => expectedIds.Contains(x.TestId)).OrderBy(x => x.Name).ThenBy(x => x.FieldA).ToList();
                var result = await test.SearchAsync(query);
                Assert.Equal(expected.Count, result.Count);
                for (int i = 0; i < expectedIds.Count; i++)
                {
                    var original = expected[i];
                    var created = result[i];
                    Assert.Equal(original.Id, created.Id);
                    Assert.Equal(original.Name, created.Name);
                    Assert.Equal(original.FieldA, created.FieldA);
                    Assert.Equal(original.Weight, created.Weight);
                    Assert.Equal(original.Children[0].Name, created.Children[0].Name);
                }

            });
            task.Start();
        }
        [Fact]
        public void SearchWithNot()
        {
            Settings.Instance.DropDatabase("test");
            var task = new Task(async () =>
            {
                await test.StoreAsync(testData);

                var query = new FindRequest<TestItem>();
                query.Not(x => x.Name, "beta");
                query.Not(x => x.FieldA, "item5");
                query.Sort(x => x.Name);
                query.Sort(x => x.FieldA);
                var expectedIds = new List<int> { 4, 6, 7, 11, 12 };
                var expected = testData.Where(x => expectedIds.Contains(x.TestId)).OrderBy(x => x.Name).ThenBy(x => x.FieldA).ToList();
                var result = await test.SearchAsync(query);
                Assert.Equal(expected.Count, result.Count);
                for (int i = 0; i < expectedIds.Count; i++)
                {
                    var original = expected[i];
                    var created = result[i];
                    Assert.Equal(original.Id, created.Id);
                    Assert.Equal(original.Name, created.Name);
                    Assert.Equal(original.FieldA, created.FieldA);
                    Assert.Equal(original.Weight, created.Weight);
                    Assert.Equal(original.Children[0].Name, created.Children[0].Name);
                }
            });
            task.Start();
        }
        [Fact]
        public void SearchWithLike()
        {
            Settings.Instance.DropDatabase("test");
            var task = new Task(async () =>
            {
                await test.StoreAsync(testData);

                var query = new FindRequest<TestItem>();
                query.Find(x => x.Name, "be", EnComparator.Like);
                query.Sort(x => x.Name);
                query.Sort(x => x.FieldA);
                var expectedIds = new List<int> { 1, 2, 3, 5, 11 };
                var expected = testData.Where(x => expectedIds.Contains(x.TestId)).OrderBy(x => x.Name).ThenBy(x => x.FieldA).ToList();
                var result = await test.SearchAsync(query);
                Assert.Equal(expected.Count, result.Count);
                for (int i = 0; i < expectedIds.Count; i++)
                {
                    var original = expected[i];
                    var created = result[i];
                    Assert.Equal(original.Id, created.Id);
                    Assert.Equal(original.Name, created.Name);
                    Assert.Equal(original.FieldA, created.FieldA);
                    Assert.Equal(original.Weight, created.Weight);
                    Assert.Equal(original.Children[0].Name, created.Children[0].Name);
                }
            });
            task.Start();
        }
        [Fact]
        public void SearchWithGreaterThan()
        {
            Settings.Instance.DropDatabase("test");
            var task = new Task(async () =>
            {
                await test.StoreAsync(testData);

                var query = new FindRequest<TestItem>();
                query.Find(x => x.Weight, 50, EnComparator.GreaterThan);
                query.Sort(x => x.Name);
                query.Sort(x => x.FieldA);
                var expectedIds = new List<int> { 1, 3, 6, 7 };
                var expected = testData.Where(x => expectedIds.Contains(x.TestId)).OrderBy(x => x.Name).ThenBy(x => x.FieldA).ToList();
                var result = await test.SearchAsync(query);
                Assert.Equal(expected.Count, result.Count);
                for (int i = 0; i < expectedIds.Count; i++)
                {
                    var original = expected[i];
                    var created = result[i];
                    Assert.Equal(original.Id, created.Id);
                    Assert.Equal(original.Name, created.Name);
                    Assert.Equal(original.FieldA, created.FieldA);
                    Assert.Equal(original.Weight, created.Weight);
                    Assert.Equal(original.Children[0].Name, created.Children[0].Name);
                }

            });
            task.Start();
        }
        [Fact]
        public void SearchWithLessThan()
        {
            Settings.Instance.DropDatabase("test");
            var task = new Task(async () =>
            {
                await test.StoreAsync(testData);

                var query = new FindRequest<TestItem>();
                query.Find(x => x.Weight, 50, EnComparator.LessThan);
                query.Sort(x => x.Name);
                query.Sort(x => x.FieldA);
                var expectedIds = new List<int> { 2, 4, 5, 8, 9, 10, 11, 12 };
                var expected = testData.Where(x => expectedIds.Contains(x.TestId)).OrderBy(x => x.Name).ThenBy(x => x.FieldA).ToList();
                var result = await test.SearchAsync(query);
                Assert.Equal(expected.Count, result.Count);
                for (int i = 0; i < expectedIds.Count; i++)
                {
                    var original = expected[i];
                    var created = result[i];
                    Assert.Equal(original.Id, created.Id);
                    Assert.Equal(original.Name, created.Name);
                    Assert.Equal(original.FieldA, created.FieldA);
                    Assert.Equal(original.Weight, created.Weight);
                    Assert.Equal(original.Children[0].Name, created.Children[0].Name);
                }
            });
            task.Start();
        }
        [Fact]
        public void SearchNested()
        {
            Settings.Instance.DropDatabase("test");
            var task = new Task(async () =>
            {
                await test.StoreAsync(testData);

                var query = new FindRequest<TestItem>();
                query.Find(x => x.Children[0].Name, "test");
                var expectedIds = new List<int> { 1 };
                var expected = testData.Where(x => expectedIds.Contains(x.TestId)).OrderBy(x => x.Name).ThenBy(x => x.FieldA).ToList();
                var result = await test.SearchAsync(query);
                Assert.Equal(expected.Count, result.Count);
                for (int i = 0; i < expectedIds.Count; i++)
                {
                    var original = expected[i];
                    var created = result[i];
                    Assert.Equal(original.Id, created.Id);
                    Assert.Equal(original.Name, created.Name);
                    Assert.Equal(original.FieldA, created.FieldA);
                    Assert.Equal(original.Weight, created.Weight);
                    Assert.Equal(original.Children[0].Name, created.Children[0].Name);
                }
            });
            task.Start();
        }
        [Fact]
        public void SearchMixed()
        {
            Settings.Instance.DropDatabase("test");
            var task = new Task(async () =>
            {
                await test.StoreAsync(testData);
                var query = new FindRequest<TestItem>();
                query.Find(x => x.Name, "teran");
                query.And(x => x.Children[0].Name, "nest");
                query.Or(x => x.FieldA, "zet", EnComparator.Like);
                query.Or(x => x.TestId, 12, EnComparator.GreaterThan);
                query.Or(x => x.Weight, 10, EnComparator.LessThan);
                query.Sort(x => x.TestId);
                var expectedIds = new List<int> { 13, 14 };
                var expected = testData.Where(x => expectedIds.Contains(x.TestId)).OrderBy(x => x.TestId).ToList();
                var result = await test.SearchAsync(query);
                Assert.Equal(expected.Count, result.Count);
                for (int i = 0; i < expectedIds.Count; i++)
                {
                    var original = expected[i];
                    var created = result[i];
                    Assert.Equal(original.Id, created.Id);
                    Assert.Equal(original.Name, created.Name);
                    Assert.Equal(original.FieldA, created.FieldA);
                    Assert.Equal(original.Weight, created.Weight);
                    Assert.Equal(original.Children[0].Name, created.Children[0].Name);
                }
            });
            task.Start();
        }
        [Fact]
        public void UniqueIndex()
        {//MongoDB.Driver.MongoBulkWriteException
            var task = Task.Run(async () =>
            {
                Settings.Instance.DropDatabase("test");
                test.UniqueIndex("TestId", x => x.TestId);
                await test.StoreAsync(testData);
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
                await test.StoreAsync(dbl);
            });
            Assert.ThrowsAsync<MongoBulkWriteException>(async () => await task);
        }
        [Fact]
        public void StoreKeyValue()
        {
            var task = Task.Run(async () =>
            {
                Settings.Instance.DropDatabase("test_log");
                var log = new Container<object>("test_log");
                var obj = new System.Dynamic.ExpandoObject();
                var props = ((IDictionary<string, object>)obj);
                props.Add("LogType", "Info");
                props.Add("Message", "Test key value 2");
                await log.StoreAsync(new List<object> { obj });

            });
            Assert.ThrowsAsync<MongoBulkWriteException>(async () => await task);
        }
        [Fact]
        public void StoreUnknownType()
        {
            var task = Task.Run(async () =>
            {
                Settings.Instance.DropDatabase("test_log");
                var log = new Container<object>("test_log");

                await log.StoreAsync(new List<object> { new { LogType = "Info", Message = "This is a test" } });
            });
            Assert.ThrowsAsync<MongoBulkWriteException>(async () => await task);
        }
        [Fact]
        public void SearchInUkknown()
        {
            Settings.Instance.DropDatabase("test_log");
            var log = new Container<object>("test_log");
            Task.Run(async () =>
            {
                var obj = new { Message = "This is a test" };
                await log.StoreAsync(new List<object> { obj });
                var query = new FindRequest<object>();
                query.Find("Message", "This is a test");
                var expectedIds = new List<int> { 13, 14 };
                var result = await log.SearchAsync(query);
                Assert.Equal(1, result.Count);
                var created = result[0];
                var val = ExpressionGererator.ObjectValue("Message", created);
                Assert.Equal("This is a test", val);
            }).Wait();
        }
    }
}
