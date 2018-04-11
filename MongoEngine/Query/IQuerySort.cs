using System;
namespace Vb.Mongo.Engine.Query
{
    public interface IQuerySort
    {
        string Field { get; set; }
        bool Ascending { get; set; }
    }
}
