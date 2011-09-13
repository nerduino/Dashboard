using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

namespace Dashboard.Server
{
    public interface Logger
    {
        void Log(string message);
        void Log(string format, params object[] args);
    }

    public interface DashboardDataSource
    {
        IEnumerable<string> GetData();
        string ContentType { get; }
    }


    public class DashboardServer
    {
        private HttpWebServer webserver;
        private DashboardDataSource datasource = new PrivateDataSource();
        
        public Logger Log { get; set; }

        public DashboardServer(int port, DashboardDataSource datasource)
        {
            this.datasource = datasource;
            webserver = new HttpWebServer();
            webserver.Prefixes.Add(string.Format("http://+:{0}/", port));
            webserver.ProcessRequest += new EventHandler<HttpEventArgs>(webserver_ProcessRequest);
        }

        public void Start()
        {
            webserver.Start();
            Log.Log("Server started.");
        }

        public void Stop()
        {
            webserver.Stop();
            Log.Log("Server stopped.");
        }

        public void webserver_ProcessRequest(object sender, HttpEventArgs e)
        {
            HttpEventArgs args = e as HttpEventArgs;
            var context = args.Context;

            HttpListenerRequest request  = context.Request;
            HttpListenerResponse response = context.Response;
            byte[] buff; 
            try
            {
                    buff = Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, datasource.GetData()));
                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.StatusDescription = "OK";
            }
            catch
            {
                buff = Encoding.UTF8.GetBytes("");
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.StatusDescription = "Internal Server Error";
            }
            
            response.ContentType = datasource.ContentType;
            response.ProtocolVersion = new Version("1.1");
            response.KeepAlive = false;
     
            response.OutputStream.Write(buff, 0, buff.Length);

            try
            {
                Log.Log("{0} - {1} [{2}] \"{3} {4} HTTP/{5}\" {6} {7}",
                    request.RemoteEndPoint.Address,
                    context.User == null ? "-":context.User.ToString(),
                    DateTime.Now.ToString("dd/MM/yyyy:HH:mm:ss zzz"),
                    request.HttpMethod,
                    request.Url.AbsolutePath,
                    request.ProtocolVersion,
                    response.StatusCode,
                    buff.Length);
            }
            catch (Exception ex)
            {
                Log.Log(ex.ToString());
            }
            response.Close();
     }
    }
}