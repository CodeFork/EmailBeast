using System.Collections.Generic;

namespace EmailBeast.Net
{
    public interface INetworkClientConnection
    {
        void Send(IEnumerable<byte> bytesToSend);

        void Close();
    }
}
