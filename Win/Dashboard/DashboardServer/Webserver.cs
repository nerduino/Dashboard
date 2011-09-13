using System.Net;
using System;
using System.Threading;


public class HttpEventArgs : EventArgs
{
    public HttpEventArgs(HttpListenerContext context)
        : base()
    {
        Context = context;
    }
    public HttpListenerContext Context { get; private set; }
}


public class HttpWebServer
{
    private static System.Threading.AutoResetEvent listenForNextRequest = new System.Threading.AutoResetEvent(false);
    private HttpListener httpListener;

    public HttpWebServer()
    {
        httpListener = new HttpListener();
    }

    public bool IsRunning { get; private set; }
    public HttpListenerPrefixCollection Prefixes
    {
        get
        {
            return httpListener.Prefixes;
        }
    }

    public void Start()
    {
        httpListener.Start();
        System.Threading.ThreadPool.QueueUserWorkItem(Listen);
    }

    public void Stop()
    {
        httpListener.Stop();
        IsRunning = false;
    }

    private void ListenerCallback(IAsyncResult result)
    {
        HttpListener listener = result.AsyncState as HttpListener;
        HttpListenerContext context = null;

        if (listener == null)
            // Nevermind 
            return;

        try
        {
            context = listener.EndGetContext(result);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());
            return;
        }
        finally
        {
            listenForNextRequest.Set();
        }
        if (context == null)
        {
            return;
        }

        OnProcessRequest(new HttpEventArgs(context));
    }

    // Loop here to begin processing of new requests. 
    private void Listen(object state)
    {
        while (httpListener.IsListening)
        {
            httpListener.BeginGetContext(new AsyncCallback(ListenerCallback), httpListener);
            listenForNextRequest.WaitOne();
        }
    }


    public event EventHandler<HttpEventArgs> ProcessRequest;

    private void OnProcessRequest(HttpEventArgs e)
    {
        if (ProcessRequest != null)
        {
            ProcessRequest(this, e);
        }
    }

}