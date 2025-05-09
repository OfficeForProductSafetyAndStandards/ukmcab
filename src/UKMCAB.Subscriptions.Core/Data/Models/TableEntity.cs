using System.Globalization;

namespace UKMCAB.Subscriptions.Core.Data.Models;

public class TableEntity : ITableEntity
{
    private readonly IDictionary<string, object> _properties;

    public TableEntity(string tableKey, string partitionKey, string rowKey)
        : this(null)
    {
        TableKey = tableKey;
        PartitionKey = partitionKey;
        RowKey = rowKey;
    }

    public TableEntity(IDictionary<string, object> values)
    {
        _properties = values != null ?
            new Dictionary<string, object>(values) :
            new Dictionary<string, object>();

        Id = Guid.NewGuid();
    }
    public Guid? Id { get; internal set; }

    public string? TableKey
    {
        get { return GetString(TableConstants.PropertyNames.TableKey); }
        set { _properties[TableConstants.PropertyNames.TableKey] = value; }
    }

    public string PartitionKey
    {
        get { return GetString(TableConstants.PropertyNames.PartitionKey); }
        set { _properties[TableConstants.PropertyNames.PartitionKey] = value; }
    }

    public string RowKey
    {
        get { return GetString(TableConstants.PropertyNames.RowKey); }
        set { _properties[TableConstants.PropertyNames.RowKey] = value; }
    }

    public DateTimeOffset? Timestamp
    {
        get { return GetValue<DateTimeOffset?>(TableConstants.PropertyNames.Timestamp); }
        set { _properties[TableConstants.PropertyNames.Timestamp] = value; }
    }

    public void Add(string key, object value) => SetValue(key, value);

    private void SetValue(string key, object value)
    {
        if (value != null && _properties.TryGetValue(key, out object existingValue) && existingValue != null)
        {
            value = CoerceType(existingValue, value);
        }
        _properties[key] = value;
    }
    public string GetString(string key) => GetValue<string>(key);
    private T GetValue<T>(string key) => (T)GetValue(key, typeof(T));
    private object GetValue(string key, Type type = null)
    {
        if (!_properties.TryGetValue(key, out object value) || value == null)
        {
            return null;
        }

        if (type != null)
        {
            var valueType = value.GetType();
            if (type == typeof(DateTime?) && valueType == typeof(DateTimeOffset))
            {
                return ((DateTimeOffset)value).UtcDateTime;
            }
            if (type == typeof(DateTimeOffset?) && valueType == typeof(DateTime))
            {
                return new DateTimeOffset((DateTime)value);
            }
            if (type == typeof(BinaryData) && value is byte[] byteArray)
            {
                return new BinaryData(byteArray);
            }
            EnforceType(type, valueType);
        }

        return value;
    }
    private static void EnforceType(Type requestedType, Type givenType)
    {
        if (!requestedType.IsAssignableFrom(givenType))
        {
            throw new InvalidOperationException(string.Format(
                CultureInfo.InvariantCulture,
                $"Cannot return {requestedType} type for a {givenType} typed property."));
        }
    }
    private static object CoerceType(object existingValue, object newValue)
    {
        if (!existingValue.GetType().IsAssignableFrom(newValue.GetType()))
        {
            return existingValue switch
            {
                double _ => newValue switch
                {
                    // if we already had a double value, preserve it as double even if newValue was an int.
                    // example: entity["someDoubleValue"] = 5;
                    int newIntValue => (double)newIntValue,
                    _ => newValue
                },
                long _ => newValue switch
                {
                    // if we already had a long value, preserve it as long even if newValue was an int.
                    // example: entity["someLongValue"] = 5;
                    int newIntValue => (long)newIntValue,
                    _ => newValue
                },
                string when newValue is DateTime || newValue is DateTimeOffset _ => newValue.ToString(),
                _ => newValue
            };
        }

        return newValue;
    }

    internal static class TableConstants
    {
        internal static class PropertyNames
        {
            public const string Timestamp = "Timestamp";
            public const string TableKey = "TableKey";
            public const string PartitionKey = "PartitionKey";
            public const string RowKey = "RowKey";
        }
    }
}


