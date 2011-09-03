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
            var dashboardServiceProvider = new Dashboard.Server.DashboardServiceProvider { Logger = new ConsoleLogger(), DataSource = new TestDataSource()};
            
            TcpServer server = new TcpServer(dashboardServiceProvider, 9999);
            server.Start();
            Thread.Sleep(-1);
        }
    }

    public class TestDataSource : ProviderDataSource
    {
        private Random r = new Random();
        private static string[] SERVERS = new string[] { "000", "211", "212", "221", "222", "231", "232", "241", "242", "251", "252" };
        
        public IEnumerable<string> GetData()
        {
            return from s in SERVERS select string.Format("{0}:{1}", s, r.Next(100));
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
