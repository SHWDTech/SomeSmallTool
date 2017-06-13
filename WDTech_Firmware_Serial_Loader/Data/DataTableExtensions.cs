using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace WDTech_Firmware_Serial_Loader.Data
{
    public static class DataTableExtensions
    {
        public static List<T> ToListOf<T>(this DataTable dt)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            var columnNames = dt.Columns.Cast<DataColumn>()
                .Select(c => c.ColumnName)
                .ToList();
            var objectProperties = typeof(T).GetProperties(flags);
            var targetList = dt.AsEnumerable().Select(dataRow =>
            {
                var instanceOfT = Activator.CreateInstance<T>();

                foreach (var properties in objectProperties.Where(properties => columnNames.Contains(properties.Name) && dataRow[properties.Name] != DBNull.Value))
                {
                    if (properties.PropertyType == typeof(float))
                    {
                        var value = float.Parse(dataRow[properties.Name].ToString());
                        properties.SetValue(instanceOfT, value, null);
                    }
                    else if (properties.PropertyType == typeof(DateTime))
                    {
                        var value = DateTime.Parse(dataRow[properties.Name].ToString());
                        properties.SetValue(instanceOfT, value, null);
                    }
                    else if (properties.PropertyType == typeof(bool))
                    {
                        var value = long.Parse(dataRow[properties.Name].ToString()) == 1;
                        properties.SetValue(instanceOfT, value, null);
                    }
                    else if (properties.PropertyType == typeof(int))
                    {
                        var value = int.Parse(dataRow[properties.Name].ToString());
                        properties.SetValue(instanceOfT, value, null);
                    }
                    else
                    {
                        properties.SetValue(instanceOfT, dataRow[properties.Name], null);
                    }
                }
                return instanceOfT;
            }).ToList();

            return targetList;
        }
    }
}
