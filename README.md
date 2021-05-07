## 简介

> 消息队列：ActiveMQ

| Class                                                        | 说明                       |
| ------------------------------------------------------------ | -------------------------- |
| `Consumer<T>`: 消费消息异常时，支持重新消费（args.ShouldRecovery = true;）<br>`Producer<T>`: 发送消息异常时，支持本地持久化存储消息并定时重发（options.PersistMsgWhenException = true;） | ==核心类==                 |
| `SimpleConsumer`<br>`SimpleProducer`                         | 简单封装（只支持基础功能） |

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