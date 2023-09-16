using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Reformat.Data.EFCore.Annotations;
using Reformat.Data.EFCore.Core;
using Reformat.Data.EFCore.Domain;
using Reformat.Data.EFCore.Enums;
using Reformat.Framework.Core.Common.Extensions.lang;

namespace Reformat.Data.EFCore;

public static class QueryHelper
{
    /// <summary>
    /// 生成查询Order部份
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="BlackList"></param>
    /// <returns></returns>
    public static IQueryable<T> GetOrder<T>(this IQueryable<T> queryable, SqlQuery<T> SQLQuery) where T : EFCoreEntity
    {
        var orderParms = SQLQuery.OrderParms;
        if (orderParms == null || orderParms.Count == 0) return queryable;
        var entityType = typeof(T);
        ParameterExpression p = Expression.Parameter(entityType);
        
        foreach (var orderParm in orderParms)
        {
            var columnName = orderParm.Key;
            var sortDirection = orderParm.Value;

            PropertyInfo property = entityType.GetProperty(columnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (property == null)
            {
                throw new ArgumentException($"Property '{columnName}' not found on type '{entityType.Name}'.");
            }
            var expr = GetOrderExpression(entityType, property);

            switch (sortDirection)
            {
                case OrderType.AES:
                {
                    var method = typeof(Queryable).GetMethods().FirstOrDefault(m => m.Name == "OrderBy" && m.GetParameters().Length == 2);
                    var genericMethod = method.MakeGenericMethod(entityType, property.PropertyType);
                    queryable = (IQueryable<T>)genericMethod.Invoke(null, new object[] { queryable, expr });
                    break;
                }
                case OrderType.DESC:
                {
                    var method = typeof(Queryable).GetMethods().FirstOrDefault(m => m.Name == "OrderByDescending" && m.GetParameters().Length == 2);
                    var genericMethod = method.MakeGenericMethod(entityType, property.PropertyType);
                    queryable =  (IQueryable<T>)genericMethod.Invoke(null, new object[] { queryable, expr });
                    break;
                }
            }
        }
        return queryable;
    }
    
    /// <summary>
    /// 获取生成表达式
    /// </summary>
    /// <param name="objType"></param>
    /// <param name="pi"></param>
    /// <returns></returns>
    private static LambdaExpression GetOrderExpression(Type objType, PropertyInfo pi)
    {
        var paramExpr = Expression.Parameter(objType);
        var propAccess = Expression.PropertyOrField(paramExpr, pi.Name);
        var expr = Expression.Lambda(propAccess, paramExpr);
        return expr;
    }
    

    /// <summary>
    /// 生成查询where部份
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="BlackList"></param>
    /// <returns></returns>
    public static IQueryable<T> GetWhere<T>(this IQueryable<T> queryable, SqlQuery<T> SQLQuery) where T : EFCoreEntity
    {
        var queryParms = SQLQuery.QueryParms;
        if (queryParms == null) return queryable;

        var _type = queryParms.GetType();
        var _defaultvalue = _type.GetConstructor(Type.EmptyTypes).Invoke(null);
        var _diff = queryParms.GetDiffentMap(_defaultvalue);

        var _plist = _type.GetProperties();
        foreach (var property in _plist)
        {
            // 默认值跳过
            if(!_diff.ContainsKey(property.Name))continue;
            
            var whereAttribute = property.GetCustomAttribute<WhereAttribute>();
            var columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
            string columnName = columnAttribute != null ? columnAttribute.Name : property.Name;

            if (whereAttribute == null) whereAttribute = new WhereAttribute();

            var value = property.GetValue(queryParms);
            if (value == null) continue;

            switch (whereAttribute.type)
            {
                case ConditionalType.Equal:
                    queryable = queryable.Where(entity => EF.Property<object>(entity, columnName) == value);
                    break;
                case ConditionalType.Like:
                    queryable = queryable.Where(entity =>
                        EF.Property<string>(entity, columnName).Contains(value.ToString()));
                    break;
                case ConditionalType.GreaterThan:
                    if (!value.IsNumericType()) throw new Exception(property.Name + ": 非数字类型");
                    queryable = queryable.Where(entity =>
                        EF.Property<decimal>(entity, columnName) > (decimal)value);
                    break;
                case ConditionalType.GreaterThanOrEqual:
                    if (!value.IsNumericType()) throw new Exception(property.Name + ": 非数字类型");
                    queryable = queryable.Where(
                        entity => EF.Property<decimal>(entity, columnName) >= (decimal)value);
                    break;
                case ConditionalType.LessThan:
                    if (!value.IsNumericType()) throw new Exception(property.Name + ": 非数字类型");
                    queryable = queryable.Where(entity =>
                        EF.Property<decimal>(entity, columnName) < (decimal)value);
                    break;
                case ConditionalType.LessThanOrEqual:
                    if (!value.IsNumericType()) throw new Exception(property.Name + ": 非数字类型");
                    queryable = queryable.Where(
                        entity => EF.Property<decimal>(entity, columnName) <= (decimal)value);
                    break;
                case ConditionalType.LikeLeft:
                    queryable = queryable.Where(entity =>
                        EF.Property<string>(entity, columnName).StartsWith(value.ToString()));
                    break;
                case ConditionalType.LikeRight:
                    queryable = queryable.Where(entity =>
                        EF.Property<string>(entity, columnName).EndsWith(value.ToString()));
                    break;
                case ConditionalType.NoEqual:
                    queryable = queryable.Where(entity => EF.Property<object>(entity, columnName) != value);
                    break;
                case ConditionalType.IsNullOrEmpty:
                    queryable = queryable.Where(entity =>
                        EF.Property<string>(entity, columnName) == null ||
                        EF.Property<string>(entity, columnName) == string.Empty);
                    break;
                case ConditionalType.IsNot:
                    queryable = queryable.Where(entity => EF.Property<object>(entity, columnName) != value);
                    break;
                case ConditionalType.NoLike:
                    queryable = queryable.Where(entity =>
                        !EF.Property<string>(entity, columnName).Contains(value.ToString()));
                    break;
                case ConditionalType.EqualNull:
                    queryable = queryable.Where(entity => EF.Property<object>(entity, columnName) == null);
                    break;
                default:
                    throw new NotImplementedException("处理 " + whereAttribute.type.ToString() + " 条件类型的代码未实现");
            }
        }

        return queryable;
    }
}