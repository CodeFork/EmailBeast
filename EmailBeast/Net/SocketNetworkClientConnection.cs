using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace EmailBeast.Net
{
    public class SocketNetworkClientConnection : INetworkClientConnection
    {
        protected Socket ClientSocket { get; private set; }

        private readonly byte[] _receiveBuffer = new byte[1024];

        public SocketNetworkClientConnection(Socket clientSocket)
        {
            if (null == clientSocket)
                throw new ArgumentNullException("clientSocket");

            ClientSocket = clientSocket;

            ClientSocket.BeginReceive(_receiveBuffer, 0, _receiveBuffer.Length, SocketFlags.None, ReceiveCallback, this);
        }

        public void Send(IEnumerable<byte> bytesToSend)
        {
            byte[] dataBuffer = bytesToSend.ToArray();

            // Begin sending the data to the remote device.
            ClientSocket.BeginSend(dataBuffer, 0, dataBuffer.Length, 0, SendCallback, this);
        }

        public void Close()
        {
            ClientSocket.Close();

            Console.WriteLine("[SocketNetworkClientConnection::Close]  Socket Closed");
        }

        private static void SendCallback(IAsyncResult ar)
        {
            if (!ar.IsCompleted)
                return;

            SocketNetworkClientConnection _this = (SocketNetworkClientConnection) ar.AsyncState;

            Console.WriteLine("Send Completed");
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            SocketNetworkClientConnection _this = (SocketNetworkClientConnection)ar.AsyncState;
 
            Console.WriteLine("Received Data");
        }
    }
}
