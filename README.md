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
In order to add filters into the search use the AddCriteria function like the following
example:

            query.AddCriteria("Name", "beta");

This set the query to search for items whose field attribute Name is Equal to "beta"

**AddCriteria** accepts the following parameters

**field**: The field name
**value**: The value you wish to search for
**logicOperator**: The logicOperator with multiple fields **(AND,OR,NOT)**
**compare**: The comparison criteria. The following comparison options are available at this moment
+ **Equal To:** The field is equal to search value
+ **Like:** The field is like the search value
+ **Greater Than:** The field is greater than search value
+ **Less Than:** The field is Less than search value

**Sorting Results**

To sort the search Results use the AddSort function like the example bellow

            query.AddSort("Name");
This example tells the search to sort results by the Field Attribute Name Ascending, if you want to do descending sort you need to set the ascending Parameter to false like the next example

            query.AddSort("Name", false);

You can define more than one Sort fields eg Sort first by name Ascending and next Weight Descending

            query.AddSort("Name");
            query.AddSort("Weight", false);

To perform the Search you simply call the **Core** function **Search** with the QueryInfo instance of your search like the example below:

            query = new FindRequest<TestItem>();
            query.AddCriteria("Name", "beta", EnOperator.Or);
            query.AddCriteria("FieldA", "item5", EnOperator.Or);
            query.AddSort("Name");
            query.AddSort("FieldA");
            var result = test.Search(query);


## Releases

13/05/2018: Version 0.1