using System;
using System.Net.Sockets;
using EmailBeast.Net;

namespace EmailBeast
{
    public class SmtpBeastServer : IDisposable
    {
        private INetworkServerConnection _networkServerConnection;


        public SmtpBeastServerConfig ServerConfig { get; private set; }
        public bool IsStarted { get; private set; }


        public SmtpBeastServer(SmtpBeastServerConfig serverConfig, INetworkServerConnection networkServerConnection = null)
        {
            if (null == serverConfig)
                throw new ArgumentNullException("serverConfig");

            ServerConfig = serverConfig;
            _networkServerConnection = networkServerConnection ?? new SocketNetworkServerConnection();
            IsStarted = false;
        }


        public void Start()
        {
            _networkServerConnection.ConnectionAccepted += networkServerConnection_ConnectionAccepted;


            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                _networkServerConnection
                    .BindTo(ServerConfig.EndPoint)
                    .StartListeningForConnections(100);

                IsStarted = true;
            }
            catch (Exception exception)
            {
                Console.WriteLine("[SmtpBeastServer::Start]  Caught Exception:\n{0}", exception);
            }
        }


        public void Stop()
        {
            try
            {
                if (null == _networkServerConnection)
                    throw new ObjectDisposedException("_networkServerConnection");

                if (_networkServerConnection.IsListening)
                    _networkServerConnection.StopListeningForConnections();

                Console.WriteLine("[SmtpBeastServer::Stop]  Stopping Network Connection");
            }
            catch (Exception exception)
            {
                Console.WriteLine("[SmtpBeastServer::Stop]  Caught Exception:\n{0}", exception);
            }
        }


        public void Dispose()
        {
            Console.WriteLine("[SmtpBeastServer::Dispose]");
            lock (this)
            {
                if (null != _networkServerConnection)
                {
                    try
                    {
                        Console.WriteLine("[SmtpBeastServer::Dispose]  StopListeningForConnections");
                        if (_networkServerConnection.IsListening)
                        {
                            _networkServerConnection.StopListeningForConnections();
                        }
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine("Caught Exception in SmtpBeastServer::Dispose:\n{0}", exception);
                    }
                    finally
                    {
                        _networkServerConnection = null;
                    }
                }
            }

            Console.WriteLine("[SmtpBeastServer::Dispose]  Disposed");
        }


        private void networkServerConnection_ConnectionAccepted(INetworkServerConnection parent, INetworkClientConnection clientConnection)
        {
            Console.WriteLine("[SmtpBeastServer::networkServerConnection_ConnectionAccepted]  Connection Accepted - Sending Greeting");


            clientConnection.Send(new SmtpServerMessageBuilder().Greeting());

            clientConnection.Close();

            IsStarted = false;
        }
    }
}
