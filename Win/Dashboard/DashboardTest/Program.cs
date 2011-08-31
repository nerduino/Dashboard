using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dashboard.Server.Tcp;
using System.Threading;


namespace DashboardTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            TcpServer server = new TcpServer(new Dashboard.Server.DashboardServiceProvider(), 9999);
            server.Start();
            Thread.Sleep(30000);
        }
    }


}
