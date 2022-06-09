﻿namespace PythonnetWrapper.Interfaces
{
    interface IPythonLogger
    {
        // flushes the buffer
        void flush();

        // writes str to the logger's buffer
        void write(string str);

        // writes multiple lines of strings to the logger's buffer
        void writelines(string[] str);

        // closes the buffer
        void close();

        // Reads the stream
        string ReadStream();
    }
}
