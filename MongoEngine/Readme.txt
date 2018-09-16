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
    