using System;
using EmailBeast.Net;

namespace EmailBeast
{
    public class SmtpBeastServer : IDisposable
    {
        private INetworkServerConnection _networkServerConnection;


        public SmtpBeastServerConfig ServerConfig { get; private set; }
        public bool IsStarted { get { return _networkServerConnection.IsListening; } }


        public SmtpBeastServer(SmtpBeastServerConfig serverConfig, INetworkServerConnection networkServerConnection = null)
        {
            if (null == serverConfig)
                throw new ArgumentNullException("serverConfig");

            ServerConfig = serverConfig;
            _networkServerConnection = networkServerConnection ?? new SocketNetworkServerConnection();
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
                if (null != _networkServerConnection && _networkServerConnection.IsListening)
                {
                    Console.WriteLine("[SmtpBeastServer::Stop]  Stopping Network Connection");
                    _networkServerConnection.StopListeningForConnections();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("[SmtpBeastServer::Stop]  Caught Exception:\n{0}", exception);
            }
        }

        private void networkServerConnection_ConnectionAccepted(INetworkServerConnection parent, INetworkClientConnection clientConnection)
        {
            Console.WriteLine("[SmtpBeastServer::networkServerConnection_ConnectionAccepted]  Connection Accepted - Sending Greeting");

            clientConnection.Send(new SmtpServerMessageBuilder().Greeting());

            clientConnection.Close();
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


        //public static void AcceptCallback(IAsyncResult ar)
        //{
        //    // Signal the main thread to continue.
        //    ConnectionAcceptedManualResetEvent.Set();

        //    // Get the socket that handles the client request.
        //    Socket listener = (Socket) ar.AsyncState;
        //    Socket handler = listener.EndAccept(ar);

        //    // Create the state object.
        //    SmtpBeastStateObject state = new SmtpBeastStateObject
        //    {
        //        workSocket = handler
        //    };

        //    handler.Send(new SmtpServerMessageBuilder().Greeting());

        //    handler.BeginReceive(state.buffer, 0, SmtpBeastStateObject.BufferSize, 0, ReadCallback, state);
        //}
        /*
        public static void ReadCallback(IAsyncResult ar)
        {
            Console.WriteLine("ReadCallback: IsCompleted? [{0}]  CompletedSynchronously? [{1}]", ar.IsCompleted, ar.CompletedSynchronously);

            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            SmtpBeastStateObject state = (SmtpBeastStateObject) ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket. 
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read // more data.
                content = state.sb.ToString();
                if (content.IndexOf("\r\n", StringComparison.Ordinal) > -1)
                {
                    // All the data has been read from the 
                    // client. Display it on the console.
                    Console.WriteLine("Read {0} bytes from socket.\n Data: {1}", content.Length, content.Trim());

                    // Echo the data back to the client.
                    Send(handler, content);
                }
                else
                {
                    // Not all data received. Get more.
                    handler.BeginReceive(state.buffer, 0, SmtpBeastStateObject.BufferSize, 0, ReadCallback, state);
                }
            }
        }
*/
        /*
        private static void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, handler);
        }


        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket) ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

                Console.WriteLine("Closed connection.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        */
    }
}
