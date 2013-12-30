using System;

namespace Sfx.JSON
{    
    public static class Json
    {
		public static bool Debug;

        public static string Serialize(object value)
        {
            return Serialize(value, true);
        }

        public static string Serialize(object value, bool minimize)
		{
			if(value == null)
			{
				return null;
			}

			var writer = new JsonWriter(minimize);
			writer.WriteValue(value);
			return writer.Json;
        }

        public static object Deserialize(string jsonText)
        {
			jsonText = jsonText.TrimStart(' ', '\n', '\r', '\t');

			if(jsonText[0] == '{')
			{
				return new JsonReader(jsonText).ReadObject();
			}
			else
			{
				return new JsonReader(jsonText).ReadValue();
			}
        }

        public static object Deserialize(string jsonText, Type type)
        {
            if (string.IsNullOrEmpty(jsonText))
            {
                return null;
            }

			var deserialized = Deserialize(jsonText);
            return Converter.Convert(deserialized, type, 0);
        }

        public static T Deserialize<T>(string jsonText)
        {
            if (string.IsNullOrEmpty(jsonText))
            {
                return default(T);
            }
			
			var deserialized = Deserialize(jsonText);
            return Converter.Convert<T>(deserialized);
        }
    }
}
