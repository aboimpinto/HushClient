using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Olimpo.TcpClientManager;

public class Channel : IDisposable
{
    private const int BUFFER = 4096;            // Be able to receive 4MBytes of data

    private IClient _client;
    private NetworkStream _stream;
    private bool _disposed;
    private CancellationTokenSource _cancellationTokenSource;

    public Channel(IClient client, NetworkStream stream)
    {
        this._stream = stream;
        this._client = client;

        this._client.Running = true;
        this._client.Channel = this;

        var readingStreamThread = new Thread(new ParameterizedThreadStart(this.StartReadingStream));
        readingStreamThread.Start(this._stream);

        this._cancellationTokenSource = new CancellationTokenSource();
    }

    public async Task Send(string message)
    {
        var data = Encoding.UTF8.GetBytes(message);

        if (data.Length > BUFFER)
        {
            throw new InvalidOperationException("The message is too big to be sent");
        }

        await this._stream.WriteAsync(data, 0, data.Length);
        await Task.Delay(TimeSpan.FromMilliseconds(100));      // 100ms delay to give time to the server to process the message
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!this._disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }
            
            if (this._disposed)
            {
                return;
            }

            this._disposed = true;
            this._stream.Close();
            this._stream.Dispose();

            this._client.Stop();
        }
    }

    public void Dispose()
    {
        this._cancellationTokenSource.Cancel();

        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Close()
    {
        this._cancellationTokenSource.Cancel();

        this.Dispose(false);
        this._client.Running = false;
        this._client.Channel = null;
    }

    private void StartReadingStream(object obj)
    {
        

        byte[] buffer = new byte[BUFFER];     
        string data;

        var stream = (NetworkStream)obj;

        while(stream.CanRead && !this._cancellationTokenSource.IsCancellationRequested)
        {
            using (var ms = new MemoryStream())
            {
                do
                {
                    var numberOfBytesRead = stream.Read(buffer, 0, buffer.Length);
                    ms.Write(buffer, 0, numberOfBytesRead);

                } while(!this._cancellationTokenSource.IsCancellationRequested && stream.DataAvailable);

                data = Encoding.ASCII.GetString(ms.ToArray(), 0, (int)ms.Length);
                this._client.DataReceived?.OnNext(new DataReceivedArgs(data));
            }
        }
    }
}
