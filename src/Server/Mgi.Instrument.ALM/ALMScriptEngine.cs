using System;
using System.Collections.Generic;
using IronPython.Hosting;
using log4net;
using Microsoft.Scripting.Hosting;

namespace Mgi.Instrument.ALM
{
    public class ALMScriptEngine : IScriptEngine
    {
        protected readonly ScriptRuntime _runtime;
        protected readonly ScriptEngine _engine;
        protected readonly ScriptScope _scope;
        protected readonly ILog _logger;
        //private readonly Stream _out;
        //private readonly Stream _err;

        public string ScriptFile { get; private set; }


        public ALMScriptEngine(/*ILogProvider logProvider*/)
        {
            //_logger = logProvider.GetScriptLogger();
            _runtime = Python.CreateRuntime();

            _engine = _runtime.GetEngine("Python");
            _engine.SetSearchPaths(new[]
            {
                AppContext.BaseDirectory,
                AppContext.BaseDirectory + "Lib",
                AppContext.BaseDirectory + "lib"
            });
            _scope = _engine.GetSysModule();
        }

        //private Stream ListingOut(string label)
        //{
        //    var dir = Path.GetFullPath("./log/scriptIO");
        //    if (!Directory.Exists(dir))
        //        Directory.CreateDirectory(dir);
        //    var fname = $"Script-{label}-{DateTime.Now.ToString("yyyy.MM.dd HH.mm.ss.ffff")}.log";
        //    var fio = new BufferedStream(new FileStream($"{dir}/{fname}", FileMode.CreateNew, FileAccess.Write));
        //    return fio;
        //}


        public virtual void Initialize()
        {
            // nothing should initalized now, for overrider 
        }

        public void Share(IDictionary<string, object> shared)
        {
            foreach (var kv in shared)
                _scope.SetVariable(kv.Key, kv.Value);
        }

        /// <summary>
        /// 加载脚本assembly。该上下文将绑定到Scope
        /// </summary>
        /// <param name="path"></param>
        /// <exception cref="Microsoft.Scripting.SyntaxErrorException">语法错误</exception>
        public void LoadAssembly(string path)
        {
            _engine.CreateScriptSourceFromFile(path)
                       .Compile()
                       .Execute(_scope);
        }

        /// <summary>
        /// 获取已经共享的对象值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool TryGetVariable<T>(string key, out T t)
            => _scope.TryGetVariable<T>(key, out t);

        /// <summary>
        /// 在当前的script scope中执行指定路径的脚本.
        /// </summary>
        /// <param name="path"></param>
        /// <exception cref="Microsoft.Scripting.SyntaxErrorException">语法错误</exception>
        public virtual void ExecuteByFile(string path)
        {
            ScriptFile = path;
            try
            {
                _engine.CreateScriptSourceFromFile(path)
                        .Compile()
                        .Execute(_scope);
            }
            catch (Exception ex)
            {
                LogPythonStack(ex);
                throw;
            }
        }

        /// <summary>
        /// 在当前空间/上下文中执行脚本字串
        /// </summary>
        /// <param name="script"></param>
        /// <exception cref="Microsoft.Scripting.SyntaxErrorException">语法错误</exception>
        public virtual void Execute(string script)
        {
            ScriptFile = "Mem.string";
            try
            {
                _engine.CreateScriptSourceFromString(script)
                        .Compile()
                        .Execute(_scope);
            }
            catch (Exception ex)
            {
                LogPythonStack(ex);
                throw;
            }
        }

        private void LogPythonStack(Exception ex)
        {
            _logger.ErrorFormat("Script fail, path: {0}", ScriptFile);
            var exOp = _engine.GetService<ExceptionOperations>();
            exOp.GetExceptionMessage(ex, out string msg, out string errType);
            _logger.ErrorFormat("type:{0},{1}", errType, msg);
            foreach (var x in exOp.GetStackFrames(ex))
                _logger.ErrorFormat("line:{0}, method:{1}, file:{2}", x.GetFileName(), x.GetMethodName(), x.GetFileLineNumber());
        }

        public virtual void Cleanup()
        {
            try
            {
                _runtime.Shutdown();
                //_out.Flush();
                //_out.Close();

                //_err.Flush();
                //_err.Close();                
            }
            catch (Exception ex)
            {
                _logger.WarnFormat("Scriptor shutdown encountered error: {0}", ex.Message);
            }
        }

        public virtual void ExecuteByFile(string path, string invokeId)
        {
            _logger.InfoFormat("Not used invokeId {0}", invokeId);
            ExecuteByFile(path);
        }
    }
}
