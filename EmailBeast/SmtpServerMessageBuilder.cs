using System;
using System.Text;

namespace EmailBeast
{
    public class SmtpServerMessageBuilder
    {
        public byte[] Greeting()
        {
            string greeting = string.Format("220 {0} Service Ready\r\n", Environment.MachineName);

            return Encoding.UTF8.GetBytes(greeting);
        }
    }
}
