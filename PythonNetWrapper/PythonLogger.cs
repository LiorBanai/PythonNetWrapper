using System;
using System.Collections.Generic;
using System.Diagnostics;
using PythonNetWrapper.Interfaces;

namespace PythonNetWrapper
{
    public class PythonLogger : IPythonLogger
    {
        private readonly List<string> _buffer;

        public PythonLogger()
        {
            _buffer = new List<string>();
        }

        public void close()
        {
            _buffer.Clear();
        }

        public void flush()
        {
            _buffer.Clear();
        }

        public string ReadStream()
        {
            var str = string.Join(Environment.NewLine, _buffer);
            return str;
        }

        public void write(string str)
        {
            if (str == "\n")
            {
                return;
            }

            _buffer.Add(str);
            Trace.WriteLine(str);
        }

        public void writelines(string[] str)
        {
            _buffer.AddRange(str);
        }
    }
}
