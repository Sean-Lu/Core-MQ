using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Newtonsoft.Json;

namespace Sean.Core.MQ.ActiveMQ
{
    internal class ConnectionPool : IDisposable
    {
        public static ConnectionPool Instance { get; } = new ConnectionPool();

        private readonly ConcurrentDictionary<ConnectionOptions, Connection> _dict = new ConcurrentDictionary<ConnectionOptions, Connection>();
        private static object locker = new object();

        private ConnectionPool() { }

        /// <summary>
        /// 获取连接
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public Connection GetConnection(ConnectionOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            if (_dict.TryGetValue(options, out var connection) && connection != null)
            {
                if (!connection.TransportFailed || connection.ITransport != null && connection.ITransport.IsFaultTolerant)
                    return connection;

                if (connection.ITransport != null
                    && !connection.ITransport.IsDisposed
                    && (!connection.ITransport.IsStarted || connection.ITransport.IsConnected))
                    return connection;
            }

            lock (locker)
            {
                connection?.Dispose();
                connection = CreateConnection(options) as Connection;
                _dict[options] = connection;
                return connection;
            }
        }

        /// <summary>
        /// 释放连接
        /// </summary>
        /// <param name="options"></param>
        public void DisposeConnection(ConnectionOptions options)
        {
            if (_dict.TryGetValue(options, out var connection))
            {
                lock (locker)
                {
                    connection?.Dispose();// 关闭连接
                }
                _dict.TryRemove(options, out _);
            }
        }
        /// <summary>
        /// 释放所有连接
        /// </summary>
        /// <param name="options"></param>
        public void DisposeAllConnection()
        {
            if (!_dict.Any())
            {
                return;
            }

            lock (locker)
            {
                foreach (var connection in _dict.Values)
                {
                    connection?.Dispose();
                }
            }

            _dict.Clear();
        }

        private IConnection CreateConnection(ConnectionOptions options)
        {
            var factory = new ConnectionFactory(options.BrokerUri)
            {
                NonBlockingRedelivery = options.NonBlockingRedelivery
            };
            if (!string.IsNullOrWhiteSpace(options.ClientId))
            {
                factory.ClientId = options.ClientId;
            }
            if (options.PrefetchPolicy != null)
            {
                factory.PrefetchPolicy = options.PrefetchPolicy;
            }
            if (options.RedeliveryPolicy != null)
            {
                factory.RedeliveryPolicy = options.RedeliveryPolicy;
            }

            var connection = string.IsNullOrWhiteSpace(options.UserName) ? factory.CreateConnection() : factory.CreateConnection(options.UserName, options.Password);
            connection.ExceptionListener += Connection_ExceptionListener;
            connection.ConnectionInterruptedListener += Connection_ConnectionInterruptedListener;
            connection.ConnectionResumedListener += Connection_ConnectionResumedListener;
            return connection;
        }

        /// <summary>
        /// 异常触发
        /// </summary>
        /// <param name="exception"></param>
        private void Connection_ExceptionListener(Exception exception)
        {
            //DebugHelper.Output("Connection_ExceptionListener");
        }

        /// <summary>
        /// 连接中断触发
        /// </summary>
        private void Connection_ConnectionInterruptedListener()
        {
            //DebugHelper.Output("Connection_ConnectionInterruptedListener");
        }

        /// <summary>
        /// 连接恢复触发
        /// </summary>
        private void Connection_ConnectionResumedListener()
        {
            //DebugHelper.Output("Connection_ConnectionResumedListener");
        }

        public void Dispose()
        {
            DisposeAllConnection();
        }
    }
}
