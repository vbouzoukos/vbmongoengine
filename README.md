# VB Mongo Engine

A simple Engine to connect insert and search Items in a mongo database.

### Nuget Install
[Nuget](https://www.nuget.org/packages/Vb.Mongo.Engine)

## Usage Example

### Set up a connection to mongo DB
In order to connect into the Mongo DB you ll need to use the StartUp function of the Settings Class

            Settings.StartUp("mongodb://localhost");

### Insert Data into Mongo DB database

To insert data into the mongo DB database you need to Set Up a Core instance of the stored entity like the following example

            var test = new Core<TestItem>("test");
This creates an instance of the TestItem entity and will be mapped to the corresponding mongoDB entity, next you can insert data using the Store function of the Core instance

            test.Store(
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
                }
            );


### Search in Mongo DB


In order to search for Data you ll need a FindRequest instance that contains the search information

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

To perform the Search you simply call the **Core** function **Search** with the FindRequest instance of your search like the example below:

            query = new FindRequest<TestItem>()
            .query.And(x=>x.Name,"beta")
            .Or(x=>x.Name,"epsilon")
            .Not(x=>x.Weight,50)
            .Sort(x=>x.Name)
            .Sort(x=>x.Weight, false);
            var result = test.Search(query);
            
You can use the SearchAsync in case you need asynchronous results 

###Delete Items

In order to delete items from mongoDB use a FindRequest like the above containing the data you wish to delete

	var result = test.Delete(query);

###Update Items
Update items is performed using the Replace function in Core like the result bellow

	test.Replace(x=>x.Id, itemUpdated);

Replace accepts the following data parameters

**id** idField, The expression of the Id field
**item** The entity that contains the updated data and will be used to replace the old data

Similar are the ReplaceAsync for asynchronous execution and
###Mass Update or Store
Bulk, BulkAsync(For asynchronus execution) are used to mass insert or update data 

	test.Bulk(x=>x.Id, listOfUpdateAndNew);
	
**id** idField, The expression of the Id field
**items** The list of entities that contains the updated and new data.

## Releases

13/05/2018: Version 0.1

16/09/2018: Version 0.2