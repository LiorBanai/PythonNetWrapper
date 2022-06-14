<h1 align="left">PythonNetWrapper <img src="./Assets/PythonnetWrapper.png" align="right" width="155px" height="155px"></h1> 

![.NET Core Desktop](https://github.com/LiorBanai/PythonNetWrapper/workflows/.NET%20Core%20Desktop/badge.svg)
<a href="https://github.com/LiorBanai/PythonNetWrapper/issues">
    <img src="https://img.shields.io/github/issues/LiorBanai/PythonNetWrapper"  alt="Issues"/>
</a> ![GitHub closed issues](https://img.shields.io/github/issues-closed-raw/LiorBanai/PythonNetWrapper)
<a href="https://github.com/LiorBanai/PythonNetWrapper/blob/master/LICENSE">
    <img src="https://img.shields.io/github/license/LiorBanai/PythonNetWrapper"  alt="License"/>
</a>
[![Nuget](https://img.shields.io/nuget/v/PythonNetWrapper)](https://www.nuget.org/packages/PythonNetWrapper/)
[![Nuget](https://img.shields.io/nuget/dt/PythonNetWrapper)](https://www.nuget.org/packages/PythonNetWrapper/) [![Donate](https://www.paypalobjects.com/en_US/i/btn/btn_donate_SM.gif)](https://www.paypal.com/donate/?business=MCP57TBRAAVXA&no_recurring=0&item_name=Support+Open+source+Projects+%28Analogy+Log+Viewer%2C+HDF5-CSHARP%2C+etc%29&currency_code=USD)

# 
a library for executing pythonnet in C# projects

This library  allows you to run python scripts or call python methods inside your C# project.
the basic usage is just to create PythonWrapperController object and use it methods (call Initialize before use):


```cs
public interface IPythonWrapperController
    {
        /// <summary>
        /// Initialize the Python Engine (must called first)
        /// </summary>
        void Initialize();
        /// <summary>
        /// Shutdown the Python Engine
        /// </summary>
        void ShutDown();
        /// <summary>
        /// Add search paths (each one needs to ends with /)
        /// </summary>
        /// <param name="paths">list of paths</param>
        /// <returns>The os and sys paths after adding</returns>
        string AddSearchPaths(List<string> paths);
        /// <summary>
        /// the os and sys paths
        /// </summary>
        /// <returns></returns>
        string PythonPaths();
        /// <summary>
        /// Call Py.Import on the GetFileNameWithoutExtension of the file.
        /// </summary>
        /// <param name="fileName">The file name of the py file to import</param> <param name="log">Output log print method (if Logger is setup)</param>
        /// <returns></returns>
        PyObject ImportScript(string fileName, out string log);
        /// <summary>
        /// Executes a Python command text or script 
        /// </summary>
        /// <typeparam name="T">the return type of the result (Or PyObject if no result are returned)</typeparam>
        /// <param name="textCommandOrScript">The text to execute</param>
        /// <param name="log">Output log print method (if Logger is setup)</param>
        /// <returns>The result (or PyObject that is PyObject.None)</returns>
        T ExecuteCommandOrScript<T>(string textCommandOrScript, out string log);
        /// <summary>
        /// Import the file name and call the specific method
        /// </summary>
        /// <typeparam name="T">The return type of the result (Or PyObject, that is PyObject.None, if no result are returned)</typeparam>
        /// <param name="fileName"></param>
        /// <param name="methodName"></param>
        /// <param name="log">Output log print method (if Logger is setup)</param>
        /// <param name="args">The arguments to the method</param>
        /// <returns>The result of the method (or PyObject that is PyObject.None if no result is returned)</returns>
        T ExecuteMethod<T>(string fileName, string methodName, out string log, params PyObject[] args);
        /// <summary>
        /// Execute a method on an imported script Object
        /// </summary>
        /// <typeparam name="T">The return type of the result (Or PyObject, that is PyObject.None, if no result are returned)</typeparam>
        /// <param name="script">the imported script Object</param>
        /// <param name="methodName">The method to call on the script object</param>
        /// <param name="log">Output log print method (if Logger is setup)</param>
        /// <param name="args">The arguments to the method</param>
        /// <returns></returns>
        T ExecuteMethodOnScriptObject<T>(PyObject script, string methodName, out string log, params PyObject[] args);
    }
```


```cs  
   public class PythonWrapperController : IPythonWrapperController
    {
        private readonly IPythonWrapperEngine _pythonWrapperEngine;
        private string pathToVirtualEnv;
        private string pythonExecutableFolder;
        private string pythonDll = "python37.dll";
        private IntPtr pythonThreads;
        private bool enableLogging;
        private bool throwOnErrors;
        private bool initialized;
        public PythonWrapperController(IPythonWrapperEngine pythonWrapperEngine, string pathToVirtualEnv, string pythonExecutableFolder,
        string pythonDll = "python37.dll", bool throwOnErrors = true, bool enableLogging = true)
        {
            _pythonWrapperEngine = pythonWrapperEngine;
            this.pathToVirtualEnv = pathToVirtualEnv;
            this.pythonExecutableFolder = pythonExecutableFolder;
            if (!string.IsNullOrEmpty(pythonDll))
            {
                this.pythonDll = pythonDll;
            }
            this.enableLogging = enableLogging;
            this.throwOnErrors = throwOnErrors;
        }

        public PythonWrapperController(string pathToVirtualEnv, string pythonExecutableFolder, string pythonDll = "python37.dll",
        bool throwOnErrors = true, bool enableLogging = true) 
            : this(new PythonWrapperNet(), pathToVirtualEnv, pythonExecutableFolder, pythonDll, throwOnErrors, enableLogging)
        {
        }
        public PythonWrapperController(string pathToVirtualEnv, string pythonExecutableFolder, IPythonLogger logger,
        string pythonDll = "python37.dll", bool throwOnErrors = true, bool enableLogging = true)
            : this(new PythonWrapperNet(logger), pathToVirtualEnv, pythonExecutableFolder, pythonDll, throwOnErrors, enableLogging)
        {
        }
         ..
```


you have the following options:
1. ImportScript: allows you to import a script and call methods on that script.
2. RunScript: content of py file to execute (the text itself , not the file path).
3. ExecuteMethod: execute a method of a file (the file is imported and the method is called.)
