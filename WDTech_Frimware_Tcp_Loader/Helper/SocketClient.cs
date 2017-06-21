using System;
using System.Net.Sockets;

namespace WDTech_Frimware_Tcp_Loader.Helper
{
    public delegate void TcpDataReceived(SocketClientReceiveEventArgs args);

    public class SocketClientEventArgs
    {
        public string Message { get; set; }

        public Exception Exception { get; set; }
    }

    public class SocketClientReceiveEventArgs : SocketClientEventArgs
    {
        public int BytesTransferred { get; set; }

        public byte[] Buffer { get; set; }
    }


    public class SocketClient
    {
        private readonly Socket _clientSocket;

        private readonly SocketAsyncEventArgs _asyncEventArgs;

        public event TcpDataReceived TcpDataReceived;

        public SocketClient(Socket client)
        {
            _clientSocket = client;
            _asyncEventArgs = new SocketAsyncEventArgs();
            _asyncEventArgs.SetBuffer(new byte[4096], 0, 4096);
            _asyncEventArgs.Completed += (sender, args) =>
            {
                ProcessReceive();
            };
            var willRaiseEvent = _clientSocket.ReceiveAsync(_asyncEventArgs); //投递接收请求
            if (!willRaiseEvent)
            {
                lock (_clientSocket)
                {
                    ProcessReceive();
                }
            }
        }

        public void Send(byte[] sendBytes)
        {
            _clientSocket.Send(sendBytes);
        }

        private void ProcessReceive()
        {
            if (_asyncEventArgs.BytesTransferred >= 0 && _asyncEventArgs.SocketError == SocketError.Success)
            {
                DataReceived(new SocketClientReceiveEventArgs
                {
                    BytesTransferred = _asyncEventArgs.BytesTransferred,
                    Buffer = _asyncEventArgs.Buffer
                });
            }
        }

        private void DataReceived(SocketClientReceiveEventArgs args)
        {
            TcpDataReceived?.Invoke(args);
        }

        public void Dispoose()
        {
            _clientSocket.Dispose();
        }
    }
}
