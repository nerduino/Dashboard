using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Dashboard.Server;


namespace DashboardTest
{

    //May require perms...
    //netsh http add urlacl url=http://+:9999/ user=DOMAIN\user
    public class Program
    {
        public static void Main(string[] args)
        {
            var dashboardserver = new DashboardServer(999, new TestDataSource());
            dashboardserver.Log = new ConsoleLogger();
            dashboardserver.Start();
            Thread.Sleep(-1);
        }
    }

    public class TestDataSource : DashboardDataSource
    {
        private Random r = new Random();
        private static string[] SERVERS = new string[] { "000", "211", "212", "221", "222", "231", "232", "241", "242", "251", "252" };

        public string ContentType { get { return "text/csv"; } }

        public IEnumerable<string> GetData()
        {
            return from s in SERVERS select string.Format("{0},{1}", s, r.Next(100));
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
