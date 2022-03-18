using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.RabbitMQ
{
    public class RabbitMQPersistentConnection : IDisposable
    {
        private IConnection _connection;
        private readonly IConnectionFactory _connectionFactory;
        private readonly int _retryCount;
        private object lockObject = new object();
        private bool _disposed;
        public RabbitMQPersistentConnection(IConnectionFactory connectionFactory,int retryCount=5)
        {
            _connectionFactory = connectionFactory;
            _retryCount = retryCount;
        }
        public bool IsConnected => _connection != null && _connection.IsOpen;
        public IModel CreateModel()
        {
            return _connection.CreateModel();
        }
        public void Dispose()
        {
            _connection.Dispose();
            _disposed = true;
        }
        public bool TryConnect()
        {
            lock (lockObject)
            {
                var policy = Policy.Handle<SocketException>()
                    .Or<BrokerUnreachableException>()
                    .WaitAndRetry(_retryCount, retryAtempt => TimeSpan.FromSeconds(Math.Pow(2, retryAtempt)), (ex, time) =>
                    {
                    }
                );

                policy.Execute(() =>
                {
                    _connection = _connectionFactory.CreateConnection();
                });
                if (IsConnected)
                {
                    _connection.ConnectionShutdown += ConnectionShutDown;
                    _connection.CallbackException += ConnectionCallbackException;
                    _connection.ConnectionBlocked += ConnectionBlocked;
                    //log
                    return true;
                }
                return false;
            }

        }

        private void ConnectionBlocked(object sender, global::RabbitMQ.Client.Events.ConnectionBlockedEventArgs e)
        {
            if (_disposed) return;
            TryConnect();
        }

        private void ConnectionCallbackException(object sender, global::RabbitMQ.Client.Events.CallbackExceptionEventArgs e)
        {
            if (_disposed) return;
            TryConnect();
        }

        private void ConnectionShutDown(object sender, ShutdownEventArgs e)
        {
            //log connection
            if (_disposed) return;
            TryConnect();
        }
    }
}
