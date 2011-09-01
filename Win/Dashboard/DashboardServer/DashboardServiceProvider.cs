using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dashboard.Server.Tcp;
using LinqToWmi.Core.WMI;
using System.Management;
using WmiEntities;
using System.Text.RegularExpressions;


namespace Dashboard.Server
{
    public class DashboardServiceProvider : TcpServiceProvider
    {

        private static string OHAI = "OHAI";
        private static string KTHXBAI = "KTHXBAI";

        public Logger Logger { get; set; }

        public override object Clone()
        {
            return this;
        }

        public override void OnAcceptConnection(ConnectionState state)
        {
            log("Connection from {0}",state.RemoteEndPoint.ToString());
            WriteLine(state, OHAI);
            
            using (WmiContext context = new WmiContext(@"\\."))
            {
                context.ManagementScope.Options.Impersonation = ImpersonationLevel.Impersonate;
         
                foreach (var s in Private.GetProcessData(context))
               {
                    WriteLine(state, s);
               }
            }
            
            WriteLine(state, KTHXBAI);
            
            log("Data Sent to {0}", state.RemoteEndPoint.ToString());

            state.EndConnection();
        }

        private bool WriteLine(ConnectionState state, string msg)
        {
            byte[] msgBytes = Encoding.ASCII.GetBytes(String.Format("{0}{1}", msg, Environment.NewLine));
            return state.Write(msgBytes, 0, msgBytes.Length);
        }



        public override void OnReceiveData(ConnectionState state)
        {
            //Do nothing.
        }


        public override void OnDropConnection(ConnectionState state)
        {
            log("Connnection closed {0}", state.RemoteEndPoint.ToString());
        }

     
        private void log(string format, params object[] args)
        {
            if (Logger != null)
            {
                Logger.Log(format, args);
            }
        }
        
    }
   
    public interface Logger
    {
        void Log(string message);
        void Log(string format, params object[] args);
    }
}