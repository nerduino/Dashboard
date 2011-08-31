using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Configuration;
using Dashboard.Server.Tcp;

namespace Dashboard.Service
{
    public partial class DashboardService : ServiceBase
    {

        private EventLog log;
        private TcpServer server;

        public DashboardService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            ConfigurationManager.RefreshSection("AppSetings");

            string logSource = ConfigurationManager.AppSettings["LogSource"];
            string logName = ConfigurationManager.AppSettings["LogName"];
            
            if (!EventLog.SourceExists(logSource))
            {
                EventLog.CreateEventSource(logSource, logName);
            }

            log = new EventLog();
            log.Source = logSource;
           
            int port;
            if (!int.TryParse(ConfigurationManager.AppSettings["ListenPort"], out port))
            {
                port = 9999;
            }

            Dashboard.Server.DashboardServiceProvider dashboardServiceProvider = new Dashboard.Server.DashboardServiceProvider();
            dashboardServiceProvider.EventLog = log;

            server = new TcpServer(dashboardServiceProvider, port);
            server.Start();

            log.WriteEntry(string.Format("Listening on port {0}.",port));
        }

        protected override void OnStop()
        {
            log.WriteEntry("Stopping Server.");
            try
            {
                server.Stop();
            }
            catch
            {
            }
            log.WriteEntry("Server Stopped.");
        }
    }
}
