using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Tam.Maestro.Common
{
    public static class Extensions
    {
        public static Dictionary<TValue, TKey> Reverse<TKey, TValue>(this IDictionary<TKey, TValue> source)
        {
            var dictionary = new Dictionary<TValue, TKey>();
            foreach (var entry in source)
            {
                if (!dictionary.ContainsKey(entry.Value))
                    dictionary.Add(entry.Value, entry.Key);
            }
            return dictionary;
        }

        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> list, int parts)
        {
            var i = 0;
            return from item in list
                group item by i++ % parts into part
                select part;
        }

        public static IEnumerable<TResult> FullOuterGroupJoin<A, B, Key, TResult>(
            this IEnumerable<A> first,
            IEnumerable<B> second,
            Func<A, Key> firstKeySelector,
            Func<B, Key> secondKeySelector,
            Func<Key, IEnumerable<A>, IEnumerable<B>, TResult> resultSelector)
        {

            var aLookup = first.ToLookup(firstKeySelector);
            var bLookup = second.ToLookup(secondKeySelector);
            var keys = aLookup.Select(p => p.Key).Union(bLookup.Select(p => p.Key));

            var join = keys.Select(key => resultSelector(key, aLookup[key], bLookup[key]));

            foreach (var item in join)
                yield return item;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            dictionary.TryGetValue(key, out value);
            return value;
        }

        public static void AddIfNoValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TValue> value)
        {
            TValue inner;
            if (!dictionary.TryGetValue(key, out inner))
                dictionary[key] = value.Invoke();
        }

        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TValue> value)
        {
            TValue inner;
            if (dictionary.TryGetValue(key, out inner))
                return inner;

            var invoke = value.Invoke();
            dictionary[key] = invoke;
            return invoke;
        }

        public static void AddIfNotInList<TKey, TValue>(this Dictionary<TKey, List<TValue>> dictionary, TKey key, TValue value)
        {
            List<TValue> inner;
            if (dictionary.TryGetValue(key, out inner))
            {
                if (inner.Contains(value))
                    return;
                inner.Add(value);
            }
            else
            {
                var ret = new List<TValue> { value };
                dictionary[key] = ret;
            }
        }

        public static bool TryInitializeValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, out TValue outValue) where TValue : new()
        {
            TValue inner;
            if (dictionary.TryGetValue(key, out inner))
            {
                outValue = inner;
                return true;
            }

            var value = new TValue();
            dictionary[key] = value;
            outValue = value;
            return false;
        }

        public static TValue GetOrInitialize<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
        {
            TValue inner;
            if (dictionary.TryGetValue(key, out inner))
                return inner;

            var value = new TValue();
            dictionary[key] = value;
            return value;
        }

        public static TValue GetOrInitialize<TKey, TKey2, TValue>(this Dictionary<TKey, Dictionary<TKey2, TValue>> dictionary, TKey key, TKey2 key2) where TValue : new()
        {
            Dictionary<TKey2, TValue> inner;
            if (dictionary.TryGetValue(key, out inner))
                return inner.GetOrInitialize(key2);

            var value = new TValue();
            dictionary[key] = new Dictionary<TKey2, TValue> { { key2, value } };
            return value;
        }

        public static TValue GetOrInitialize<TKey, TKey2, TKey3, TValue>(this Dictionary<TKey, Dictionary<TKey2, Dictionary<TKey3, TValue>>> dictionary, TKey key, TKey2 key2, TKey3 key3) where TValue : new()
        {
            Dictionary<TKey2, Dictionary<TKey3, TValue>> inner;
            if (dictionary.TryGetValue(key, out inner))
                return inner.GetOrInitialize(key2, key3);

            var value = new TValue();
            dictionary[key] = new Dictionary<TKey2, Dictionary<TKey3, TValue>> { { key2, new Dictionary<TKey3, TValue> { { key3, value } } } };
            return value;
        }

        public static void Set<TKey, TKey2, TValue>(this Dictionary<TKey, Dictionary<TKey2, TValue>> dictionary, TKey key, TKey2 key2, TValue value) where TValue : new()
        {
            Dictionary<TKey2, TValue> inner;
            if (dictionary.TryGetValue(key, out inner))
                inner[key2] = value;
            else
            {
                dictionary[key] = new Dictionary<TKey2, TValue> { { key2, value } };
            }
        }

        public static void Set<TKey, TKey2, TKey3, TValue>(this Dictionary<TKey, Dictionary<TKey2, Dictionary<TKey3, TValue>>> dictionary, TKey key, TKey2 key2, TKey3 key3, TValue value) where TValue : new()
        {
            Dictionary<TKey2, Dictionary<TKey3, TValue>> innerValue;
            if (dictionary.TryGetValue(key, out innerValue))
                innerValue.Set(key2, key3, value);
            else
            {
                dictionary[key] = new Dictionary<TKey2, Dictionary<TKey3, TValue>> { { key2, new Dictionary<TKey3, TValue> { { key3, value } } } };
            }
        }

        public static void AggregateValue<TKey>(this Dictionary<TKey, long> dictionary, TKey key, long value)
        {
            long dictValue;
            if (dictionary.TryGetValue(key, out dictValue))
                dictionary[key] += value;
            else
                dictionary[key] = value;
        }

        public static void AggregateValue<TKey, TKey2>(this Dictionary<TKey, Dictionary<TKey2, long>> dictionary, TKey key, TKey2 key2, long value)
        {
            Dictionary<TKey2, long> dictValue;
            if (dictionary.TryGetValue(key, out dictValue))
                dictValue.AggregateValue(key2, value);
            else
                dictionary[key] = new Dictionary<TKey2, long> { { key2, value } };
        }

        public static void AggregateValue<TKey, TKey2, TKey3>(this Dictionary<TKey, Dictionary<TKey2, Dictionary<TKey3, long>>> dictionary, TKey key, TKey2 key2, TKey3 key3, long value)
        {
            Dictionary<TKey2, Dictionary<TKey3, long>> dictValue;
            if (dictionary.TryGetValue(key, out dictValue))
                dictValue.AggregateValue(key2, key3, value);
            else
                dictionary[key] = new Dictionary<TKey2, Dictionary<TKey3, long>> { { key2, new Dictionary<TKey3, long> { { key3, value } } } };
        }

        public static DataTable ConvertToDataTable<T>(this IList<T> data)
        {
            var properties = TypeDescriptor.GetProperties(typeof(T));
            var table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (var item in data)
            {
                var row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            table.AcceptChanges();
            return table;
        }

        public static DataRow ConvertToDataRow<T>(this T item, DataTable table)
        {
            var properties =
                TypeDescriptor.GetProperties(typeof(T));
            var row = table.NewRow();
            foreach (PropertyDescriptor prop in properties)
                row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
            return row;
        }


        public static void ConvertEnumerableToDataRow<T>(this IEnumerable<T> enumerable, DataTable table)
        {
            var items = enumerable as IList<T> ?? enumerable.ToList();
            if (items.FirstOrDefault() == null)
            {
                table.Rows.Add(string.Empty);
                return;
            }

            var properties = items.FirstOrDefault().GetType().GetProperties();

            foreach (var item in items)
            {
                var row = table.NewRow();

                for (var i = 0; i < properties.Length; i++)
                {
                    var property = properties[i];

                    if (table.Columns.Contains(property.Name))
                    {
                        row[property.Name] = item.GetType().InvokeMember(property.Name, BindingFlags.GetProperty, null, item, null);
                    }
                    else
                    {
                        row[i] = item.GetType().InvokeMember(property.Name, BindingFlags.GetProperty, null, item, null);
                    }
                }
                table.Rows.Add(row);
            }
        }


        public static T ConvertToEntity<T>(this DataRow tableRow) where T : new()
        {
            // Create a new type of the entity I want
            var t = typeof(T);
            var returnObject = new T();

            foreach (DataColumn col in tableRow.Table.Columns)
            {
                var colName = col.ColumnName;

                // Look for the object's property with the columns name, ignore case
                var pInfo = t.GetProperty(colName.ToLower(),
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                // did we find the property ?
                if (pInfo != null)
                {
                    var val = tableRow[colName];

                    // is this a Nullable<> type
                    var isNullable = Nullable.GetUnderlyingType(pInfo.PropertyType) != null;
                    if (isNullable)
                    {
                        if (val is DBNull)
                        {
                            val = null;
                        }
                        else
                        {
                            // Convert the db type into the T we have in our Nullable<T> type
                            val = Convert.ChangeType(val, Nullable.GetUnderlyingType(pInfo.PropertyType));
                        }
                    }
                    else
                    {
                        // Convert the db type into the type of the property in our entity
                        SetDefaultValue(ref val, pInfo.PropertyType);
                        if (pInfo.PropertyType.IsEnum && !pInfo.PropertyType.IsGenericType)
                        {
                            val = Enum.ToObject(pInfo.PropertyType, val);
                        }
                        else
                            val = Convert.ChangeType(val, pInfo.PropertyType);
                    }
                    // Set the value of the property with the value from the db
                    if (pInfo.CanWrite)
                        pInfo.SetValue(returnObject, val, null);
                }
            }

            // return the entity object with values
            return returnObject;
        }

        private static void SetDefaultValue(ref object val, Type propertyType)
        {
            if (val is DBNull)
            {
                val = GetDefault(propertyType);
            }
        }

        public static object GetDefault(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        public static List<T> ConvertToList<T>(this DataTable table) where T : new()
        {
            // Create a list of the entities we want to return
            // Iterate through the DataTable's rows
            // Return the finished list
            return table.Rows.Cast<DataRow>().Select(dr => dr.ConvertToEntity<T>()).ToList();
        }

        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (var item in enumeration)
            {
                action(item);
            }
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            foreach (var element in source.Where(element => seenKeys.Add(keySelector(element))))
            {
                yield return element;
            }
        }

        public static bool IsNumeric(this string Text)
        {
            string lNumbers = "0123456789";
            if (string.IsNullOrEmpty(Text))
                return (false);

            for (int i = 0; i < Text.Length; i++)
            {
                if (!lNumbers.Contains(Text.Substring(i, 1)))
                    return (false);
            }

            return (true);
        }

        public static bool Intersects(this DisplayDaypart daypart1, DisplayDaypart daypart2)
        {
            return DisplayDaypart.Intersects(daypart1, daypart2);
        }

        public static bool Intersects(this DisplayDaypart daypart1, DateTime date, int time)
        {
            if (!daypart1.Days.Contains(date.DayOfWeek))
                return false;
            return DisplayDaypart.IsBetween(time, daypart1.StartTime, daypart1.EndTime);
        }

        public static bool Intersects(this DisplayDaypart daypart, Tam.Maestro.Services.ContractInterfaces.InventoryBusinessObjects.TimeSpan timeSpan)
        {
            return DisplayDaypart.Intersects(daypart.StartTime, daypart.EndTime, timeSpan.StartTime, timeSpan.EndTime);
        }
    }
}

