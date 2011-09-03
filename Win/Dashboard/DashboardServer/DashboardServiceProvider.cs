using System;
using System.Text;
using Dashboard.Server.Tcp;
using System.Collections.Generic;

namespace Dashboard.Server
{
    public interface Logger
    {
        void Log(string message);
        void Log(string format, params object[] args);
    }

    public interface ProviderDataSource
    {
        IEnumerable<string> GetData();
    }


    public class DashboardServiceProvider : TcpServiceProvider
    {

        protected static string OHAI = "OHAI";
        protected static string KTHXBAI = "KTHXBAI";
        
        public ProviderDataSource DataSource { get; set; }
        public Logger Logger { get; set; }


        public DashboardServiceProvider()
            : base()
        {
            DataSource = new PrivateDataSource();
        }

        protected void log(string format, params object[] args)
        {
            if (Logger != null)
            {
                Logger.Log(format, args);
            }
        }

        protected bool WriteLine(ConnectionState state, string msg)
        {
            byte[] msgBytes = Encoding.ASCII.GetBytes(String.Format("{0}{1}", msg, Environment.NewLine));
            return state.Write(msgBytes, 0, msgBytes.Length);
        }

        public override object Clone()
        {
            return this;
        }

        public override void OnAcceptConnection(ConnectionState state)
        {
            log("Connection from {0}", state.RemoteEndPoint.ToString());

            WriteLine(state, OHAI);
            if (DataSource != null)
            {
                foreach (var s in DataSource.GetData())
                {
                    WriteLine(state, s);
                }
            }
            WriteLine(state, KTHXBAI);

            log("Data Sent to {0}", state.RemoteEndPoint.ToString());

            state.EndConnection();
        }

        public override void OnReceiveData(ConnectionState state)
        {
            //Do nothing.
        }

        public override void OnDropConnection(ConnectionState state)
        {
            log("Connnection closed {0}", state.RemoteEndPoint.ToString());
        }
    }
}