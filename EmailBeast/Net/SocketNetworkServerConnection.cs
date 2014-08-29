using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace EmailBeast.Net
{
    public class SocketNetworkServerConnection : INetworkServerConnection
    {
        private readonly ManualResetEvent _connectionAcceptedEvent = new ManualResetEvent(false);
        private readonly ManualResetEvent _stopThreadEvent = new ManualResetEvent(false);

        private Socket _listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private Thread _listenerThread = new Thread(ListeningThreadProc);


        public event NetworkConnectionAccepted ConnectionAccepted;

        public bool IsListening { get; private set; }

        public INetworkServerConnection BindTo(EndPoint endPoint)
        {
            _listenerSocket.Bind(endPoint);

            return this;
        }

        public INetworkServerConnection StartListeningForConnections(int maximumPendingConnectionsQueueLength = 5)
        {
            if (ThreadState.Unstarted != _listenerThread.ThreadState)
                throw new AlreadyListeningForConnectionsException();

            _stopThreadEvent.Reset();
            IsListening = true;

            _listenerSocket.Listen(maximumPendingConnectionsQueueLength);

            _listenerThread.Start(this);


            return this;
        }


        public void StopListeningForConnections()
        {
            IsListening = false;
            _stopThreadEvent.Set();

            Console.WriteLine("[SocketNetworkServerConnection::StopListeningForConnections]  Shutting Down _listenerSocket");
            _listenerSocket.Close();

            Console.WriteLine("[SocketNetworkServerConnection::StopListeningForConnections]  Joining thread.");
            _listenerThread.Join(250);

            Console.WriteLine("[SocketNetworkServerConnection::StopListeningForConnections]  Stopped");
        }


        public void Dispose()
        {
            Console.WriteLine("[SocketNetworkServerConnection::Dispose]");
            lock (this)
            {
                if (null != _listenerSocket)
                {
                    try
                    {
                        if (IsListening)
                            StopListeningForConnections();

                        _listenerSocket.Close();
                        _listenerSocket.Dispose();

                        Console.WriteLine("[SocketNetworkServerConnection::Dispose]  ListenerSocket Disposed");
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine("[SocketNetworkServerConnection::Dispose]  Caught Exception in SocketNetworkServerConnection::Dispose (_listenerSocket):\n{0}", exception);
                    }
                    finally
                    {
                        _listenerSocket = null;
                    }

                    Console.WriteLine("[SocketNetworkServerConnection::Dispose]  ListenerSocket Disposed");
                }


                if (null != _listenerThread)
                {
                    try
                    {
                        if (_listenerThread.ThreadState != ThreadState.Stopped)
                        {
                            _listenerThread.Abort();
                        }
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine("[SocketNetworkServerConnection::Dispose]  Caught Exception in SocketNetworkServerConnection::Dispose (_listenerThread):\n{0}", exception);
                    }
                    finally
                    {
                        _listenerThread = null;
                    }

                    Console.WriteLine("[SocketNetworkServerConnection::Dispose]  Thread Disposed");
                }
            }
        }


        private static void ListeningThreadProc(object state)
        {
            SocketNetworkServerConnection _this = (SocketNetworkServerConnection) state;
            _this._listenerThread.Name = "Socket Network Server Listener";

            while (_this.IsListening)
            {
                // Set the event to nonsignaled state.
                _this._connectionAcceptedEvent.Reset();

                // Start an asynchronous socket to listen for connections.
                Console.WriteLine("Waiting for a connection...");
                _this._listenerSocket.BeginAccept(AcceptCallback, _this);

                // Wait until a connection is made before continuing.

                WaitHandle.WaitAny(new WaitHandle[] {_this._connectionAcceptedEvent, _this._stopThreadEvent});
            }


            Console.WriteLine("Listener Thread closed.");
        }


        private static void AcceptCallback(IAsyncResult ar)
        {
            SocketNetworkServerConnection _this = (SocketNetworkServerConnection) ar.AsyncState;

            if (!_this.IsListening)
                return;

            // Signal the main thread to continue.
            _this._connectionAcceptedEvent.Set();


            // Get the socket that handles the client request.
            Socket handler = _this._listenerSocket.EndAccept(ar);

            SocketNetworkClientConnection clientConnection = new SocketNetworkClientConnection(handler);

            if (null != _this.ConnectionAccepted)
                _this.ConnectionAccepted(_this, clientConnection);
        }
    }

    public class AlreadyListeningForConnectionsException : Exception
    {
        public AlreadyListeningForConnectionsException()
            : base("The NetworkServerConnection is already listening for connections. Do not call StartListeningForConnections() more than once.")
        { }
    }
}
