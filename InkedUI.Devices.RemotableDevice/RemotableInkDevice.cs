using Ether.Network.Client;
using Ether.Network.Packets;
using InkedUI.Shared;
using InkedUI.Shared.Devices;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace InkedUI.Devices.RemotableDevice
{
    public class RemotableInkDevice : InkDevice
    {
        InkHostVirtualDeviceClient _client;

        public RemotableInkDevice(string remoteHost, int port = 1161)
        {
            _client = new InkHostVirtualDeviceClient(remoteHost, port, 100);
            _client.Connect();
        }

        private void Send(InkAction.InkActionTypes actionType)
        {
            var packet = new NetPacket();
            packet.Write(new InkAction(actionType).Serialize());
            _client.Send(packet);
        }

        public override async Task Clear()
        {
            Send(InkAction.InkActionTypes.Clear);
            await Task.Yield();
        }

        public override async Task Draw(EInkCanvas canvas)
        {
            var packet = new NetPacket();
            var inkAction = new InkAction(canvas); // draws
            packet.Write(inkAction.Serialize());
            _client.Send(packet);

            await Task.Yield();
        }

        public override async Task Init()
        {
            Send(InkAction.InkActionTypes.Init);
            await Task.Yield();
        }

        public override async Task Reset()
        {
            Send(InkAction.InkActionTypes.Reset);
            await Task.Yield();
        }
    }

    internal sealed class InkHostVirtualDeviceClient : NetClient
    {
        public InkHostVirtualDeviceClient(string host, int port, int bufferSize)
        {
            this.Configuration.Host = host;
            this.Configuration.Port = port;
            this.Configuration.BufferSize = bufferSize;
        }

        public override void HandleMessage(INetPacketStream packet)
        {
            var responseJson = packet.Read<string>();
            var response = InkAction.Deserialize(responseJson);

            if (response != null && response.InkActionType == InkAction.InkActionTypes.Ready)
            {
                // ready!
            }
        }

        /// <summary>
        /// Triggered when connected to the server.
        /// </summary>
        protected override void OnConnected()
        {
        }

        /// <summary>
        /// Triggered when disconnected from the server.
        /// </summary>
        protected override void OnDisconnected()
        {
        }

        /// <summary>
        /// Triggered when an error occurs.
        /// </summary>
        /// <param name="socketError"></param>
        protected override void OnSocketError(SocketError socketError)
        {
            Console.WriteLine("Socket Error: {0}", socketError.ToString());
        }
    }
}
