using Ether.Network.Common;
using Ether.Network.Packets;
using InkedUI.Shared;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace InkedUI.Devices.RemotableDevice
{
    public class InkHostClient : NetUser
    {
        public void SendReady()
        {
            Console.WriteLine($"[{this.Id}] Remote ink connection from {this.Socket.RemoteEndPoint.ToString()}.");
            using (var packet = new NetPacket())
            {
                packet.Write(new InkAction(InkAction.InkActionTypes.Ready).Serialize());
                this.Send(packet);
            }
        }

        public override void HandleMessage(INetPacketStream packet)
        {
            var jsonValue = packet.Read<string>();

            try
            {
                var value = InkAction.Deserialize(jsonValue);

                Console.WriteLine($"[{this.Id}] Processing action " + value.InkActionType.ToString());
                HandleMessageAsync(value).Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Deserialization exception: " + ex.ToString());
            }
        }

        private async Task HandleMessageAsync(InkAction value)
        {
            try
            {
                switch (value.InkActionType)
                {
                    case InkAction.InkActionTypes.Reset:
                        await InkHostServer.ActiveDevice.Reset();
                        break;
                    case InkAction.InkActionTypes.Init:
                        await InkHostServer.ActiveDevice.Init();
                        break;
                    case InkAction.InkActionTypes.Draw:
                        var canvas = EInkCanvas.ImportJson(value.CanvasJson);
                        Console.WriteLine($"[{this.Id}] Drawing {canvas.AvailableInkColors.Length} colors to {canvas.Width}x{canvas.Height} canvas.");
                        Console.WriteLine($"...   {canvas.Width}x{canvas.Height}");
                        Console.WriteLine($"...   Colors: {string.Join(", ", canvas.AvailableInkColors.Select(c => c.Name))}");
                        await InkHostServer.ActiveDevice.Draw(canvas);
                        break;
                    case InkAction.InkActionTypes.Clear:
                        await InkHostServer.ActiveDevice.Clear();
                        break;
                }
                Console.WriteLine($"[{this.Id}] Action complete.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
