using System;
using System.Net;

namespace EmailBeast.Net
{
    public delegate void NetworkConnectionAccepted(INetworkServerConnection parent, INetworkClientConnection clientConnection);



    public interface INetworkServerConnection : IDisposable
    {
        bool IsListening { get; }

        INetworkServerConnection BindTo(EndPoint endPoint);
        INetworkServerConnection StartListeningForConnections(int maximumPendingConnectionsQueueLength = 5);

        void StopListeningForConnections();

        event NetworkConnectionAccepted ConnectionAccepted;
    }
}
