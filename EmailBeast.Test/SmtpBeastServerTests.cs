using System;
using System.Net;
using ActiveUp.Net.Mail;
using Xunit;

namespace EmailBeast.Test
{
    public class SmtpBeastServerTests
    {
        private readonly IPEndPoint _smtpEndPoint = new IPEndPoint(IPAddress.Any, 25);


        [Fact]
        public void ConstructorThrowsArgumentNullExceptionIfServerConfigIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
// ReSharper disable once ObjectCreationAsStatement
                new SmtpBeastServer(null);
            });
        }


        [Fact]
        public void CanStartInstance()
        {
            // Arrange
            SmtpBeastServerConfig serverConfig = new SmtpBeastServerConfig
            {
                EndPoint = _smtpEndPoint
            };


            using (SmtpBeastServer smtpBeastServer = new SmtpBeastServer(serverConfig))
            {
                // Act
                smtpBeastServer.Start();

                // Assert
                Assert.True(smtpBeastServer.IsStarted);
            }
        }


        [Fact]
        public void CanCreateSmtpConnectionToInstance()
        {
            // Arrange
            SmtpBeastServerConfig serverConfig = new SmtpBeastServerConfig
            {
                EndPoint = _smtpEndPoint
            };


            using (SmtpBeastServer smtpBeastServer = new SmtpBeastServer(serverConfig))
            {
                smtpBeastServer.Start();

                // Act
                SmtpClient smtpClient = new SmtpClient();

                string result = smtpClient.Connect(new IPAddress(new byte[] { 127, 0, 0, 1 }), _smtpEndPoint.Port);

                Console.WriteLine("result: [{0}]", result);

                // Assert
            }
        }
    }
}
