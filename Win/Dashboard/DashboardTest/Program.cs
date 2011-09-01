using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dashboard.Server.Tcp;
using System.Threading;
using Dashboard.Server;


namespace DashboardTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var dashboardServiceProvider = new Dashboard.Server.DashboardServiceProvider { Logger = new ConsoleLogger() };
            
            TcpServer server = new TcpServer(dashboardServiceProvider, 9999);
            server.Start();
            Thread.Sleep(-1);
        }
    }

    public class ConsoleLogger : Logger
    {

        #region Logger Members

        public void Log(string message)
        {
            Console.WriteLine(message);
        }

        public void Log(string format, params object[] args)
        {
            Console.WriteLine(string.Format(format, args));
        }

        #endregion
    }

}
