using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Autofac;
using Python.Runtime;
using PythonNetWrapper.Interfaces;

namespace PythonNetWrapper
{
    public class PythonWrapperNet : IPythonWrapperEngine
    {
        private Lazy<PyModule> module;
        private readonly PythonLogger _logger = new PythonLogger();
        private List<string> existingPaths = new List<string>();
        public PythonWrapperNet()
        {
            module = new Lazy<PyModule>(Py.CreateScope);
        }

        public void Dispose()
        {
            module.Value.Dispose();
        }

        public string PythonPaths()
        {
            try
            {
                using (Py.GIL())
                {
                    dynamic sys = Py.Import("sys");
                    dynamic os = Py.Import("os");
                    StringBuilder sb = new StringBuilder($"PYTHONHOME: {os.getenv("PYTHONHOME")}");
                    sb.AppendLine($"PYTHONPATH: {os.getenv("PYTHONPATH")}");
                    sb.AppendLine($"sys.executable: {sys.executable}");
                    sb.AppendLine($"sys.prefix: {sys.prefix}");
                    sb.AppendLine($"sys.base_prefix: {sys.base_prefix}");
                    sb.AppendLine($"sys.exec_prefix: {sys.exec_prefix}");
                    sb.AppendLine($"sys.base_exec_prefix: {sys.base_exec_prefix}");
                    sb.AppendLine("sys.path:");
                    foreach (var p in sys.path)
                    {
                        sb.AppendLine(p.ToString());
                    }

                    return sb.ToString();
                }
            }
            catch (Exception ex)
            {
                return $"Python Error: {ex} ({nameof(ExecuteMethodOnScriptObject)})";
            }
        }

        public void AddSearchPaths(List<string> paths)
        {
            var searchPaths = paths.Where(Directory.Exists).Distinct().ToList();

            using (Py.GIL())
            {
                dynamic sys = Py.Import("sys");
                dynamic os = Py.Import("os");
                foreach (string path in searchPaths)
                {
                    sys.path.append(os.path.dirname(os.path.expanduser(path)));
                }
            }

        }

        public T ExecuteCommandOrScript<T>(string command, bool throwOnErrors, out string log)
        {
            T result = default;
            log = "";
            try
            {
                using (Py.GIL())
                {
                    var pyCompile = PythonEngine.Compile(command);
                    result = module.Value.Execute(pyCompile).As<T>();
                    log = _logger.ReadStream();
                    _logger.flush();
                }
            }
            catch (Exception ex)
            {
                log = $"Python Error: {ex} ({nameof(ExecuteCommandOrScript)})";
                if (throwOnErrors)
                {
                    throw;
                }
            }

            return result;
        }

        private void AddToPathIfNeeded(string fileName,dynamic sys, dynamic os)
        {
            string filePath = Path.GetDirectoryName(fileName);
            bool pathExist = existingPaths.Contains(fileName);
            if (!pathExist)
            {
                foreach (var p in sys.path)
                {
                    var pythonPath = p.ToString();
                    existingPaths.Add(pythonPath);
                    if (pythonPath.Contains(filePath))
                    {
                        pathExist = true;
                        break;
                    }
                }
            }

            if (!pathExist)
            {
                var newPath = os.path.dirname(os.path.expanduser(fileName));
                sys.path.append(newPath);
                existingPaths.Add(newPath.ToString());
            }
        }
        public PyObject ImportScript(string fileName, bool throwOnErrors, out string log)
        {
            PyObject result = default;
            log = "";

            try
            {
                using (Py.GIL())
                {
                    dynamic sys = Py.Import("sys");
                    dynamic os = Py.Import("os");
                    AddToPathIfNeeded(fileName,sys,os);
                    result = Py.Import(Path.GetFileNameWithoutExtension(fileName));
                    log = _logger.ReadStream();
                    _logger.flush();
                }
            }
            catch (Exception ex)
            {
                log = $"Python Error: {ex} ({nameof(ImportScript)})";
                if (throwOnErrors)
                {
                    throw;
                }
            }

            return result;

        }

        public T ExecuteMethodOnScriptObject<T>(PyObject script, string methodName, bool throwOnErrors, out string log, params PyObject[] args)
        {
            T result = default;

            try
            {
                using (Py.GIL())
                {
                    result = script.InvokeMethod(methodName, args).As<T>();
                    log = _logger.ReadStream();
                    _logger.flush();
                    return result;
                }
            }
            catch (Exception ex)
            {
                log = $"Python Error: {ex} ({nameof(ExecuteMethodOnScriptObject)})";
                if (throwOnErrors)
                {
                    throw;
                }
            }

            return result;
        }
        public T ExecuteMethod<T>(string fileName, string methodName, bool throwOnErrors, out string log, params PyObject[] args)
        {
            T result = default;
            log = "";

            try
            {
                using (Py.GIL())
                {
                    dynamic sys = Py.Import("sys");
                    dynamic os = Py.Import("os");
                    AddToPathIfNeeded(fileName,sys,os);
                    PyObject fromFile = Py.Import(Path.GetFileNameWithoutExtension(fileName));
                    var nativeResult = fromFile.InvokeMethod(methodName, args);
                    result = nativeResult.As<T>();
                    log = _logger.ReadStream();
                    _logger.flush();
                }
            }
            catch (Exception ex)
            {
                log = $"Python Error: {ex} ({nameof(ExecuteMethod)})";
                if (throwOnErrors)
                {
                    throw;
                }
            }

            return result;
        }

        public void Initialize(IContainer appContainer)
        {
            SetVariable("DiContainer", new DiContainer(appContainer));
        }

        public IList<string> SearchPaths()
        {
            var pythonPaths = new List<string>();
            using (Py.GIL())
            {
                dynamic sys = Py.Import("sys");
                foreach (PyObject path in sys.path)
                {
                    pythonPaths.Add(path.ToString());
                }
            }

            return pythonPaths.Select(Path.GetFullPath).ToList();
        }
        
        public void SetVariable(string name, object value)
        {
            using (Py.GIL())
            {
                module.Value.Set(name, value.ToPython());
            }
        }

        public void SetupLogger(bool throwOnErrors)
        {
            SetVariable("Logger", _logger);
            const string loggerSrc = "import sys\n" +
                                     "from io import StringIO\n" +
                                     "sys.stdout = Logger\n" +
                                     "sys.stdout.flush()\n" +
                                     "sys.stderr = Logger\n" +
                                     "sys.stderr.flush()\n";
            ExecuteCommandOrScript<PyObject>(loggerSrc, throwOnErrors, out _);
        }
    }
}
