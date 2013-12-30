using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Linq;

namespace Sfx.JSON
{
    /// Based on Nikhil Kothari's dynamicrest:
    /// https://github.com/nikhilk/dynamicrest


    public sealed class JsonWriter
    {
        const int MAX_NEST_LEVEL = 30;
        StringWriter _internalWriter;
        IndentedTextWriter _writer;
        Stack<Scope> _scopes;
		bool minimizeWhitespace;

        public JsonWriter() : this(true)
        {
        }

        public JsonWriter(bool minimizeWhitespace) : this(new StringWriter(), minimizeWhitespace)
        {
            _internalWriter = (StringWriter)_writer.Target;
        }

        public JsonWriter(TextWriter writer): this(writer, /* minimizeWhitespace */ true)
        {
        }

        public JsonWriter(TextWriter writer, bool minimizeWhitespace)
        {
			this.minimizeWhitespace = minimizeWhitespace;
            _writer = new IndentedTextWriter(writer, minimizeWhitespace);
            _scopes = new Stack<Scope>();
        }

        public string Json
        {
            get
            {
                if (_internalWriter != null)
                {
                    return _internalWriter.ToString();
                }
                throw new InvalidOperationException("Only available when you create JsonWriter without passing in your own TextWriter.");
            }
        }

        public void EndScope()
        {
            if (_scopes.Count == 0)
            {
                throw new InvalidOperationException("No active scope to end.");
            }

            _writer.WriteLine();

            _writer.Indent--;

            Scope scope = _scopes.Pop();
            if (scope.Type == ScopeType.Array)
            {
                _writer.Write("]");
            }
            else
            {
                _writer.Write("}");
            }
        }

        public static string EscapeString(string s)
        {
            if (String.IsNullOrEmpty(s))
            {
                return String.Empty;
            }

            StringBuilder b = null;
            int startIndex = 0;
            int count = 0;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];

                // Append the unhandled characters (that do not require special treament)
                // to the string builder when special characters are detected.
                if (c == '\r' || c == '\t' || c == '\"' ||
                    c == '\\' || c == '\r' || c < ' ' || c > 0x7F)
                {
                    if (b == null)
                    {
                        b = new StringBuilder(s.Length + 6);
                    }

                    if (count > 0)
                    {
                        b.Append(s, startIndex, count);
                    }

                    startIndex = i + 1;
                    count = 0;
                }

                switch (c)
                {
                    case '\r':
                        b.Append("\\r");
                        break;
                    case '\t':
                        b.Append("\\t");
                        break;
                    case '\"':
                        b.Append("\\\"");
                        break;
                    case '\\':
                        b.Append("\\\\");
                        break;
                    case '\n':
                        b.Append("\\n");
                        break;
                    default:
                        if ((c < ' ') || (c > 0x7F))
                        {
                            b.AppendFormat(CultureInfo.InvariantCulture, "\\u{0:x4}", (int)c);
                        }
                        else
                        {
                            count++;
                        }
                        break;
                }
            }

            string processedString = s;
            if (b != null)
            {
                if (count > 0)
                {
                    b.Append(s, startIndex, count);
                }
                processedString = b.ToString();
            }

