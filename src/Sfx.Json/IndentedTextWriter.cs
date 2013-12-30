using System;
using System.Text;
using System.Globalization;
using System.IO;
using System.Diagnostics;

namespace Sfx.JSON
{
    /// Based on Nikhil Kothari's dynamicrest:
    /// https://github.com/nikhilk/dynamicrest

    sealed class IndentedTextWriter : TextWriter
    {
        private readonly TextWriter _writer;

        private int _indentLevel;
        private bool _tabsPending;
        private readonly string _tabString;

        public IndentedTextWriter(TextWriter writer, bool minimize) : base(CultureInfo.InvariantCulture)
        {
            _writer = writer;
            this.Minimize = minimize;

            _writer.NewLine = "\n";
            _tabString = "    ";
            _indentLevel = 0;
            _tabsPending = false;
        }

        public bool Minimize { get; set; }

        public override Encoding Encoding
        {
            get { return _writer.Encoding; }
        }
        
        public int Indent
        {
            get {  return _indentLevel; }
            set
            {
                Debug.Assert(value >= 0);
                if (value < 0)
                {
                    value = 0;
                }
                _indentLevel = value;
            }
        }

        public TextWriter Target
        {
            get
            {
                return _writer;
            }
        }

        public override void Close()
        {
            _writer.Close();
        }

        public override void Flush()
        {
            _writer.Flush();
        }

        private void OutputTabs()
        {
            if (_tabsPending)
            {
                if (this.Minimize  == false)
                {
                    for (int i = 0; i < _indentLevel; i++)
                    {
                        _writer.Write(_tabString);
                    }
                }
                _tabsPending = false;
            }
        }

        public override void Write(string s)
        {
            OutputTabs();
            _writer.Write(s);
        }
        
        public override void Write(bool value)
        {
            OutputTabs();
            _writer.Write(value);
        }
        
        public void Write(byte[] value)
        {
            OutputTabs();
            _writer.Write(value);
        }

        public override void Write(char value)
        {
            OutputTabs();
            _writer.Write(value);
        }

        public override void Write(char[] buffer)
        {
            OutputTabs();
            _writer.Write(buffer);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            OutputTabs();
            _writer.Write(buffer, index, count);
        }

        public override void Write(double value)
        {
            OutputTabs();
            _writer.Write(value);
        }

        public override void Write(float value)
        {
            OutputTabs();
            _writer.Write(value);
        }

        public override void Write(int value)
        {
            OutputTabs();
            _writer.Write(value);
        }

        public override void Write(long value)
        {
            OutputTabs();
            _writer.Write(value);
        }

        public override void Write(object value)
        {
            OutputTabs();
            _writer.Write(value);
        }

        public override void Write(string format, object arg0)
        {
            OutputTabs();
            _writer.Write(format, arg0);
        }

        public override void Write(string format, object arg0, object arg1)
        {
            OutputTabs();
            _writer.Write(format, arg0, arg1);
        }

        public override void Write(string format, params object[] arg)
        {
            OutputTabs();
            _writer.Write(format, arg);
        }

        public override void WriteLine(string s)
        {
            OutputTabs();
            _writer.WriteLine(s);
            _tabsPending = true;
        }

        public override void WriteLine()
        {
            if (!this.Minimize )
            {
                OutputTabs();
                _writer.WriteLine();
                _tabsPending = true;
            }
        }

        public override void WriteLine(bool value)
        {
            OutputTabs();
            _writer.WriteLine(value);
            _tabsPending = true;
        }

        public override void WriteLine(char value)
        {
            OutputTabs();
            _writer.WriteLine(value);
            _tabsPending = true;
        }

        public override void WriteLine(char[] buffer)
        {
            OutputTabs();
            _writer.WriteLine(buffer);
            _tabsPending = true;
        }

        public override void WriteLine(char[] buffer, int index, int count)
        {
            OutputTabs();
            _writer.WriteLine(buffer, index, count);
            _tabsPending = true;
        }

        public override void WriteLine(double value)
        {
            OutputTabs();
            _writer.WriteLine(value);
            _tabsPending = true;
        }

        public override void WriteLine(float value)
        {
            OutputTabs();
            _writer.WriteLine(value);
            _tabsPending = true;
        }

        public override void WriteLine(int value)
        {
            OutputTabs();
            _writer.WriteLine(value);
            _tabsPending = true;
        }

        public override void WriteLine(long value)
        {
            OutputTabs();
            _writer.WriteLine(value);
            _tabsPending = true;
        }

        public override void WriteLine(object value)
        {
            OutputTabs();
            _writer.WriteLine(value);
            _tabsPending = true;
        }

        public override void WriteLine(string format, object arg0)
        {
            OutputTabs();
            _writer.WriteLine(format, arg0);
            _tabsPending = true;
        }

        public override void WriteLine(string format, object arg0, object arg1)
        {
            OutputTabs();
            _writer.WriteLine(format, arg0, arg1);
            _tabsPending = true;
        }

        public override void WriteLine(string format, params object[] arg)
        {
            OutputTabs();
            _writer.WriteLine(format, arg);
            _tabsPending = true;
        }

        public override void WriteLine(UInt32 value)
        {
            OutputTabs();
            _writer.WriteLine(value);
            _tabsPending = true;
        }

        public void WriteSignificantNewLine()
        {
            if (!this.Minimize )
            {
                WriteLine();
            }
        }

        public void WriteNewLine()
        {
            if (!this.Minimize )
            {
                WriteLine();
            }
        }

        public void WriteTrimmed(string text)
        {
            if (!this.Minimize )
            {
                Write(text);
            }
            else
            {
                Write(text.Trim());
            }
        }

        public void WriteLineTrimmed(string text)
        {
            if (!this.Minimize )
            {
                WriteLine(text);
            }
            else
            {
                WriteTrimmed(text.Trim());
            }
        }
    }
}
