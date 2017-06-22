using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using WDTech_Frimware_Tcp_Loader.Data;

namespace WDTech_Frimware_Tcp_Loader.Helper
{
    public class SocketServer
    {
        private Socket _serverSocket;

        private bool _isServerDisposed;

        public List<SocketClient> ConnectedClients { get; }= new List<SocketClient>();

        public event SocketAcceptHandler SocketAcceptd;

        public event Disconnected ClientDisconnected;

        public bool StartServer(IPEndPoint serverEndPoint)
        {
            _serverSocket?.Dispose();
            _serverSocket = new Socket(serverEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                _serverSocket.Bind(serverEndPoint);
                _serverSocket.Listen(4096);
                StartAccept(null);
                _isServerDisposed = false;
            }
            catch (Exception ex)
            {
                SimpleLog.Error("Start Tcp Server Failed", ex);
                return false;
            }

            return true;
        }

        public bool Stop()
        {
            try
            {
                _serverSocket.Dispose();
                _isServerDisposed = true;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private void StartAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            if (_isServerDisposed) return;
            if (acceptEventArgs == null)
            {
                acceptEventArgs = new SocketAsyncEventArgs();
                acceptEventArgs.Completed += AcceptEventCompleted;
            }
            else
            {
                acceptEventArgs.AcceptSocket = null; //释放上次绑定的Socket，等待下一个Socket连接
            }

            var willRaiseEvent = _serverSocket.AcceptAsync(acceptEventArgs);//同步才是false，大多数的情况下都是异步的
            if (!willRaiseEvent)
            {
                ProcessAccept(acceptEventArgs);
            }
        }

        private void AcceptEventCompleted(object sender, SocketAsyncEventArgs acceptEventArgs)
        {
            ProcessAccept(acceptEventArgs);
        }

        private void ProcessAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            try
            {
                var client = new SocketClient(acceptEventArgs.AcceptSocket);
                client.Disconnected += OnClientDisconnected;
                ConnectedClients.Add(client);
                SocketAcceptd?.Invoke(new SocketAcceptEventArgs
                {
                    AcceptSocket = acceptEventArgs.AcceptSocket
                });
            }
            catch (Exception ex)
            {
                SimpleLog.Error($"Accept Client Failed, IpEndPoint:{acceptEventArgs.AcceptSocket.RemoteEndPoint}", ex);
            }

            StartAccept(acceptEventArgs); //把当前异步事件释放，等待下次连接
        }

        private void OnClientDisconnected(SocketClientDisconnectedArgs args)
        {
            ClientDisconnected?.Invoke(args);
        }
    }
}