namespace Tam.Maestro.Services.ContractInterfaces
{
    public static class Extensions2
    {
        public static void Raise<TEventArgs>(this TEventArgs e, object sender, ref EventHandler<TEventArgs> eventDelegate) where TEventArgs : EventArgs
        {
            //This is not Cargo Cult Programming (http://en.wikipedia.org/wiki/Cargo_cult_programming) - I actually understand it :)
            // Copy a reference to the delegate field now into a temporary field for thread safety
            EventHandler<TEventArgs> temp = System.Threading.Interlocked.CompareExchange(ref eventDelegate, null, null);
            // If any methods registered interest with our event, notify them
            if (temp != null)
                temp(sender, e);
        }

        public static byte[] ToByteArray(this object self)
        {
            if (self == null)
                return (null);

            using (MemoryStream lStream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(lStream, self);
                return lStream.ToArray();
            }
        }

        [Obsolete("This extension method has been deprecated.  Please use its replacement method FromBytes<T>.", false)]
        public static T[] FromByteArray<T>(this byte[] self)
        {
            using (MemoryStream lStream = new MemoryStream(self))
            {
                return (T[])new BinaryFormatter().Deserialize(lStream);
            }
        }

        public static T FromBytes<T>(this byte[] self) where T : class
        {
            if (self == null)
                return (null);

            using (MemoryStream lStream = new MemoryStream(self))
            {
                return (T)new BinaryFormatter().Deserialize(lStream);
            }
        }
    }
}