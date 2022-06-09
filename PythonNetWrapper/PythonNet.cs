using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Autofac;
using Python.Runtime;
using PythonnetWrapper.Interfaces;

namespace PythonnetWrapper
{
    public class PythonNet : IPythonEngine
    {
        private Lazy<PyScope> m_scope;
        private PythonLogger m_logger = new PythonLogger();

        public PythonNet()
        {
            m_scope = new Lazy<PyScope>(Py.CreateScope);
        }

        public void Dispose()
        {
            m_scope.Value.Dispose();
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
        public PyObject ExecuteCommand(string command, out string log)
        {
            PyObject result = null;
            log = "";
            try
            {
                using (Py.GIL())
                {
                    var pyCompile = Python.Runtime.PythonEngine.Compile(command);
                    result = m_scope.Value.Execute(pyCompile);
                    log = m_logger.ReadStream();
                    m_logger.flush();
                }
            }
            catch (Exception ex)
            {
                log = $"Python Error: {ex} ({nameof(ExecuteCommand)})";
            }

            return result;
        }

        public PyObject ImportScript(string fileName, out string log)
        {
            PyObject result = null;
            log = "";

            try
            {
                using (Py.GIL())
                {
                    dynamic sys = Py.Import("sys");
                    dynamic os = Py.Import("os");
                    bool pathExist = false;
                    foreach (var p in sys.path)
                    {
                        if (p.ToString().Contains(Path.GetDirectoryName(fileName)))
                        {
                            pathExist = true;
                            break;
                        }
                    }

                    if (!pathExist)
                    {
                        sys.path.append(os.path.dirname(os.path.expanduser(fileName)));
                    }

                    result = Py.Import(Path.GetFileNameWithoutExtension(fileName));
                    log = m_logger.ReadStream();
                    m_logger.flush();
                }
            }
            catch (Exception ex)
            {
                log = $"Python Error: {ex} ({nameof(ImportScript)})";
            }

            return result;

        }

        public PyObject ExecuteMethodOnScriptObject(PyObject script, string methodName, out string log, params PyObject[] args)
        {
            PyObject result = null;

            try
            {
                result = script.InvokeMethod(methodName, args);
                log = m_logger.ReadStream();
                m_logger.flush();
                return result;
            }
            catch (Exception ex)
            {
                log = $"Python Error: {ex} ({nameof(ExecuteMethodOnScriptObject)})";
            }

            return result;
        }
        public PyObject ExecuteMethod(string fileName, string methodName, out string log, params PyObject[] args)
        {
            PyObject result = null;
            log = "";

            try
            {
                using (Py.GIL())
                {
                    dynamic sys = Py.Import("sys");
                    dynamic os = Py.Import("os");
                    bool pathExist = false;
                    foreach (var p in sys.path)
                    {
                        if (p.ToString().Contains(Path.GetDirectoryName(fileName)))
                        {
                            pathExist = true;
                            break;
                        }
                    }

                    if (!pathExist)
                    {
                        sys.path.append(os.path.dirname(os.path.expanduser(fileName)));
                    }

                    PyObject fromFile = Py.Import(Path.GetFileNameWithoutExtension(fileName));
                    result = fromFile.InvokeMethod(methodName, args);
                    log = m_logger.ReadStream();
                    m_logger.flush();
                }
            }
            catch (Exception ex)
            {
                log = $"Python Error: {ex} ({nameof(ExecuteMethod)})";
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

        public void SetSearchPath(IList<string> paths)
        {
            var searchPaths = paths.Where(Directory.Exists).Distinct().ToList();

            using (Py.GIL())
            {
                var src = "import sys\n" +
                           $"sys.path.extend({searchPaths.ToPython()})";
                ExecuteCommand(src, out _);
            }
        }

        public void SetVariable(string name, object value)
        {
            using (Py.GIL())
            {
                m_scope.Value.Set(name, value.ToPython());
            }
        }

        public void SetupLogger()
        {
            SetVariable("Logger", m_logger);
            const string loggerSrc = "import sys\n" +
                                     "from io import StringIO\n" +
                                     "sys.stdout = Logger\n" +
                                     "sys.stdout.flush()\n" +
                                     "sys.stderr = Logger\n" +
                                     "sys.stderr.flush()\n";
            ExecuteCommand(loggerSrc, out _);
        }
    }
}
