using System.Net.Sockets;
using System.Text;

namespace EmailBeast
{
    /// <summary>
    /// State object for reading client data asynchronously
    /// </summary>
    public class SmtpBeastStateObject
    {
        // Size of receive buffer.
        public const int BufferSize = 1024;


        // Client  socket.
        public Socket workSocket = null;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }
}
