using System;
using System.Collections.Generic;
using System.IO;
using Python.Runtime;
using PythonNetWrapper.Interfaces;

namespace PythonNetWrapper
{
    public class PythonWrapperController : IPythonWrapperController
    {
        private readonly IPythonWrapperEngine _pythonWrapperEngine;
        private string pathToVirtualEnv;
        private string pythonExecutableFolder;
        private string pythonDll = "python37.dll";
        private IntPtr pythonThreads;
        private bool enableLogging;

        public PythonWrapperController(IPythonWrapperEngine pythonWrapperEngine, string pathToVirtualEnv, string pythonExecutableFolder, string pythonDll = "python37.dll", bool enableLogging = true)
        {
            _pythonWrapperEngine = pythonWrapperEngine;
            this.pathToVirtualEnv = pathToVirtualEnv;
            this.pythonExecutableFolder = pythonExecutableFolder;
            if (!string.IsNullOrEmpty(pythonDll))
            {
                this.pythonDll = pythonDll;
            }
            this.enableLogging = enableLogging;
        }

        public PythonWrapperController(string pathToVirtualEnv, string pythonExecutableFolder, string pythonDll = "python37.dll", bool enableLogging = true) : this(new PythonWrapperNet(), pathToVirtualEnv, pythonExecutableFolder, pythonDll, enableLogging)
        {
        }
        public void Initialize()
        {
            var path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
            string pathEnv = string.Empty;
            var searchPAth = new List<string>();
            if (!string.IsNullOrEmpty(pathToVirtualEnv) && Directory.Exists(pathToVirtualEnv))
            {
                string pathToLib = Path.Combine(pathToVirtualEnv, "lib");
                string pathToPackages = Path.Combine(pathToLib, "site-packages");
                string env = $"{pathToPackages};{pathToLib}";
                Environment.SetEnvironmentVariable("PYTHONPATH", env, EnvironmentVariableTarget.Process);
                pathEnv = path = $"{path};{pathToPackages};{pathToLib}";
                searchPAth.Add(pathToLib);
                searchPAth.Add(pathToPackages);
            }

            if (!string.IsNullOrEmpty(pythonExecutableFolder) && Directory.Exists(pythonExecutableFolder))
            {
                pathEnv = $"{path};{pythonExecutableFolder}";
                string pythonPath = Path.Combine(pythonExecutableFolder, pythonDll);
                if (File.Exists(pythonPath))
                    Runtime.PythonDLL = pythonPath;

            }

            if (!string.IsNullOrEmpty(pathEnv))
            {
                Environment.SetEnvironmentVariable("PATH", pathEnv, EnvironmentVariableTarget.Process);
            }

            //Environment.SetEnvironmentVariable("PATH", pathToVirtualEnv, EnvironmentVariableTarget.Process);
            //Environment.SetEnvironmentVariable("PYTHONHOME", pathToVirtualEnv, EnvironmentVariableTarget.Process);
            //Environment.SetEnvironmentVariable("PYTHONPATH", $"{pathToVirtualEnv}\\Lib\\site-packages;{pathToVirtualEnv}\\Lib", EnvironmentVariableTarget.Process);

            PythonEngine.PythonPath = PythonEngine.PythonPath + Path.PathSeparator +
                                                     Environment.GetEnvironmentVariable("PYTHONPATH", EnvironmentVariableTarget.Process);
            PythonEngine.PythonHome = pathToVirtualEnv;
            PythonEngine.Initialize();
            pythonThreads = PythonEngine.BeginAllowThreads();
            _pythonWrapperEngine.SetSearchPath(searchPAth);
            var paths = _pythonWrapperEngine.PythonPaths();
            if (enableLogging)
            {
                _pythonWrapperEngine.SetupLogger();
            }
        }

        public T RunScript<T>(string script, out string log)
        {
            return _pythonWrapperEngine.ExecuteCommand<T>(script, out log);
        }

        public T ImportScript<T>(string fileName, out string log)
        {
            return _pythonWrapperEngine.ImportScript<T>(fileName, out log);
        }

        public T ExecuteMethod<T>(string fileName, string methodName, out string log, params PyObject[] args)
        {
            return _pythonWrapperEngine.ExecuteMethod<T>(fileName, methodName, out log, args);
        }

        public T ExecuteMethodOnScriptObject<T>(PyObject script, string methodName, out string log,
            params PyObject[] args)
        {
            return _pythonWrapperEngine.ExecuteMethodOnScriptObject<T>(script, methodName, out log, args);

        }

        public void ShutDown()
        {
            if (pythonThreads != IntPtr.Zero)
            {
                PythonEngine.EndAllowThreads(pythonThreads);
            }
            PythonEngine.Shutdown();
        }
    }
}
