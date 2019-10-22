version 0.1
First release base functionality Insert items, search items drop database.

version 0.2

	Breaking changes
	*Searching functionality is now defined in Vb.Mongo.Engine.Find namespace
	*Renamed QueryInfo to FindRequest
    *removed AddCriteria and AddSort replaced with "And" "Or" "Not" and "Sort" functions 

	Support async Search
	Paging and max results. Default can be set in Settings
	Added Delete and Update options

version 0.3
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