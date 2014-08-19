using System;
using System.Net;
using ActiveUp.Net.Mail;
using Xunit;

namespace EmailBeast.Test
{
    public class ConsoleAppTests
    {
        [Fact]
        public void CanConnectToConsoleApp()
        {
            // Arrange
            SmtpClient smtpClient = new SmtpClient();

            // Act
            string result = smtpClient.Connect(IPAddress.Loopback, 25);
            Console.WriteLine("Connected: [{0}]", result);
        }
    }
}
