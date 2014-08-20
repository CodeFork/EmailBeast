using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace EmailBeast
{
    public class SmtpBeastServer : IDisposable
    {
        private static readonly ManualResetEvent ConnectionAcceptedManualResetEvent = new ManualResetEvent(false);



        public SmtpBeastServerConfig ServerConfig { get; private set; }

        public bool IsStarted { get; private set; }


        public SmtpBeastServer(SmtpBeastServerConfig serverConfig)
        {
            if (null == serverConfig)
                throw new ArgumentNullException("serverConfig");

            ServerConfig = serverConfig;
            IsStarted = false;
        }


        public void Start()
        {
            // Data buffer for incoming data.
            // byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket. The DNS name of the computer
            // running the listener is "host.contoso.com".
            //IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            //IPAddress ipAddress = ipHostInfo.AddressList[0];
            //IPEndPoint localEndPoint = ServerConfig.EndPoint;


            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                listener.Bind(ServerConfig.EndPoint);
                listener.Listen(100);

                IsStarted = true;

                while (true)
                {
                    // Set the event to nonsignaled state.
                    ConnectionAcceptedManualResetEvent.Reset();

                    // Start an asynchronous socket to listen for connections.
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(AcceptCallback, listener);

                    // Wait until a connection is made before continuing.
                    ConnectionAcceptedManualResetEvent.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }


            IsStarted = false;
        }

        public void Dispose()
        {
        }


        public static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            ConnectionAcceptedManualResetEvent.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket) ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.
            SmtpBeastStateObject state = new SmtpBeastStateObject
            {
                workSocket = handler
            };

            handler.Send(new SmtpServerMessageBuilder().Greeting());

            handler.BeginReceive(state.buffer, 0, SmtpBeastStateObject.BufferSize, 0, ReadCallback, state);
        }

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
    }
}
