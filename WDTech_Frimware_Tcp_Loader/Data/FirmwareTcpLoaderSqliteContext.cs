using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using FirmwareDownloaderHelper;

namespace WDTech_Frimware_Tcp_Loader.Data
{
    public class FirmwareSerialLoaderSqliteContext
    {
        public static string DefaultConnectinoString { get; set; }

        private readonly string _connectionString;

        public FirmwareSerialLoaderSqliteContext()
        {
            _connectionString = DefaultConnectinoString;
        }

        #region DbSets

        public List<ConfigDict> ConfigDicts 
            => GetDbSet<ConfigDict>();

        public List<LocalConfig> LocalConfigs
            => GetDbSet<LocalConfig>();

        #endregion

        private List<T> GetDbSet<T>() where T : class, new()
        {
            var tableName = typeof(T).Name;
            using (var conn = new SQLiteConnection(_connectionString))
            {
                var cmd = new SQLiteCommand($"SELECT * FROM {tableName}", conn);
                using (var adapter = new SQLiteDataAdapter(cmd))
                {
                    var table = new DataTable();
                    adapter.Fill(table);
                    return table.ToListOf<T>();
                }
            }
        }

        public T FirstOrDefault<T>(string where) where T : class, new()
        {
            var tableName = typeof(T).Name;
            using (var conn = new SQLiteConnection(_connectionString))
            {
                var cmd = string.IsNullOrWhiteSpace(where)
                    ? new SQLiteCommand($"SELECT * FROM {tableName}", conn)
                    : new SQLiteCommand($"SELECT * FROM {tableName} WHERE {where}", conn);
                using (var adapter = new SQLiteDataAdapter(cmd))
                {
                    var table = new DataTable();
                    adapter.Fill(table);
                    return table.ToListOf<T>().FirstOrDefault();
                }
            }
        }

        public int AddOrUpdate<T>(List<T> models) where T : class, new()
        {
            var count = 0;
            foreach (var model in models)
            {
                count += AddOrUpdate(model);
            }

            return count;
        }

        public int AddOrUpdate<T>(T model) where T : class, new()
        {
            return IsExist(model) ? Update(model) : Add(model);
        }

        public bool IsExist<T>(T model) where T : class, new()
        {
            var propertyInfo = model.GetType().GetProperty("Id");
            if (propertyInfo != null)
            {
                var id = (long)propertyInfo.GetValue(model, null);

                var tableName = typeof(T).Name;
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand($"SELECT Count(*) FROM {tableName} where Id = {id}", conn))
                    {
                        return (long)cmd.ExecuteScalar() != 0;
                    }
                }
            }

            return false;
        }

        public object GetId<T>(string where) where T : class, new()
        {
            var tableName = typeof(T).Name;
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SQLiteCommand($"SELECT Id FROM {tableName} where {where}", conn))
                {
                    return cmd.ExecuteScalar();
                }
            }
        }


        private int Add<T>(T model) where T : class, new()
        {
            var stametParams = InsertParams(model);
            var sql = $"INSERT INTO {typeof(T).Name} ({stametParams[0]}) VALUES ({stametParams[1]})";

            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        private int Update<T>(T model) where T : class, new()
        {
            var stametParams = UpdateParams(model);
            var sql = $"UPDATE {typeof(T).Name} SET {stametParams}";

            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        private static string[] InsertParams<T>(T model) where T : class, new()
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            var objectProperties = typeof(T).GetProperties(flags);
            var columns = new List<string>();
            var values = new List<string>();

            foreach (var objectProperty in objectProperties)
            {
                if (objectProperty.Name == "Id") continue;
                columns.Add(objectProperty.Name);
                var value = objectProperty.GetValue(model, null);
                if (value is string)
                {
                    value = $"'{value}'";
                }
                if (value is bool)
                {
                    value = Convert.ToInt32(value);
                }
                if (value is DateTime)
                {
                    value = ($"'{value:yyyy-MM-dd HH:mm:ss}'");
                }
                values.Add(value.ToString());
            }

            var result = new string[2];
            result[0] = string.Join(",", columns);
            result[1] = string.Join(",", values);

            return result;
        }

        private static string UpdateParams<T>(T model) where T : class, new()
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            var objectProperties = typeof(T).GetProperties(flags);
            var result = new List<string>();
            foreach (var objectProperty in objectProperties)
            {
                if (objectProperty.Name == "Id") continue;
                var value = objectProperty.GetValue(model, null);
                if (value is string)
                {
                    value = $"'{value}'";
                }
                if (value is bool)
                {
                    value = Convert.ToInt32(value);
                }
                if (value is DateTime)
                {
                    value = ($"'{value:yyyy-MM-dd HH:mm:ss}'");
                }
                result.Add($"{objectProperty.Name} = {value}");
            }
            var propertyInfo = model.GetType().GetProperty("Id");
            return propertyInfo != null ? $"{string.Join(",", result)} WHERE Id = {(long)propertyInfo.GetValue(model, null)}" : string.Empty;
        }

        public int Execute(string executeSql)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(executeSql, conn))
                {
                    return cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
