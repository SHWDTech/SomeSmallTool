using System;
using System.Net.Sockets;

namespace WDTech_Frimware_Tcp_Loader.Helper
{
    public delegate void SocketAcceptHandler(SocketAcceptEventArgs args);

    public class SocketEventArgs
    {
        public string Message { get; set; }

        public Exception Exception { get; set; }

        public Socket AcceptSocket { get; set; }
    }

    public class SocketAcceptEventArgs : SocketEventArgs
    {
    }

    public class SocketDisconnectEventArgs : SocketEventArgs
    {
        
    }
}
