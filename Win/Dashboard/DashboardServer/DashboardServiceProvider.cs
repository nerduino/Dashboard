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

        public System.Diagnostics.EventLog EventLog { get; set; }

        public override object Clone()
        {
            return new DashboardServiceProvider();
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
            state.EndConnection();

            log("Data Send to {0}", state.RemoteEndPoint.ToString());
        }

        private bool WriteLine(ConnectionState state, string msg)
        {
            byte[] msgBytes = Encoding.ASCII.GetBytes(String.Format("{0}{1}", msg, Environment.NewLine));
            return state.Write(msgBytes, 0, msgBytes.Length);
        }



        public override void OnReceiveData(ConnectionState state)
        {
            //Ignore.
        }


        public override void OnDropConnection(ConnectionState state)
        {
            log("Connnection closed {0}", state.RemoteEndPoint.ToString());
        }

        
      

        private void log(string format, params object[] args)
        {
            if (EventLog != null)
            {
                EventLog.WriteEntry(String.Format(format, args));
            }
        }
        
    }
   
}