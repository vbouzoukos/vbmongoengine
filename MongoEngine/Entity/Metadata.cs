using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Vb.Mongo.Engine.Entity
{
    class Metadata
    {
        /// <summary>
        /// Read the custom attribute of a type
        /// </summary>
        /// <typeparam name="T">The returned custom attribute</typeparam>
        /// <param name="type">The type with the custom attribute</param>
        /// <returns></returns>
        public static T CustomAttributeInfo<T>(Type pType) where T : Attribute
        {
            var attribute = pType.GetTypeInfo().GetCustomAttributes(typeof(T), true).FirstOrDefault() as T;
            return attribute;
        }

        /// <summary>
        /// Read a property custom attribute
        /// </summary>
        /// <typeparam name="T">The returned custom attribute</typeparam>
        /// <param name="prop">The property that has the custom attribute</param>
        /// <returns></returns>
        public static T CustomAttributeInfo<T>(PropertyInfo prop) where T : Attribute
        {
            var attribute = prop.GetCustomAttributes(typeof(T), true).FirstOrDefault() as T;
            return attribute;
        }

        public static T[] CustomAttributeInfos<T>(PropertyInfo prop) where T : Attribute
        {
            var attributes = prop.GetCustomAttributes(typeof(T), true) as T[];
            return attributes.ToArray();
        }
        /// <summary>
        /// Generate the expression prop=>prop.pPropertyName
        /// </summary>
        /// <typeparam name="T">The class we want to extract the property</typeparam>
        /// <param name="pPropertyName">The property name</param>
        /// <returns>The expression prop=>prop.[pPropertyName] </returns>
        public static Expression<Func<T, object>> PropertyExpression<T>(string pPropertyName)
        {
            var parameter = Expression.Parameter(typeof(T));
            PropertyInfo prop = typeof(T).GetProperty(pPropertyName);
            var property = Expression.Property(parameter, prop);
            var conversion = Expression.Convert(property, typeof(object));
            var lambda = Expression.Lambda<Func<T, object>>(conversion, parameter);
            return lambda;
        }
    }
}
