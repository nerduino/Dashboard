using System.Net;
using System;
public class Server
{
    private static System.Threading.AutoResetEvent listenForNextRequest = new System.Threading.AutoResetEvent(false);

    protected Server()
    {
        _httpListener = new HttpListener();
    }

    private HttpListener _httpListener;

    public string Prefix { get; set; }
    public void Start()
    {
        if (string.IsNullOrEmpty(Prefix))
            throw new InvalidOperationException("No prefix has been specified");
        _httpListener.Prefixes.Clear();
        _httpListener.Prefixes.Add(Prefix);
        _httpListener.Start();
        System.Threading.ThreadPool.QueueUserWorkItem(Listen);
    }

    internal void Stop()
    {
        _httpListener.Stop();
        IsRunning = false;
    }

    public bool IsRunning { get; private set; }

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
            return;
        //ProcessRequest(context);
    }

    // Loop here to begin processing of new requests. 
    private void Listen(object state)
    {
        while (_httpListener.IsListening)
        {
            _httpListener.BeginGetContext(new AsyncCallback(ListenerCallback), _httpListener);
            listenForNextRequest.WaitOne();
        }
    }

}