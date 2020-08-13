using System.Collections.Generic;

namespace Mgi.Instrument.ALM
{
    public interface IScriptEngine
    {
        void Initialize();

        void Share(IDictionary<string, object> shared);

        /// <summary>
        /// 加载脚本assembly。该上下文将绑定到Scope
        /// </summary>
        /// <param name="path"></param>
        /// <exception cref="Microsoft.Scripting.SyntaxErrorException">语法错误</exception>
        void LoadAssembly(string path);

        /// <summary>
        /// 获取已经共享的对象值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        bool TryGetVariable<T>(string key, out T t);

        /// <summary>
        /// 在当前的script scope中执行指定路径的脚本.
        /// </summary>
        /// <param name="path"></param>
        /// <exception cref="Microsoft.Scripting.SyntaxErrorException">语法错误</exception>
        void ExecuteByFile(string path);

        /// <summary>
        /// 在当前的script scope中执行指定路径的脚本. 使用invokeId进行标识
        /// </summary>
        /// <param name="path"></param>
        /// <param name="invokeId"></param>
        void ExecuteByFile(string path, string invokeId);

        /// <summary>
        /// 在当前空间/上下文中执行脚本字串
        /// </summary>
        /// <param name="script"></param>
        /// <exception cref="Microsoft.Scripting.SyntaxErrorException">语法错误</exception>
        void Execute(string script);

        /// <summary>
        /// 清理
        /// </summary>
        void Cleanup();
    }
}