            return processedString;
        }

        public void StartArrayScope()
        {
            StartScope(ScopeType.Array);
        }

        public void StartObjectScope()
        {
            StartScope(ScopeType.Object);
        }

        private void StartScope(ScopeType type)
        {
            if (_scopes.Count != 0)
            {
                Scope currentScope = _scopes.Peek();
                if ((currentScope.Type == ScopeType.Array) &&
                    (currentScope.ObjectCount != 0))
                {
                    _writer.WriteTrimmed(", ");
                }

                currentScope.ObjectCount++;
            }

            Scope scope = new Scope(type);
            _scopes.Push(scope);

            if (type == ScopeType.Array)
            {
                _writer.Write("[");
            }
            else
            {
                _writer.Write("{");
            }
            _writer.Indent++;
            _writer.WriteLine();
        }

        public void WriteName(string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            if (_scopes.Count == 0)
            {
                throw new InvalidOperationException("No active scope to write into.");
            }
            if (_scopes.Peek().Type != ScopeType.Object)
            {
                throw new InvalidOperationException("Names can only be written into Object scopes.");
            }

            Scope currentScope = _scopes.Peek();
            if (currentScope.Type == ScopeType.Object)
            {
                if (currentScope.ObjectCount != 0)
                {
                    _writer.WriteLineTrimmed(", ");
                }

                currentScope.ObjectCount++;
            }

            _writer.Write("\"");
            _writer.Write(name);
            _writer.WriteTrimmed("\": ");
        }

        private void WriteCore(string text, bool quotes)
        {
            if (_scopes.Count != 0)
            {
                Scope currentScope = _scopes.Peek();
                if (currentScope.Type == ScopeType.Array)
                {
                    if (currentScope.ObjectCount != 0)
                    {
                        _writer.WriteLineTrimmed(", ");
                    }

                    currentScope.ObjectCount++;
                }
            }

            if (quotes)
            {
                _writer.Write('"');
            }
            _writer.Write(text);
            if (quotes)
            {
                _writer.Write('"');
            }
        }

        public void WriteValue(bool value)
        {
            WriteCore(value ? "true" : "false", /* quotes */ false);
        }

        public void WriteValue(int value)
        {
            WriteCore(value.ToString(CultureInfo.InvariantCulture), /* quotes */ false);
        }

        public void WriteValue(long value)
        {
            WriteCore(value.ToString(CultureInfo.InvariantCulture), /* quotes */ false);
        }

        public void WriteValue(decimal value)
        {
            WriteCore(value.ToString(CultureInfo.InvariantCulture), /* quotes */ false);
        }

        public void WriteValue(float value)
        {
            WriteCore(value.ToString(CultureInfo.InvariantCulture), /* quotes */ false);
        }

        public void WriteValue(double value)
        {
            WriteCore(value.ToString(CultureInfo.InvariantCulture), /* quotes */ false);
        }

        public void WriteValue(Enum value)
        {
            WriteCore(value.ToString(), /* quotes */ true);
        }

        public void WriteValue(IJsonSerializable value)
        {
            if (value == null)
            {
                WriteCore("null", /* quotes */ false);
            }
            else
            {
                WriteCore(value.Serialize(this.minimizeWhitespace), /* quotes */ false);
            }
        }

        public void WriteValue(DateTime dateTime)
        {
			WriteCore(EscapeString(dateTime.ToString("s", System.Globalization.CultureInfo.InvariantCulture)), /* quotes */ true);
        }

        public void WriteValue(string s)
        {
            if (s == null)
            {
                WriteCore("null", /* quotes */ false);
            }
            else
            {
                WriteCore(EscapeString(s), /* quotes */ true);
            }
        }

        public void WriteValue(ICollection items)
        {
            if ((items == null) || (items.Count == 0))
            {
                WriteCore("[]", /* quotes */ false);
            }
            else
            {
                StartArrayScope();

                foreach (object o in items)
                {
                    WriteValue(o);
                }

                EndScope();
            }
        }
        
        public void WriteValue(Byte[] items)
        {
            if (items == null)
            {
                WriteCore("[]", /* quotes */ false);
            }
            else
            {
                StartArrayScope();
                foreach (Byte o in items)
                {
                    WriteValue(o);
                }              
                EndScope();
            }
        }
        
        public void WriteValue(IEnumerable items)
        {
            if (items == null)
            {
                WriteCore("[]", /* quotes */ false);
            }
            else
            {
                StartArrayScope();
                
                foreach (object o in items)
                {
                    WriteValue(o);
                }
                
                EndScope();
            }
        }
		
		public void WriteValue(IDictionary record)
		{
			if ((record == null) || (record.Count == 0))
			{
				WriteCore("{}", /* quotes */ false);
			}
			else
			{
				StartObjectScope();
				
				foreach (DictionaryEntry entry in record)
				{
					string name = entry.Key as string;
					if (String.IsNullOrEmpty(name))
					{
						throw new ArgumentException("Key of unsupported type contained in Hashtable.");
					}
					
					WriteName(name);
					WriteValue(entry.Value);
				}
				
				EndScope();
			}
		}

        public void WriteValue(object o)
        {
            WriteValue(o, 0);
        }

        public void WriteValue(object o, int nestLevel)
        {
            if (nestLevel > MAX_NEST_LEVEL)
            {
                return;
            }

            if (o == null)
            {
                WriteCore("null", /* quotes */ false);
            }
            else if (o is bool)
            {
                WriteValue((bool)o);
            }
            else if (o is int)
            {
                WriteValue((int)o);
            }
            else if (o is long)
            {
                WriteValue((long)o);
            }
            else if (o is decimal)
            {
                WriteValue((decimal)o);
            }
            else if (o is float)
            {
                WriteValue((float)o);
            }
            else if (o is double)
            {
                WriteValue((double)o);
            }
            else if (o is DateTime)
            {
                WriteValue((DateTime)o);
            }
            else if (o is string)
            {
                WriteValue((string)o);
            }
            else if (o is Enum)
            {
                WriteValue((Enum)o);
            }
			else if (o is IJsonSerializable)
			{
				WriteValue((IJsonSerializable)o);
			}
			else if (o is IDictionary)
			{
				WriteValue((IDictionary)o);
			}
            else if (o is Byte[])
            {
                WriteValue((Byte[])o);
            }
            else if (o is ICollection)
            {
                WriteValue((ICollection)o);
            }
            else if (o is IEnumerable)
            {
                WriteValue((IEnumerable)o);
            }
            else
            {
				WriteObject(o, nestLevel);
            }
        }

		/// <summary>
		/// Serializa los campos y propiedades del objeto.   
		/// Es util por ejemplo para forzar serializar como objeto una clase que implemente IEnumerable
		/// ya que por defecto se serializaría como un array, ignorando las propiedades y los campos
		/// que pudiera definir.   
		/// Ejemplo:
		/// 
		///	    public sealed class RecordSet : IEnumerable<Record>, IJsonSerializable
		///	    {
		///			public string Serialize (bool minimice)
		///			{			
		///				var writer = new JsonWriter(minimice);
		///				writer.WriteObject(this, 0);
		///				return writer.Json;
		///			}
		///	    }
		/// 
		/// </summary>
		public void WriteObject (object o, int nestLevel)
		{
			StartObjectScope();
			
			// serializar las propiedades
			var propDescs = TypeDescriptor.GetProperties(o);
			foreach (PropertyDescriptor propDesc in propDescs)
			{
				if (!propDesc.Attributes.OfType<NotJsonSeralizableAttribute>().Any())
				{
					object value;

					try
					{
						value = propDesc.GetValue(o);
					}
					catch(Exception ex)
					{
						value = ex.Message;
					}

					WriteName(propDesc.Name);
					WriteValue(value, nestLevel + 1);
				}
			}
			
			// serializar las variables
			var fields = o.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
			foreach (var field in fields)
			{
				var value = field.GetValue(o);
				WriteName(field.Name);
				WriteValue(value, nestLevel + 1);
			}
			
			EndScope();
		}

        private enum ScopeType
        {
            Array = 0,
            Object = 1
        }

        private sealed class Scope
        {
            private int _objectCount;
            private ScopeType _type;

            public Scope(ScopeType type)
            {
                _type = type;
            }

            public int ObjectCount
            {
                get { return _objectCount; }
                set { _objectCount = value; }
            }

            public ScopeType Type
            {
                get { return _type; }
            }
        }
    }
}