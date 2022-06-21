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
        private bool throwOnErrors;
        private bool initialized;
        public PythonWrapperController(IPythonWrapperEngine pythonWrapperEngine, string pathToVirtualEnv, string pythonExecutableFolder, string pythonDll = "python37.dll", bool throwOnErrors = true, bool enableLogging = true)
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

        public PythonWrapperController(string pathToVirtualEnv, string pythonExecutableFolder, string pythonDll = "python37.dll", bool throwOnErrors = true, bool enableLogging = true)
            : this(new PythonWrapperNet(), pathToVirtualEnv, pythonExecutableFolder, pythonDll, throwOnErrors, enableLogging)
        {
        }
        public PythonWrapperController(string pathToVirtualEnv, string pythonExecutableFolder, IPythonLogger logger, string pythonDll = "python37.dll", bool throwOnErrors = true, bool enableLogging = true)
            : this(new PythonWrapperNet(logger), pathToVirtualEnv, pythonExecutableFolder, pythonDll, throwOnErrors, enableLogging)
        {
        }
        public void Initialize()
        {
            if (initialized)
            {
                return;

            }
            var path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
            var searchPath = new List<string>();
            if (!string.IsNullOrEmpty(pathToVirtualEnv) && Directory.Exists(pathToVirtualEnv))
            {
                string pathToLib = Path.Combine(pathToVirtualEnv, "lib");
                string pathToPackages = Path.Combine(pathToLib, "site-packages");
                string env = $"{pathToPackages};{pathToLib}";
                Environment.SetEnvironmentVariable("PYTHONPATH", env, EnvironmentVariableTarget.Process);
                path = $"{path};{pathToPackages};{pathToLib}";
                searchPath.Add(pathToLib);
                searchPath.Add(pathToPackages);
            }
            var finalPathEnv = path;

            if (!string.IsNullOrEmpty(pythonExecutableFolder) && Directory.Exists(pythonExecutableFolder))
            {
                finalPathEnv = $"{finalPathEnv};{pythonExecutableFolder}";
            }
            Environment.SetEnvironmentVariable("PATH", finalPathEnv, EnvironmentVariableTarget.Process);
            string pythonPath = Path.Combine(pythonExecutableFolder, pythonDll);
            try
            {
                if (File.Exists(pythonPath))
                {
                    Runtime.PythonDLL = pythonPath;
                }

                //Environment.SetEnvironmentVariable("PATH", pathToVirtualEnv, EnvironmentVariableTarget.Process);
                //Environment.SetEnvironmentVariable("PYTHONHOME", pathToVirtualEnv, EnvironmentVariableTarget.Process);
                //Environment.SetEnvironmentVariable("PYTHONPATH", $"{pathToVirtualEnv}\\Lib\\site-packages;{pathToVirtualEnv}\\Lib", EnvironmentVariableTarget.Process);
                var PYTHONPATH = Environment.GetEnvironmentVariable("PYTHONPATH", EnvironmentVariableTarget.Process);
                if (string.IsNullOrEmpty(PYTHONPATH))
                {
                    Environment.SetEnvironmentVariable("PYTHONPATH", finalPathEnv, EnvironmentVariableTarget.Process);

                }

                PythonEngine.PythonPath = PythonEngine.PythonPath + Path.PathSeparator + PYTHONPATH;

                if (!string.IsNullOrEmpty(pathToVirtualEnv))
                {
                    PythonEngine.PythonHome = pathToVirtualEnv;
                }
                PythonEngine.Initialize();
                pythonThreads = PythonEngine.BeginAllowThreads();
                _pythonWrapperEngine.AddSearchPaths(searchPath);
                var paths = _pythonWrapperEngine.PythonPaths();
                if (enableLogging)
                {
                    _pythonWrapperEngine.SetupLogger(throwOnErrors, out _);
                }

                initialized = true;

            }
            catch (Exception)
            {
                if (throwOnErrors)
                    throw;
            }


        }

        public T ExecuteCommandOrScript<T>(string script, out string log)
        {
            return _pythonWrapperEngine.ExecuteCommandOrScript<T>(script, throwOnErrors, out log);
        }

        public PyObject ImportScript(string fileName, out string log)
        {
            return _pythonWrapperEngine.ImportScript(fileName, throwOnErrors, out log);
        }

        public T ExecuteMethod<T>(string fileName, string methodName, out string log, params PyObject[] args)
        {
            return _pythonWrapperEngine.ExecuteMethod<T>(fileName, methodName, throwOnErrors, out log, args);
        }

        public T ExecuteMethodOnScriptObject<T>(PyObject script, string methodName, out string log,
            params PyObject[] args)
        {
            return _pythonWrapperEngine.ExecuteMethodOnScriptObject<T>(script, methodName, throwOnErrors, out log, args);

        }

        public string AddSearchPaths(List<string> paths)
        {
            if (initialized)
            {
                try
                {
                    _pythonWrapperEngine.AddSearchPaths(paths);
                    return _pythonWrapperEngine.PythonPaths();
                }
                catch (Exception)
                {
                    if (throwOnErrors)
                    {
                        throw;
                    }
                }
            }

            return string.Empty;
        }

        public string PythonPaths()
        {
            try
            {
                return _pythonWrapperEngine.PythonPaths();
            }
            catch (Exception e)
            {
                if (throwOnErrors)
                {
                    throw;
                }

                return $"ERROR :{e}";
            }
        }
        public void ShutDown()
        {
            try
            {
                if (pythonThreads != IntPtr.Zero)
                {
                    PythonEngine.EndAllowThreads(pythonThreads);
                }
                PythonEngine.Shutdown();
                initialized = false;

            }
            catch (Exception)
            {
                if (throwOnErrors)
                {
                    throw;
                }
            }
        }
    }
}
