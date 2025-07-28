using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Magneto.Contract.UavDef;

public sealed class UavDefDataBase
{
    public const string ConnectionString =
        "Server=localhost;Port=5432;Database=skywaver;User Id=postgres;Password=root123;";

    private static readonly object _lockObject = new();
    private static UavDefDataBase _instance;

    private UavDefDataBase()
    {
        using var context = new UavDefDbContext();
        context.Database.Migrate();
    }

    public static UavDefDataBase Instance
    {
        get
        {
            if (_instance != null) return _instance;
            lock (_lockObject)
            {
                _instance ??= new UavDefDataBase();
            }

            return _instance;
        }
    }

    public static NpgsqlConnection Connection => new(ConnectionString);

    /// <summary>
    ///     Inserts the specified table name.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <returns>Primary id,less than 0 is false.</returns>
    public static int Insert(object data)
    {
        try
        {
            var tableName =
                ((TableAttribute)data.GetType().GetCustomAttributes(typeof(TableAttribute), false)
                    .First()).Name;
            var haveId = false;
            var parameters = new DynamicParameters();
            // Add properties to the parameters
            foreach (var property in data.GetType().GetProperties())
            {
                var value = property.GetValue(data);
                if (property.Name is "Id")
                {
                    haveId = true;
                    continue;
                }

                if (value is null) continue;
                if (value.GetType().IsClass && !value.GetType().Namespace!.StartsWith("System")) continue;
                if (property.PropertyType.IsGenericType &&
                    property.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>)) continue;
                parameters.Add(property.Name, property.GetValue(data));
            }

            var sql = $"INSERT INTO {tableName} " +
                      $"({string.Join(", ", parameters.ParameterNames.Select(p => $"\"{p}\""))}) " +
                      $"VALUES ({string.Join(", ", parameters.ParameterNames.Select(p => $"@{p}"))}) ";
            using var connection = new NpgsqlConnection(ConnectionString);
            if (!haveId)
                return connection.Execute(sql, data);
            sql += "RETURNING \"Id\"";
            return connection.QuerySingle<int>(sql, data);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return -1;
        }
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tableType">object type of the table name in db content.</param>
    /// <param name="conditions"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static IEnumerable<T> Select<T>(Type tableType, string conditions,
        SqlMapper.IDynamicParameters parameters)
    {
        try
        {
            var tableName =
                ((TableAttribute)tableType.GetCustomAttributes(typeof(TableAttribute), false).First())
                .Name;
            var sql =
                $"SELECT * FROM {tableName} WHERE 1=1 {conditions}";
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();
            return connection.Query<T>(sql, parameters);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    public static void Update(object data)
    {
        try
        {
            var tableName =
                ((TableAttribute)data.GetType().GetCustomAttributes(typeof(TableAttribute), false)
                    .First()).Name;
            // 构造 SET 子句
            var setClause = string.Join(", ", data.GetType().GetProperties()
                .Where(p => p.Name != "Id" && p.GetValue(data) is not null && !p.PropertyType.IsGenericType)
                .Select(p => $"\"{p.Name}\" = @{p.Name}"));
            // 构造 WHERE 子句
            const string whereClause = "WHERE \"Id\" = @Id";
            // 构造 UPDATE 语句
            var updateSql = $"UPDATE {tableName} SET {setClause} {whereClause}";
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Execute(updateSql, data);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    /// <summary>
    ///     Deletes the specified table name.
    /// </summary>
    /// <param name="tableType">object type of the table name in db content.</param>
    /// <param name="ids">The ids.</param>
    /// <returns><c>true</c> if success, <c>false</c> otherwise.</returns>
    public static bool Delete(Type tableType, IEnumerable<int> ids)
    {
        try
        {
            var properties = tableType.GetProperties();
            var enumerable = ids.ToList();
            if (properties.Where(property => property.PropertyType.IsGenericType
                                             && property.PropertyType.GetGenericTypeDefinition() ==
                                             typeof(ICollection<>)).Any(property =>
                    !Delete(property.PropertyType.GetGenericArguments()[0], enumerable)))
                return false;
            var tableName =
                ((TableAttribute)tableType.GetCustomAttributes(typeof(TableAttribute), false).First())
                .Name;
            var keys = properties.Where(property => property.IsDefined(typeof(KeyAttribute), false));
            foreach (var key in keys)
            {
                var sql = $"DELETE FROM {tableName} " +
                          $"WHERE \"{key.Name}\" IN ({string.Join(",", enumerable)})";
                using var connection = new NpgsqlConnection(ConnectionString);
                connection.Execute(sql);
            }

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
}