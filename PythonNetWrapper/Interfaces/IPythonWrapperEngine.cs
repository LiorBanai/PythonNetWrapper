using System;
using System.Collections.Generic;
using Autofac;
using Python.Runtime;

namespace PythonNetWrapper.Interfaces
{
    public interface IPythonWrapperEngine : IDisposable
    {
        /// <summary>
        /// Executes a Python command text or script 
        /// </summary>
        /// <typeparam name="T">the return type of the result (Or PyObject if no result are returned)</typeparam>
        /// <param name="command">The text to execute</param>
        /// <param name="throwOnErrors"></param>
        /// <param name="log">Output log print method (if Logger is setup)</param>
        /// <returns>The result (or PyObject that is PyObject.None)</returns>
        T ExecuteCommand<T>(string command, bool throwOnErrors, out string log);
        /// <summary>
        /// Call Py.Import on the GetFileNameWithoutExtension of the file.
        /// </summary>
        /// <param name="fileName">The file name of the py file to import</param>
        /// <param name="throwOnErrors"></param>
        /// <param name="log">Output log print method (if Logger is setup)</param>
        /// <returns></returns>
        PyObject ImportScript(string fileName, bool throwOnErrors, out string log);
        /// <summary>
        /// Import the file name and call the specific method
        /// </summary>
        /// <typeparam name="T">The return type of the result (Or PyObject, that is PyObject.None, if no result are returned)</typeparam>
        /// <param name="fileName"></param>
        /// <param name="methodName"></param>
        /// <param name="throwOnErrors"></param>
        /// <param name="log">Output log print method (if Logger is setup)</param>
        /// <param name="args">The arguments to the method</param>
        /// <returns>The result of the method (or PyObject that is PyObject.None if no result is returned)</returns>
        T ExecuteMethod<T>(string fileName, string methodName, bool throwOnErrors, out string log, params PyObject[] args);
        /// <summary>
        /// Execute a method on an imported script Object
        /// </summary>
        /// <typeparam name="T">The return type of the result (Or PyObject, that is PyObject.None, if no result are returned)</typeparam>
        /// <param name="script">the imported script Object</param>
        /// <param name="methodName">The method to call on the script object</param>
        /// <param name="throwOnErrors"></param>
        /// <param name="log">Output log print method (if Logger is setup)</param>
        /// <param name="args">The arguments to the method</param>
        /// <returns></returns>
        T ExecuteMethodOnScriptObject<T>(PyObject script, string methodName, bool throwOnErrors, out string log, params PyObject[] args);
        // sets an object in Python's scope
        void SetVariable(string name, object value);
        /// <summary>
        /// Capture the output of the build in logger
        /// </summary>
        /// <param name="throwOnErrors"></param>
        void SetupLogger(bool throwOnErrors);
        /// <summary>
        /// Python's search path
        /// </summary>
        /// <returns>List Of Python Paths (sys.path)</returns>
        IList<string> SearchPaths();
        string PythonPaths();

        /// <summary>
        /// Add search paths (to sys.path)
        /// </summary>
        /// <param name="paths">list of paths to add</param>
        void SetSearchPath(IList<string> paths);

        // initializes this engine with the app container
        void Initialize(IContainer appContainer);
    }
}
