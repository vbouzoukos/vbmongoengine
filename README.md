# VB Mongo Engine

A simple Engine to connect insert and search Items in a mongo database.

### Nuget Install
[Nuget](https://www.nuget.org/packages/Vb.Mongo.Engine)

## Usage Example

### Set up a connection to mongo DB
Connection is managed via the MongoBuilder class. To connect to a mongoDB database you need to setup the connection string in an MongoBuilder class instance and use it to create a context to the database. From there you can create a repository to the collection that will be accessed. 

            //create an MongoBuilder instance
            var myMongoBuilder = new MongoBuilder("mongodb://localhost");
            using (var ctx =testMongoBuilder.CreateContext("mydb"))
            {
            	var repo = v.CreateRepository<TestItem>(t => t.Id);
            	//Data manipulation goes here
            }
            using (var ctx =testMongoBuilder.CreateContext("mydb2"))
            {
            	var repo = v.CreateRepository<Product>("productCollection"); 
            	//Data manipulation goes here
            }

### Insert Data into Mongo DB database

After setting up a repository instance you can insert data using the store function like the following example

            using (var ctx =testMongoBuilder.CreateContext("mydb"))
            {
            	var repo = v.CreateRepository<TestItem>(t => t.Id);
            	//store data
            	repo.Store(
                new List<TestItem>
                {
                    new TestItem() { Id = new MongoDB.Bson.ObjectId(), Name = "beta", FieldA = "cccccc", Weight = 99 },
                    new TestItem() { Id = new MongoDB.Bson.ObjectId(), Name = "beta", FieldA = "dddddd", Weight = 2 },
                    new TestItem() { Id = new MongoDB.Bson.ObjectId(), Name = "beta", FieldA = "zzzzzz", Weight = 99 },
                    new TestItem() { Id = new MongoDB.Bson.ObjectId(), Name = "Alpha", FieldA = "item1", Weight = 10 },
                    new TestItem() { Id = new MongoDB.Bson.ObjectId(), Name = "beta", FieldA = "item2", Weight = 11 },
                    new TestItem() { Id = new MongoDB.Bson.ObjectId(), Name = "gamma", FieldA = "item3", Weight = 56 },
                    new TestItem() { Id = new MongoDB.Bson.ObjectId(), Name = "delta", FieldA = "item4", Weight = 65 },
                    new TestItem() { Id = new MongoDB.Bson.ObjectId(), Name = "epsilon", FieldA = "item5", Weight = 40 },
                    new TestItem() { Id = new MongoDB.Bson.ObjectId(), Name = "sdagddg", FieldA = "item5", Weight = 8 },
                    new TestItem() { Id = new MongoDB.Bson.ObjectId(), Name = "gfdgf", FieldA = "item5", Weight = 50 },
                    new TestItem() { Id = new MongoDB.Bson.ObjectId(), Name = "piiyd", FieldA = "item5", Weight = 3 },
                    new TestItem() { Id = new MongoDB.Bson.ObjectId(), Name = "epsilon", FieldA = "aaaaa", Weight = 7 }
                });
    	    }


### Search in Mongo DB

In order to search for Data you will need to create a FindRequest instance using the CreateRequest function of the MongoRepository class that contains the search information like the following example

```
	var query = repo.CreateFindRequest();

```

**Filtering Criteria**
In order to add filters into the search use the **And**, **Or** and **Not** functions like the following
example:

    	query.And(x=>x.Name,"beta").Or(x=>x.Name,"epsilon").Not(x=>x.Weight,50);

This set the query to search for items whose field attribute Name is Equal to "beta"

**And, Or and Not** accepts the following parameters

**field**: Expression of the field we search
**value**: The value you wish to search for
**compare**: The comparison criteria. The following comparison options are available at this moment
+ **Equal To:** The field is equal to search value
+ **Like:** The field is like the search value
+ **Greater Than:** The field is greater than search value
+ **Less Than:** The field is Less than search value

**Sorting Results**

To sort the search Results use the AddSort function like the example bellow

            query.Sort(x=>x.Name);
This example tells the search to sort results by the Field Attribute Name Ascending, if you want to do descending sort you need to set the ascending Parameter to false like the next example

            query.Sort(x=>x.Name, false);

You can define more than one Sort fields eg Sort first by name Ascending and next Weight Descending

            query.Sort(x=>x.Name).Sort(x=>x.Weight, false);

To perform the Search you simply call the Execute function of the FindRequest instance like the example below:

            query = new FindRequest<TestItem>()
            .query.And(x=>x.Name,"beta")
            .Or(x=>x.Name,"epsilon")
            .Not(x=>x.Weight,50)
            .Sort(x=>x.Name)
            .Sort(x=>x.Weight, false);
            var result = query.Execute();

You can use the SearchAsync in case you need asynchronous results 

### Searching with IQueryable

The MongoRepository class offers access to the collection's IQueryable instance that can be used to perform more complex searches using Linq. To access the IQueryable you need to call the following functions depending on your needs.

The **Queryable** Property returns the entire collection data to be used by your needs.

```
var collection =repo.Queryable;
```

The **FindByCondition** function that accepts an linq expression as parameter and can be used to perform search on a subset of the collection

```
var collection =repo.FindByCondition(x=x.Weight>20);
```

### Delete Items

In order to delete items from mongoDB use a FindRequest like the above containing the data you wish to delete and call the Delete function of the MongoRepository instance

	var result = repo.Delete(query);

### Update Items
Update items is performed using the Replace function in Container like the result bellow

	test.Replace(itemUpdated);

Replace accepts the following data parameters

**item** The entity that contains the updated data and will be used to replace the old data

Similar are the ReplaceAsync for asynchronous execution and
### Mass Update or Store
Bulk, BulkAsync(For asynchronus execution) are used to mass insert or update data 

	test.Bulk(listOfUpdateAndNew);

**items** The list of entities that contains the updated and new data.

### MongoRepository

As seen above you can use any of the MongoContext class overloaded CreateRepository function to create a MongoRepository instance .

The CreateRepository has defined the following parameters. 

**idField** (**optional**) An attribute expression to define which attribute is the Id of the entity. If null then the entity should have an _id Field to retrieve data.

**collectionName** (**optional**) The mongodb collection name that stores the entities of the repository, if null the collection name is going to be the class name

**settings** (**optional**) A block of code that may needed to execute custom code before the initialization of the repository eg Creating the collection or set the auto mapping.

### Automapping

MongoBuilder offers access to the mapping mechanism of c# MongoDB driver with the **AutoMap** function

Use AutoMap to perform the mapping using the class definition and MongoDB annotations

Or use the **attributesMapping** optional parameter to set how the data is mapped.

### Transactions

Transactions require a replica set to be set in order to work. For more information how to set a replica set see the following from mongoDB documentation:

[Deploy a Replica Set](https://docs.mongodb.com/manual/tutorial/deploy-replica-set/)

[Deploy a Replica Set for Testing and Development](https://docs.mongodb.com/manual/tutorial/deploy-replica-set-for-testing/)

Transactions work on existing collections. So before inserting data you need to create the collection.

Transactions are managed via the mongoDB context and you can insert multiple documents this way.

- To start a transaction you use the **BeginTransaction** function of MongoDBContext.
- To commit the transaction use the **CommitTransaction**.
- In case you need to abort the transaction use the **RollbackTransaction** transaction.

Below is an example of manipulating data in transaction.

                    try
                    {
                        dbCtx.BeginTransaction();
                        await repo.Delete(deleteRequest);
                        await repo.Store(collection);
                        dbCtx.CommitTransaction();
                    }
                    catch
                    {
                        dbCtx.RollbackTransaction();
                        throw;
                    }
## Releases

**xx/0x/2020: Version 0.4**

```
Breaking changes
*Removed classes Settings and Container

Functionality is supported via Contexts and Repositories
Fixed bug on Transactions
Added non unique indexes support
Include Example projects
Use version 2.10.2 of MongoDB.Driver
```

**22/10/2019: Version 0.3**

	Breaking changes
	*Renamed Core to Container
	
	Enhanced searching criteria to include search field as a string
	You can store an object now instead of list
	Added unique indexes
	Added Support for
	    netcoreapp1.1
	    netcoreapp2.1
	    netstandard1.5
	    netstandard1.6
	    netstandard2.0
	    net451
	    net452

**16/09/2018: Version 0.2**

	Breaking changes
	*Searching functionality is now defined in Vb.Mongo.Engine.Find namespace
	*Renamed QueryInfo to FindRequest
	*removed AddCriteria and AddSort replaced with "And" "Or" "Not" and "Sort" functions 
	
	Support async Search
	Paging and max results. Default can be set in Settings
	Added Delete and Update options

**13/05/2018: Version 0.1**

	First release base functionality Insert items, search items drop database.

