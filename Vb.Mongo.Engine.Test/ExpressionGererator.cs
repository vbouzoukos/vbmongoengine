using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Vb.Mongo.Engine.Test
{
    public class ExpressionGererator
    {
        public static object ObjectValue(string attribute, object entity)
        {
#if NETSTANDARD1_5 || NETSTANDARD1_6
            var type=entity.GetType().GetTypeInfo();
#else
            var type = entity.GetType();
#endif
            PropertyInfo propertyInfo = type.GetProperty(attribute);
            if (propertyInfo is null)
            {
                if (type.Name == typeof(System.Dynamic.ExpandoObject).Name)
                {
                    var dobj = (System.Dynamic.ExpandoObject)entity;
                    var propertyValues = (IDictionary<string, object>)dobj;
                    var item = propertyValues[attribute];
                    return item;
                }
                throw new InvalidOperationException(string.Format("Attribute {0} not found.", attribute));
            }
            else
            {
                var item = propertyInfo.GetValue(entity);
                return item;
            }
        }
    }
}
