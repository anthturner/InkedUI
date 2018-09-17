using InkedUI.Shared;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace InkedUI.Devices.WaveShare
{
    public class DummyDevice : WaveShare75Device
    {
        protected override bool IsBusy => false;

        public DummyDevice() : base(640, 384) { }

        protected override async Task InitGpio()
        {
            Console.WriteLine("InitGpio()");
            await Task.Yield();
        }

        protected override async Task ResetDevice()
        {
            Console.WriteLine("ResetDevice()");
            await Task.Delay(500);
        }

        protected override async Task SendCommand(byte command)
        {
            Console.WriteLine($"SendCommand({BitConverter.ToString(new byte[] { command })}");
            await Task.Yield();
        }

        protected override async Task SendData(params byte[] data)
        {
            Console.WriteLine($"SendData({BitConverter.ToString(data)}");
            await Task.Yield();
        }

        public override async Task Draw(EInkCanvas canvas)
        {
            var ms1 = new MemoryStream();
            var ms2 = new MemoryStream();
            try
            {
                Console.WriteLine("Rendering Black/White image ...");
                canvas.Export(System.Drawing.Imaging.ImageFormat.Bmp,
                    Color.White,
                    ms1);

                Console.WriteLine("Rendering Red image ...");
                canvas.Export(System.Drawing.Imaging.ImageFormat.Bmp,
                    Color.Red,
                    ms2);

                Console.WriteLine("Rewind!");

                ms1.Seek(0, SeekOrigin.Begin);
                ms2.Seek(0, SeekOrigin.Begin);
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }

            Console.WriteLine("Enter DisplayFrame()");
            await this.DisplayFrame(
                new DirectBitmap((Bitmap)Image.FromStream(ms1)),
                new DirectBitmap((Bitmap)Image.FromStream(ms2)));
        }

        public override async Task Clear()
        {
            await this.DisplayClearPattern();
        }

        public override async Task Reset()
        {
            await this.ResetDevice();
        }

        public override async Task Init()
        {
            await base.Init();
        }
    }
}
