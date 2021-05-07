using System;
using Demo.NetCore.Consumers;
using Demo.NetCore.Impls.Test;
using Microsoft.Extensions.DependencyInjection;
using Sean.Core.Ioc;
using Sean.Utility.Common;
using Sean.Utility.Contracts;
using Sean.Utility.Extensions;
using Sean.Utility.Impls.Log;

namespace Demo.NetCore
{
    class Program
    {
        private static ILogger _logger;

        static void Main(string[] args)
        {
            ServiceManager.ConfigureServices(services =>
            {
                services.AddSimpleLocalLogger();
                services.AddSingleton<TestConsumer>();// 单例
                services.AddSingleton<SimpleActiveMQTest>();
                services.AddSingleton<ActiveMQTest>();
            });

            SimpleLocalLoggerBase.DateTimeFormat = time => time.ToLongDateTime();

            _logger = ServiceManager.GetService<ISimpleLogger<Program>>();

            ExceptionHelper.CatchGlobalUnhandledException(_logger);

            ISimpleDo toDo = ServiceManager.GetService<ActiveMQTest>();//new ActiveMQTest();
            //ISimpleDo toDo = ServiceManager.GetService<SimpleActiveMQTest>();//new SimpleActiveMQTest();
            toDo.Execute();
            while (Console.ReadLine() == "1")
            {
                toDo.Execute();
            }
        }
    }
}
