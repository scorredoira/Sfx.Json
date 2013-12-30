using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Sfx.JSON
{
    /// Based on Nikhil Kothari's dynamicrest:
    /// https://github.com/nikhilk/dynamicrest

    sealed class JsonReader
    {
        private TextReader reader;
		int columnIndex;
		int lineIndex;
		char lastChar;
		const int EOF = -1;

        public JsonReader(string jsonText)
            : this(new StringReader(jsonText))
        {
        }

        public JsonReader(TextReader reader)
        {
            this.reader = reader;
        }
		
		char Peek()
		{
			var c = reader.Peek();
			if(c == EOF)
			{
				return '\0';
			}
			return (char)c;
		}

		char Read()
		{
			var c = reader.Read();

			if(Json.Debug)
			{
				Console.Write((char)c);
			}

			if(c == EOF)
			{
				return '\0';
			}

			var cc = (char)c;
			if(!char.IsWhiteSpace(cc))
			{
				columnIndex++;
			}

			if(lastChar == '\n')
			{
				lineIndex++;
				columnIndex = 0;
			}

			lastChar = cc;
			return cc;
		}

		private string GetErrorMessage(string error)
		{
			return string.Format("Error at line {0}, col {1}: {2}", lineIndex, columnIndex, error);
		}

        private char GetNextSignificantCharacter()
        {
            char ch = this.Read();
            while ((ch != '\0') && Char.IsWhiteSpace(ch))
            {
                ch = this.Read();
            }
            return ch;
        }

        private string GetCharacters(int count)
        {
            string s = String.Empty;
            for (int i = 0; i < count; i++)
            {
                char ch = this.Read();
                if (ch == '\0')
                {
                    return null;
                }
                s += ch;
            }
            return s;
        }

        private char PeekNextSignificantCharacter()
        {
			while(true)
			{
				char ch = Peek();

				if(ch == '\0')
				{
					return ch;
				}
					
				if(ch == '#')
				{
					ReadComment();
					continue;
				}

				if(Char.IsWhiteSpace(ch))
				{
					this.Read();
					continue;
				}

				return ch;
			}
		}

		private void ReadComment()
		{
			while(true)
			{
				char ch = Peek();

				if(ch == '\0')
				{
					return;
				}

				if(ch == '\n')
				{
					return;
				}

				this.Read();
			}
		}

        private List<object> ReadArray()
        {
			var array = new List<object>();

            // Consume the '['
            this.Read();

            while (true)
            {
                char ch = PeekNextSignificantCharacter();
                if (ch == '\0')
                {
					throw new FormatException(GetErrorMessage("Unterminated array literal."));
                }

                if (ch == ']')
                {
                    this.Read();
                    return array;
                }

				if (array.Count != 0)
                {
                    if (ch != ',')
                    {
						throw new FormatException(GetErrorMessage("Invalid array literal."));
                    }
                    else
                    {
                        this.Read();
                    }
                }

                object item = ReadValue();
				array.Add(item);
            }
        }

        private bool ReadBoolean()
        {
            string s = ReadName(/* allowQuotes */ false);

            if (s != null)
            {
                if (s.Equals("true", StringComparison.Ordinal))
                {
                    return true;
                }
                else if (s.Equals("false", StringComparison.Ordinal))
                {
                    return false;
                }
            }

			throw new FormatException(GetErrorMessage("Invalid boolean literal."));
        }

        private string ReadName(bool allowQuotes)
        {
            char ch = PeekNextSignificantCharacter();

            if ((ch == '"') || (ch == '\''))
            {
                if (allowQuotes)
                {
                    return ReadString();
                }
            }
            else
            {
                StringBuilder sb = new StringBuilder();

                while (true)
                {
                    ch = Peek();
                    if ((ch == '_') || Char.IsLetterOrDigit(ch))
                    {
                        this.Read();
                        sb.Append(ch);
                    }
                    else
                    {
                        return sb.ToString();
                    }
                }
            }

            return null;
        }

        private void ReadNull()
        {
            string s = ReadName(/* allowQuotes */ false);

            if ((s == null) || !s.Equals("null", StringComparison.Ordinal))
            {
				throw new FormatException(GetErrorMessage("Invalid null literal."));
            }
        }

        private object ReadNumber()
        {
            char ch = this.Read();

            StringBuilder sb = new StringBuilder();
            bool hasDecimal = (ch == '.');

            sb.Append(ch);
            while (true)
            {
                ch = PeekNextSignificantCharacter();

                if (Char.IsDigit(ch) || (ch == '.'))
                {
                    hasDecimal = hasDecimal || (ch == '.');

                    this.Read();
                    sb.Append(ch);
                }
                else
                {
                    break;
                }
            }

            string s = sb.ToString();
            if (hasDecimal)
            {
				decimal value;
				if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                {
                    return value;
                }
            }
            else
            {
                int value;
                if (Int32.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                {
                    return value;
                }
                else
                {
                    long lvalue;
                    if (Int64.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out lvalue))
                    {
                        return lvalue;
                    }
                }
            }

			throw new FormatException(GetErrorMessage("Invalid numeric literal."));
        }

		public Dictionary<string, object> ReadObject()
        {
			var record = new Dictionary<string, object>();

            // trim white chars
            PeekNextSignificantCharacter();

            // Consume the '{'
            this.Read();

            while (true)
            {
                char ch = PeekNextSignificantCharacter();
                if (ch == '\0')
                {
					throw new FormatException(GetErrorMessage("Unterminated object literal."));
                }

                if (ch == '}')
                {
                    this.Read();
                    return record;
                }

				if (record.Count != 0)
                {
                    if (ch != ',')
                    {
						throw new FormatException(GetErrorMessage("Invalid object literal."));
                    }
                    else
                    {
                        this.Read();
                    }
                }

                string name = ReadName(/* allowQuotes */ true);
                ch = PeekNextSignificantCharacter();

                if (ch != ':')
                {
					throw new FormatException(GetErrorMessage(
						"Unexpected name/value pair syntax in object literal"));
                }
                else
                {
                    this.Read();
                }

                object item = ReadValue();
				record[name] = item;
            }
        }

        private string ReadString()
        {
            bool dummy;
            return ReadString(out dummy);
        }

        private string ReadString(out bool hasLeadingSlash)
        {
            StringBuilder sb = new StringBuilder();

            char endQuoteCharacter = this.Read();
            bool inEscape = false;
            bool firstCharacter = true;

            hasLeadingSlash = false;

            while (true)
            {
                char ch = Read();
                if (ch == '\0')
                {
					throw new FormatException(GetErrorMessage("Unterminated string literal."));
                }
                if (firstCharacter)
                {
                    if (ch == '\\')
                    {
                        hasLeadingSlash = true;
                    }
                    firstCharacter = false;
                }

                if (inEscape)
                {
                    if (ch == 'u')
                    {
                        string unicodeSequence = GetCharacters(4);
                        if (unicodeSequence == null)
                        {
							throw new FormatException(GetErrorMessage("Unterminated string literal."));
                        }
                        ch = (char)Int32.Parse(unicodeSequence, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    }

                    sb.Append(ch);
                    inEscape = false;
                    continue;
                }

                if (ch == '\\')
                {
                    inEscape = true;
                    continue;
                }

                if (ch == endQuoteCharacter)
                {
                    return sb.ToString();
                }

                sb.Append(ch);
            }
        }

        public object ReadValue()
        {
            object value = null;
            bool allowNull = false;

            char ch = PeekNextSignificantCharacter();
            if (ch == '[')
            {
                value = ReadArray();
            }
            else if (ch == '{')
            {
                value = ReadObject();
            }
            else if ((ch == '\'') || (ch == '"'))
            {
                value = ReadString();
            }
            else if (Char.IsDigit(ch) || (ch == '-') || (ch == '.'))
            {
                value = ReadNumber();
            }
            else if ((ch == 't') || (ch == 'f'))
            {
                value = ReadBoolean();
            }
            else if (ch == 'n')
            {
                ReadNull();
                allowNull = true;
            }

            if ((value == null) && (allowNull == false))
            {
				throw new FormatException(GetErrorMessage(string.Format(
					"Invalid JSON text near '{0}'. Verify that all strings are in quotes.", value)));
            }

            return value;
        }
    }
}
