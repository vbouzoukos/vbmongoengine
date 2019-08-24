using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Vb.Mongo.Engine.Entity
{
    internal class Metadata
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

        /// <summary>
        /// Returns an array of custom attributes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static T[] CustomAttributeInfos<T>(PropertyInfo prop) where T : Attribute
        {
            var attributes = prop.GetCustomAttributes(typeof(T), true) as T[];
            return attributes.ToArray();
        }

        /// <summary>
        /// Generate the expression prop=>prop.pPropertyName
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="pPropertyName">The property name</param>
        /// <returns>The expression prop=>prop.[pPropertyName] </returns>
        public static Expression<Func<T, object>> PropertyExpression<T>(string pPropertyName)
        {
            var parameter = Expression.Parameter(typeof(T));
#if NETSTANDARD1_5 || NETSTANDARD1_6
            PropertyInfo prop = typeof(T).GetTypeInfo().GetProperty(pPropertyName);
#else
            PropertyInfo prop = typeof(T).GetProperty(pPropertyName);
#endif
            var property = Expression.Property(parameter, prop);
            var conversion = Expression.Convert(property, typeof(object));
            var lambda = Expression.Lambda<Func<T, object>>(conversion, parameter);
            return lambda;
        }

        /// <summary>
        /// Using a member Expression like x=>x.Property get x.Property value of the entity
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="idField">Expression</param>
        /// <param name="entity">Entity that we want to get the value using the expression</param>
        /// <returns></returns>
        public static object ObjectValue<T>(Expression<Func<T, object>> expression, T entity)
        {
            var memberExp = GetMemberInfo(expression);
            var propertyInfo = memberExp.Member as PropertyInfo;
            var item = propertyInfo.GetValue(entity, null);
            return item;
        }

        /// <summary>
        /// Compare an Id expression of an mongoDb entity to check if this entity is null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="idField"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static bool EmptyMongoId<T>(Expression<Func<T, object>> idField, T entity)
        {
            var memberExp = GetMemberInfo(idField);

            var item = Expression.PropertyOrField(Expression.Constant(entity), memberExp.Member.Name);
            object constant = null;
            if (item.Type == typeof(ObjectId))
            {
                constant = (object)ObjectId.Empty;
            }
            var objPropExp = Expression.Constant(constant);
            var equalExp = Expression.Equal(item, objPropExp);
            var lambda = Expression.Lambda<Func<bool>>(equalExp);
            var result = (bool)lambda.Compile().DynamicInvoke();
            return result;
        }

        /// <summary>
        /// Returns the Member Expression of a method lamda expression
        /// </summary>
        /// <param name="method">the method expression</param>
        /// <returns>MemberExpression or null if the expression is invalid</returns>
        public static MemberExpression GetMemberInfo(Expression method)
        {
            LambdaExpression lambda = method as LambdaExpression;
            if (lambda != null)
            {

                MemberExpression memberExpr = null;

                if (lambda.Body.NodeType == ExpressionType.Convert)
                {
                    memberExpr =
                        ((UnaryExpression)lambda.Body).Operand as MemberExpression;
                }
                else if (lambda.Body.NodeType == ExpressionType.MemberAccess)
                {
                    memberExpr = lambda.Body as MemberExpression;
                }

                return memberExpr;
            }
            return null;
        }
    }
}
