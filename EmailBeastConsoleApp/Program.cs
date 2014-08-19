using System;
using System.Net;
using EmailBeast;

namespace EmailBeastConsoleApp
{
    class Program
    {
        private static readonly IPEndPoint SmptEndPoint = new IPEndPoint(IPAddress.Any, 25);


        static void Main(string[] args)
        {
            Console.WriteLine("Starting...");

            using (SmtpBeastServer smtpBeastServer = new SmtpBeastServer(new SmtpBeastServerConfig { EndPoint = SmptEndPoint }))
            {
                smtpBeastServer.Start();

                Console.WriteLine("Press <Enter> to stop listening...");
                Console.ReadLine();
            }


            Console.WriteLine("Press <Enter> to exit...");
            Console.ReadLine();
        }
    }
}
