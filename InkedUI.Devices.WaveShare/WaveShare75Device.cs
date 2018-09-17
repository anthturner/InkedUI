using InkedUI.Shared;
using InkedUI.Shared.Devices;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace InkedUI.Devices.WaveShare
{
    public abstract class WaveShare75Device : InkDevice
    {
        protected abstract bool IsBusy { get; }
        protected abstract Task InitGpio();
        protected abstract Task ResetDevice();
        protected abstract Task SendCommand(byte command);
        protected abstract Task SendData(params byte[] data);

        public int PanelWidth { get; private set; }
        public int PanelHeight { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public byte Rotation { get; private set; }

        public byte[] BlankFrame => Enumerable.Repeat((byte)0xFF, Width * Height / 8).ToArray();

        #region Constants
        private const byte PANEL_SETTING = 0x00;
        private const byte POWER_SETTING = 0x01;
        private const byte POWER_OFF = 0x02;
        private const byte POWER_OFF_SEQUENCE_SETTING = 0x03;
        private const byte POWER_ON = 0x04;
        private const byte POWER_ON_MEASURE = 0x05;
        private const byte BOOSTER_SOFT_START = 0x06;
        private const byte DEEP_SLEEP = 0x07;
        private const byte DATA_START_TRANSMISSION_1 = 0x10;
        private const byte DATA_STOP = 0x11;
        private const byte DISPLAY_REFRESH = 0x12;
        private const byte IMAGE_PROCESS = 0x13;
        private const byte LUT_FOR_VCOM = 0x20;
        private const byte LUT_BLUE = 0x21;
        private const byte LUT_WHITE = 0x22;
        private const byte LUT_GRAY_1 = 0x23;
        private const byte LUT_GRAY_2 = 0x24;
        private const byte LUT_RED_0 = 0x25;
        private const byte LUT_RED_1 = 0x26;
        private const byte LUT_RED_2 = 0x27;
        private const byte LUT_RED_3 = 0x28;
        private const byte LUT_XON = 0x29;
        private const byte PLL_CONTROL = 0x30;
        private const byte TEMPERATURE_SENSOR_COMMAND = 0x40;
        private const byte TEMPERATURE_CALIBRATION = 0x41;
        private const byte TEMPERATURE_SENSOR_WRITE = 0x42;
        private const byte TEMPERATURE_SENSOR_READ = 0x43;
        private const byte VCOM_AND_DATA_INTERVAL_SETTING = 0x50;
        private const byte LOW_POWER_DETECTION = 0x51;
        private const byte TCON_SETTING = 0x60;
        private const byte TCON_RESOLUTION = 0x61;
        private const byte SPI_FLASH_CONTROL = 0x65;
        private const byte REVISION = 0x70;
        private const byte GET_STATUS = 0x71;
        private const byte AUTO_MEASUREMENT_VCOM = 0x80;
        private const byte READ_VCOM_VALUE = 0x81;
        private const byte VCM_DC_SETTING = 0x82;
        #endregion

        protected WaveShare75Device(int width, int height)
        {
            PanelWidth = width;
            PanelHeight = height;
            Width = PanelWidth;
            Height = PanelHeight;
        }

        public async Task Init(bool skipFullInit = false)
        {
            await InitGpio();
            await ResetDevice();
            
            await SendCommand(POWER_SETTING);
            await SendData(0x37);
            await SendData(0x00);

            await SendCommand(PANEL_SETTING);
            await SendData(0xCF);
            await SendData(0x08);

            await SendCommand(BOOSTER_SOFT_START);
            await SendData(0xC7);
            await SendData(0xCC);
            await SendData(0x28);

            await SendCommand(POWER_ON);
            await WaitUntilIdle();

            await SendCommand(PLL_CONTROL);
            await SendData(0x3C);

            await SendCommand(TEMPERATURE_CALIBRATION);
            await SendData(0x00);

            await SendCommand(VCOM_AND_DATA_INTERVAL_SETTING);
            await SendData(0x77);

            await SendCommand(TCON_SETTING);
            await SendData(0x22);

            await SetResolution();

            await SendCommand(VCM_DC_SETTING);
            await SendData(0x1E);

            await SendCommand(0xE5);
            await SendData(0x03);
        }

        private async Task SetResolution()
        {
            var widthBytes = BitConverter.GetBytes(Width);
            var heightBytes = BitConverter.GetBytes(Height);

            await SendCommand(TCON_RESOLUTION);
            await SendData(
                widthBytes[1], widthBytes[0],
                heightBytes[1], heightBytes[0]);
        }

        private byte[] GetFrameData(DirectBitmap blackFrame, DirectBitmap redFrame, out DirectBitmap debugBmp)
        {
            Console.WriteLine($"Getting frame data custom for WxH {Width}x{Height}");
            debugBmp = new DirectBitmap(Width, Height);
            var bitmapData = new byte[(Width * Height)/2];
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x+=2)
                {
                    // 0x00 0000 0000 = black black
                    // 0x03 0000 0011 = black white
                    // 0x04 0000 0100 = black red

                    // 0x30 0011 0000 = white black
                    // 0x33 0011 0011 = white white
                    // 0x34 0011 0100 = white red

                    // 0x40 0100 0000 = red black
                    // 0x43 0100 0011 = red white
                    // 0x44 0100 0100 = red red

                    var value = GetPixel(blackFrame, redFrame, x, y, debugBmp);
                    var valueNext = GetPixel(blackFrame, redFrame, x + 1, y, debugBmp);
                    var b = (value << 4) | valueNext;
                    bitmapData[(Width * y + x) / 2] = (byte)b;
                }

            //var tempBmp = new DirectBitmap(Width, Height);
            //for (int j = 0; j < Height; j++)
            //    for (int i = 0; i < Width; i++)
            //    {
            //        Color px;
            //        var pos = bitmapData[(Width * j + i) / 2];
            //        if (i % 2 == 0 || i == 0)
            //            pos = (byte)(pos >> 4);
            //        else
            //            pos = (byte)(pos & 0x0f);

            //        if (pos == 0x00)
            //            px = Color.White;
            //        else if (pos == 0x03)
            //            px = Color.Black;
            //        else if (pos == 0x04)
            //            px = Color.Red;
            //        else
            //            Console.WriteLine("Unknown pixel type! " + pos);
                    
            //        tempBmp.SetPixel(i, j, px);
            //    }
            //tempBmp.Bitmap.Save("output_rebuilt.png");

            return bitmapData;
        }

        private byte GetPixel(DirectBitmap blackFrame, DirectBitmap redFrame, int x, int y, DirectBitmap debugBmp)
        {
            if (blackFrame.GetPixel(x, y).GetBrightness() < 0.5)
            {
                debugBmp.SetPixel(x, y, Color.White);
                return 0x03;
            }
            else if (redFrame.GetPixel(x, y).GetBrightness() < 0.5)
            {
                debugBmp.SetPixel(x, y, Color.Red);
                return 0x04;
            }
            else
            {
                debugBmp.SetPixel(x, y, Color.Black);
                return 0x00;
            }
        }

        public async Task DisplayClearPattern()
        {
            var data = Enumerable.Repeat((byte)0xFF, Width * Height / 8).ToArray();
            var data2 = Enumerable.Repeat((byte)0xFF, Width * Height / 8).ToArray();

            await DisplayFrameNative(BlankFrame);
        }

        public async Task DisplayFrame(DirectBitmap blackFrame, DirectBitmap redFrame)
        {
            Console.WriteLine("### DISPLAYFRAME ###");
            DirectBitmap bmp;
            var data = GetFrameData(blackFrame, redFrame, out bmp);
            bmp.Bitmap.Save("debug_bitwise.png", System.Drawing.Imaging.ImageFormat.Png);

            Console.WriteLine("DisplayFrameNative(data)");
            await DisplayFrameNative(data);
        }

        private async Task DisplayFrameNative(byte[] data1)
        {
            Console.WriteLine("### DISPLAYFRAMENATIVE ###");

            await SendCommand(DATA_START_TRANSMISSION_1);
            await SendData(data1);

            await SendCommand(DISPLAY_REFRESH);

            await WaitUntilIdle();
        }

        private async Task WaitUntilIdle()
        {
            while (IsBusy)
                await Task.Delay(100);
        }

        protected async Task Sleep()
        {
            await SendCommand(DEEP_SLEEP);
            await SendData(0xA5); // check code (only execute if check code is this)
        }
    }
}