using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Sfx.JSON
{	
    static class Converter
    {
        const int MAX_NEST_LEVEL = 10;

        public static T Convert<T>(object source)
        {
            return (T)Convert(source, typeof(T), 0);
        }

        public static object Convert(object source, Type type, int nestLevel)
        {
            if (nestLevel > MAX_NEST_LEVEL)
            {
                return type.IsValueType ? Activator.CreateInstance(type) : null;
            }

            object value;
            
            if (typeof(IJsonDeserializable).IsAssignableFrom(type))
            {
				value = ((IJsonDeserializable)Activator.CreateInstance(type)).Deserialize(source);
            }
            else if (type.IsValueType)
            {
                type = Nullable.GetUnderlyingType(type) ?? type;

                if (source == null)
                {
                    value = null;
                }
                else if (type.IsEnum)
                {
                    value = Enum.Parse(type, source.ToString());
                }
                else
                {
                    value = System.Convert.ChangeType(source, type, CultureInfo.InvariantCulture);
                }
            }
            else if (type == typeof(string))
            {
                value = source.ToString();
            }
            else if (type == typeof(object))
            {
                value = source;
            }
            else if (typeof(IList).IsAssignableFrom(type))
            {
                value = Activator.CreateInstance(type);
				var listValue = value as IList;				
				var listSource = source as List<object>;

                var handled = false;

                var genericType = GetGenericBaseType(type);

                if (genericType != null)
                {
                    var arguments = genericType.GetGenericArguments();

                    if (arguments.Length == 1) // es un NameValuexxxx
                    {
                        handled = true;
						foreach (var item in listSource)
                        {
							listValue.Add(Convert(item, arguments[0], nestLevel + 1));
                        }
                    }
                }

                if (!handled)
                {
					foreach (var item in listSource)
                    {
						listValue.Add(item);
                    }
                }
            }
            else if (typeof(IDictionary).IsAssignableFrom(type))
            {
                value = Activator.CreateInstance(type);
				var dictValue = value as IDictionary;				
				var dictSource = source as IDictionary<string, object>;

                var handled = false;

                if (type.IsGenericType)
                {
                    var arguments = type.GetGenericArguments();

                    if (arguments.Length == 1) // es un NameValuexxxx
                    {
                        handled = true;

						foreach (var item in dictSource)
                        {
							dictValue.Add(item.Key, Convert(item.Value, arguments[0], nestLevel + 1));
                        }
                    }
                }

                if (!handled)
                {
					foreach (var item in dictSource)
                    {
						dictValue.Add(item.Key, item.Value);
                    }
                }
            }
            else
            {
                value = Activator.CreateInstance(type);
				var dictSource = source as IDictionary<string, object>;

                foreach (var propertyInfo in type.GetProperties())
				{
					if(Attribute.IsDefined(propertyInfo, typeof(NotJsonSeralizableAttribute)))
					{
						continue;
					}

                    if (propertyInfo.CanWrite)
                    {
						object propertyValue;
						if(dictSource.TryGetValue(propertyInfo.Name, out propertyValue))
						{
	                        if (propertyValue != null)
	                        {
	                            propertyInfo.SetValue(value, Convert(propertyValue, propertyInfo.PropertyType, nestLevel + 1), new object[] { });
	                        }
						}
                    }
                }
            
                foreach(var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
					object fieldValue;
					if(dictSource.TryGetValue(field.Name, out fieldValue))
					{
	                    if(fieldValue != null)
	                    {
	                        field.SetValue(value, Convert(fieldValue, field.FieldType, nestLevel + 1));
	                    }
					}
                }
            }

            return value;
        }

		static Type GetGenericBaseType(Type type)
		{
			while (type != null && type != typeof(object))
			{
				if (type.IsGenericType)
				{
					return type;
				}
				type = type.BaseType;
			}           
			return null;
		}
    }
}
