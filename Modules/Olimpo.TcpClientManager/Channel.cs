using System.Net.Sockets;
using System.Text;

namespace Olimpo.TcpClientManager;

public class Channel : IDisposable
{
    private IClient _client;
    private NetworkStream _stream;
    private bool _disposed;
    

    public Channel(IClient client, NetworkStream stream)
    {
        this._stream = stream;
        this._client = client;

        this._client.Running = true;
        this._client.Channel = this;

        var readingStreamThread = new Thread(new ParameterizedThreadStart(this.StartReadingStream));
        readingStreamThread.Start(this._stream);
    }

    public void Send(string message)
    {
        var data = Encoding.UTF8.GetBytes(message);
        this._stream.Write(data, 0, data.Length);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!this._disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }
            
            this._stream.Close();
            this._client.Stop();
            this._disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Close()
    {
        this.Dispose(false);
        this._client.Running = false;
        this._client.Channel = null;
    }

    private void StartReadingStream(object obj)
    {
        byte[] buffer = new byte[256];;
        string data;
        var position = 0;

        var stream = (NetworkStream)obj;

        while(true)
        {
            while((position = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                data = Encoding.UTF8.GetString(buffer, 0, position);
                this._client.DataReceived?.OnNext(new DataReceivedArgs(data));
            }
        }        
    }
}
