using Ether.Network.Server;
using InkedUI.Shared.Devices;
using System;

namespace InkedUI.Devices.RemotableDevice
{
    public class InkHostServer : NetServer<InkHostClient>
    {
        public event EventHandler ClientDisconnected;
        public static InkDevice ActiveDevice { get; set; }

        public InkHostServer(string host, int port)
        {
            this.Configuration.Backlog = 100;
            this.Configuration.Port = port;
            this.Configuration.MaximumNumberOfConnections = 100;
            this.Configuration.Host = host;
            this.Configuration.BufferSize = 8;
            this.Configuration.Blocking = true;
        }

        protected override void Initialize()
        {
            Console.WriteLine("Server ready.");
        }

        protected override void OnClientConnected(InkHostClient connection)
        {
            Console.WriteLine($"New connection from {connection.Socket.RemoteEndPoint.ToString()}.");
            connection.SendReady();
        }

        protected override void OnClientDisconnected(InkHostClient connection)
        {
            Console.WriteLine("Client disconnected.");
            ClientDisconnected?.Invoke(this, new EventArgs());
        }

        protected override void OnError(Exception exception)
        {
            Console.WriteLine("Exception\n" + exception.ToString());
        }
    }
}
