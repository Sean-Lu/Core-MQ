using System;
using Demo.NetCore.Consumers;
using Demo.NetCore.Consumers.ActiveMQ;
using Demo.NetCore.Impls.ActiveMQ;
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
            IocContainer.Instance.ConfigureServices((services, configuration) =>
            {
                services.AddSimpleLocalLogger();
                services.AddSingleton<TestConsumer>();
                services.AddSingleton<SimpleActiveMQTest>();
                services.AddSingleton<ActiveMQTest>();
            });

            SimpleLocalLoggerBase.DateTimeFormat = time => time.ToLongDateTime();

            _logger = IocContainer.Instance.GetService<ISimpleLogger<Program>>();

            ExceptionHelper.CatchGlobalUnhandledException(_logger);

            ISimpleDo toDo = IocContainer.Instance.GetService<ActiveMQTest>();//new ActiveMQTest();
            //ISimpleDo toDo = IocContainer.Instance.GetService<SimpleActiveMQTest>();//new SimpleActiveMQTest();
            toDo.Execute();
            while (Console.ReadLine() == "1")
            {
                toDo.Execute();
            }
        }
    }
}
