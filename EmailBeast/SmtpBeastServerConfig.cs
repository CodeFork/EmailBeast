using System.Net;

namespace EmailBeast
{
    public class ListenSocketConfig
    {
        private int _backlog = 10;
        private int _maximumConnections = 10;

        public int Backlog
        {
            get { return _backlog; }
            set { _backlog = value; }
        }

        public int MaximumConnections
        {
            get { return _maximumConnections; }
            set { _maximumConnections = value; }
        }
    }

    public class SmtpBeastServerConfig
    {
        public IPEndPoint EndPoint { get; set; }
    }
}
