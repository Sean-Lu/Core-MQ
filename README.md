## 简介

> 消息队列：ActiveMQ

| Class                                                        | 说明                       |
| ------------------------------------------------------------ | -------------------------- |
| `Consumer<T>`<br>`Producer<T>`                               | 核心类                     |
| `SimpleConsumer`<br>`SimpleProducer`                         | 只支持基础功能              |

- `Consumer<T>`:

```
// 消费消息异常时，支持重新消费
args.ShouldRecovery = true;
```

- `Producer<T>`:

```
// 发送消息异常时，支持本地持久化存储消息并定时重发
options.PersistMsgWhenException = true;
```

## Packages

| Package | NuGet Stable | NuGet Pre-release | Downloads |
| ------- | ------------ | ----------------- | --------- |
| [Sean.Core.MQ.ActiveMQ](https://www.nuget.org/packages/Sean.Core.MQ.ActiveMQ/) | [![Sean.Core.MQ.ActiveMQ](https://img.shields.io/nuget/v/Sean.Core.MQ.ActiveMQ.svg)](https://www.nuget.org/packages/Sean.Core.MQ.ActiveMQ/) | [![Sean.Core.MQ.ActiveMQ](https://img.shields.io/nuget/vpre/Sean.Core.MQ.ActiveMQ.svg)](https://www.nuget.org/packages/Sean.Core.MQ.ActiveMQ/) | [![Sean.Core.MQ.ActiveMQ](https://img.shields.io/nuget/dt/Sean.Core.MQ.ActiveMQ.svg)](https://www.nuget.org/packages/Sean.Core.MQ.ActiveMQ/) |

## Nuget包引用

> **Id：Sean.Core.MQ.ActiveMQ**

- Package Manager

```
PM> Install-Package Sean.Core.MQ.ActiveMQ
```

## 配置示例

> 配置文件：

- .NET Core：`appsettings.json`

```
{
  "ActiveMQ": {
    "BrokerUri": "failover:tcp://127.0.0.1:61616/"
  }
}
```

- .NET Framework：`app.config`、`web.config`

```
  <appSettings>
    <add key="ActiveMQBrokerUri" value="failover:tcp://127.0.0.1:61616/" />
  </appSettings>
```

## 使用示例

> 项目：test\Demo.NetCore
