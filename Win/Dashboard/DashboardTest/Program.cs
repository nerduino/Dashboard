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

        public string ContentType { get { return "text/csv"; } }

        public IEnumerable<string> GetData()
        {
            PrivateDataSource ds = new PrivateDataSource();
            return ds.GetData();
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
