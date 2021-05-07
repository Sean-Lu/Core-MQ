using System;
using System.Diagnostics;

namespace Sean.Core.MQ.ActiveMQ
{
    internal class DebugHelper
    {
        /// <summary>
        /// 输出调试信息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="ex"></param>
        public static void Output(string msg, Exception ex = null)
        {
            // Debug\Trace\Console
            Debug.WriteLine($"###################[{typeof(DebugHelper).Namespace}]{msg}{(ex != null ? $" => {ex.Message}" : string.Empty)}");
        }
    }
}
