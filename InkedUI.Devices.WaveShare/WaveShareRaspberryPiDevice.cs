using InkedUI.Shared;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Unosquare.RaspberryIO.Abstractions;
using Unosquare.WiringPi;

namespace InkedUI.Devices.WaveShare
{
    public class WaveShareRaspberryPiDevice : WaveShare75Device
    {
        private IGpioPin BusyPin { get; set; }
        private IGpioPin ResetPin { get; set; }
        private IGpioPin DataPin { get; set; }
        private ISpiChannel Spi { get; set; }

        private enum LastModes { Data, Cmd, None };
        private LastModes _lastMode = LastModes.None;
        protected override bool IsBusy => !BusyPin.Read();

        //public WaveShareRaspberryPiDevice() : base(176, 264) { }
        public WaveShareRaspberryPiDevice() : base(640, 384) { }

        protected override async Task InitGpio()
        {
            Unosquare.RaspberryIO.Pi.Init<BootstrapWiringPi>();

            // NOTE: This is by BCM pin. VERY IMPORTANT!
            ResetPin = Unosquare.RaspberryIO.Pi.Gpio[17];
            BusyPin = Unosquare.RaspberryIO.Pi.Gpio[24];
            DataPin = Unosquare.RaspberryIO.Pi.Gpio[25];

            ResetPin.PinMode = GpioPinDriveMode.Output;
            BusyPin.PinMode = GpioPinDriveMode.Input;
            DataPin.PinMode = GpioPinDriveMode.Output;

            Spi = Unosquare.RaspberryIO.Pi.Spi.Channel0;
            Unosquare.RaspberryIO.Pi.Spi.Channel0Frequency = 2000000;

            await Task.Yield();
        }

        protected override async Task ResetDevice()
        {
            ResetPin.Write(GpioPinValue.Low);
            await Task.Delay(200);
            ResetPin.Write(GpioPinValue.High);
            await Task.Delay(200);
        }

        protected override async Task SendCommand(byte command)
        {
            //Console.WriteLine("C|" + command.ToString());
            DataPin.Write(GpioPinValue.Low);
            if (_lastMode != LastModes.Cmd) await Task.Delay(5);
            Spi.Write(new byte[] { command });
            _lastMode = LastModes.Cmd;
        }

        protected override async Task SendData(params byte[] data)
        {
            //Console.WriteLine("D|" + BitConverter.ToString(data));
            DataPin.Write(GpioPinValue.High);
            if (_lastMode != LastModes.Cmd) await Task.Delay(5);
            foreach (var b in data)
                Spi.Write(new[] { b });
            _lastMode = LastModes.Data;
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
