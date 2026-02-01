// Drafted by GPT-5
using Godot;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public partial class BrowserDisplay(int port_)
{
    public int port = port_;

    private TcpListener _listener;
    private Thread _thread;
    private volatile bool _stopRequested;
    private volatile bool _listening;

    public Error Start()
    {
        if (_listening)
            return Error.Ok;

        try
        {
            _stopRequested = false;
            _listener = new TcpListener(IPAddress.Loopback, port);
            _listener.Start();

            _thread = new Thread(ServerLoop)
            {
                IsBackground = true,
                Name = $"BrowserDisplay:{port}"
            };
            _thread.Start();

            _listening = true;
            return Error.Ok;
        }
        catch
        {
            Stop();
            return Error.Failed;
        }
    }

    public static BrowserDisplay AnyAvailable(int startingPort = 8000, int maxPort = 8100)
    {
        for (int port = startingPort; port <= maxPort; port++)
        {
            var browserDisplay = new BrowserDisplay(port);
            if (browserDisplay.Start() == Error.Ok)
                return browserDisplay;
        }
        return null;
    }

    public void OpenBrowser(string resource)
    {
        OS.ShellOpen($"http://127.0.0.1:{port}/{resource}");
    }

    private void ServerLoop()
    {
        while (!_stopRequested)
        {
            TcpClient client = null;
            try
            {
                client = _listener.AcceptTcpClient();
                HandleClient(client);
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (SocketException)
            {
                if (_stopRequested)
                    break;
            }
            catch
            {
                // Best-effort: keep serving subsequent requests.
            }
            finally
            {
                try { client?.Close(); } catch { }
            }
        }
    }

    private void HandleClient(TcpClient client)
    {
        client.ReceiveTimeout = 500;
        client.SendTimeout = 500;

        using var stream = client.GetStream();

        // Read request headers (ignored). Cap to avoid unbounded reads.
        var buffer = new byte[8192];
        try
        {
            if (stream.DataAvailable)
                stream.Read(buffer, 0, buffer.Length);
        }
        catch
        {
            // Ignore malformed/slow clients.
        }

        string requestLine = Encoding.UTF8.GetString(buffer).Split("\r\n")[0];
        string resource = "/";
        {
            var parts = requestLine.Split(' ');
            if (parts.Length >= 2)
                resource = parts[1].TrimStart('/');
        }
        string body = GetResource(resource);
        byte[] bodyBytes = Encoding.UTF8.GetBytes(body);

        string header =
            "HTTP/1.1 200 OK\r\n" +
            "Content-Type: text/html; charset=utf-8\r\n" +
            $"Content-Length: {bodyBytes.Length}\r\n" +
            "Connection: close\r\n\r\n";

        byte[] headerBytes = Encoding.UTF8.GetBytes(header);
        stream.Write(headerBytes, 0, headerBytes.Length);
        stream.Write(bodyBytes, 0, bodyBytes.Length);
        stream.Flush();
    }

    private string GetResource(string resource)
    {
        return IndividualBankStatement.Example().Render();
    }

    public void Stop()
    {
        _stopRequested = true;
        _listening = false;

        try { _listener?.Stop(); } catch { }

        try
        {
            if (_thread != null && _thread.IsAlive)
                _thread.Join(1000);
        }
        catch { }

        _thread = null;
        _listener = null;
    }
}
