using InkedUI.Shared.Devices;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace InkedUI.Devices.WaveShare
{
    public abstract class WaveShareDevice : InkDevice
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
        private const byte DATA_START_TRANSMISSION_2 = 0x13;
        private const byte PARTIAL_DATA_START_TRANSMISSION_1 = 0x14;
        private const byte PARTIAL_DATA_START_TRANSMISSION_2 = 0x15;
        private const byte PARTIAL_DISPLAY_REFRESH = 0x16;
        private const byte LUT_FOR_VCOM = 0x20;
        private const byte LUT_WHITE_TO_WHITE = 0x21;
        private const byte LUT_BLACK_TO_WHITE = 0x22;
        private const byte LUT_WHITE_TO_BLACK = 0x23;
        private const byte LUT_BLACK_TO_BLACK = 0x24;
        private const byte PLL_CONTROL = 0x30;
        private const byte TEMPERATURE_SENSOR_COMMAND = 0x40;
        private const byte TEMPERATURE_SENSOR_CALIBRATION = 0x41;
        private const byte TEMPERATURE_SENSOR_WRITE = 0x42;
        private const byte TEMPERATURE_SENSOR_READ = 0x43;
        private const byte VCOM_AND_DATA_INTERVAL_SETTING = 0x50;
        private const byte LOW_POWER_DETECTION = 0x51;
        private const byte TCON_SETTING = 0x60;
        private const byte TCON_RESOLUTION = 0x61;
        private const byte SOURCE_AND_GATE_START_SETTING = 0x62;
        private const byte GET_STATUS = 0x71;
        private const byte AUTO_MEASURE_VCOM = 0x80;
        private const byte VCOM_VALUE = 0x81;
        private const byte VCM_DC_SETTING_REGISTER = 0x82;
        private const byte PROGRAM_MODE = 0xA0;
        private const byte ACTIVE_PROGRAM = 0xA1;
        private const byte READ_OTP_DATA = 0xA2;
        
        private const byte ROTATE_0 = 0;
        private const byte ROTATE_90 = 1;
        private const byte ROTATE_180 = 2;
        private const byte ROTATE_270 = 3;

        private static byte[] LUT_VCOM_DC = new byte[]
        {
            0x00, 0x00,
            0x00, 0x1A, 0x1A, 0x00, 0x00, 0x01,
            0x00, 0x0A, 0x0A, 0x00, 0x00, 0x08,
            0x00, 0x0E, 0x01, 0x0E, 0x01, 0x10,
            0x00, 0x0A, 0x0A, 0x00, 0x00, 0x08,
            0x00, 0x04, 0x10, 0x00, 0x00, 0x05,
            0x00, 0x03, 0x0E, 0x00, 0x00, 0x0A,
            0x00, 0x23, 0x00, 0x00, 0x00, 0x01
        };
        private static byte[] LUT_WW = new byte[]
        {
            0x90, 0x1A, 0x1A, 0x00, 0x00, 0x01,
            0x40, 0x0A, 0x0A, 0x00, 0x00, 0x08,
            0x84, 0x0E, 0x01, 0x0E, 0x01, 0x10,
            0x80, 0x0A, 0x0A, 0x00, 0x00, 0x08,
            0x00, 0x04, 0x10, 0x00, 0x00, 0x05,
            0x00, 0x03, 0x0E, 0x00, 0x00, 0x0A,
            0x00, 0x23, 0x00, 0x00, 0x00, 0x01
        };
        private static byte[] LUT_BW = new byte[]
        {
            0xA0, 0x1A, 0x1A, 0x00, 0x00, 0x01,
            0x00, 0x0A, 0x0A, 0x00, 0x00, 0x08,
            0x84, 0x0E, 0x01, 0x0E, 0x01, 0x10,
            0x90, 0x0A, 0x0A, 0x00, 0x00, 0x08,
            0xB0, 0x04, 0x10, 0x00, 0x00, 0x05,
            0xB0, 0x03, 0x0E, 0x00, 0x00, 0x0A,
            0xC0, 0x23, 0x00, 0x00, 0x00, 0x01
        };
        private static byte[] LUT_BB = new byte[]
        {
            0x90, 0x1A, 0x1A, 0x00, 0x00, 0x01,
            0x40, 0x0A, 0x0A, 0x00, 0x00, 0x08,
            0x84, 0x0E, 0x01, 0x0E, 0x01, 0x10,
            0x80, 0x0A, 0x0A, 0x00, 0x00, 0x08,
            0x00, 0x04, 0x10, 0x00, 0x00, 0x05,
            0x00, 0x03, 0x0E, 0x00, 0x00, 0x0A,
            0x00, 0x23, 0x00, 0x00, 0x00, 0x01
        };
        private static byte[] LUT_WB = new byte[]
        {
            0x90, 0x1A, 0x1A, 0x00, 0x00, 0x01,
            0x20, 0x0A, 0x0A, 0x00, 0x00, 0x08,
            0x84, 0x0E, 0x01, 0x0E, 0x01, 0x10,
            0x10, 0x0A, 0x0A, 0x00, 0x00, 0x08,
            0x00, 0x04, 0x10, 0x00, 0x00, 0x05,
            0x00, 0x03, 0x0E, 0x00, 0x00, 0x0A,
            0x00, 0x23, 0x00, 0x00, 0x00, 0x01
        };
        #endregion

        protected WaveShareDevice(int width, int height)
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

            if (skipFullInit)
                return;

            await SendCommand(POWER_ON);

            await SendCommand(PANEL_SETTING);
            // AF: KWR  || BF: KW  || 0F:  BWROTP
            await SendData(0xAF);

            await SendCommand(PLL_CONTROL);
            // 3A: 100Hz || 29: 150Hz || 39: 200Hz || 31: 171Hz
            await SendData(0x3A);

            await SendCommand(POWER_SETTING);
            await SendData(0x03); // VDS_EN, VDG_EN
            await SendData(0x00); // VCOM_HV, VGHL_LV[1], VGHL_LV[0]
            await SendData(0x2B); // VDH
            await SendData(0x2B); // VDL
            await SendData(0x09); // VDHR

            await SendCommand(BOOSTER_SOFT_START);
            await SendData(0x07);
            await SendData(0x07);
            await SendData(0x17);

            await SendCommand(0xF8); // Power optimization
            await SendData(0x60);
            await SendData(0xA5);

            await SendCommand(0xF8); // Power optimization
            await SendData(0x89);
            await SendData(0xA5);

            await SendCommand(0xF8); // Power optimization
            await SendData(0x90);
            await SendData(0x00);

            await SendCommand(0xF8); // Power optimization
            await SendData(0x93);
            await SendData(0x2A);

            await SendCommand(0xF8); // Power optimization
            await SendData(0x73);
            await SendData(0x41);

            await SendCommand(VCM_DC_SETTING_REGISTER);
            await SendData(0x12);
            await SendCommand(VCOM_AND_DATA_INTERVAL_SETTING);
            await SendData(0x87);

            await SetLut();

            await SendCommand(PARTIAL_DISPLAY_REFRESH);
            await SendData(0x00);
        }

        private async Task SetLut()
        {
            await SendCommand(LUT_FOR_VCOM);
            await SendData(LUT_VCOM_DC);

            await SendCommand(LUT_WHITE_TO_WHITE);
            await SendData(LUT_WW);

            await SendCommand(LUT_BLACK_TO_WHITE);
            await SendData(LUT_BW);

            await SendCommand(LUT_WHITE_TO_BLACK);
            await SendData(LUT_WB);

            await SendCommand(LUT_BLACK_TO_BLACK);
            await SendData(LUT_BB);
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
        
        private byte[] GetFrameData(Bitmap monoFrame)
        {
            var bitmapData = new byte[Width * Height / 8];
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    if (monoFrame.GetPixel(x, y).GetBrightness() < 0.5)
                        bitmapData[(x + y * Width) / 8] |= (byte)(0x80 >> (x % 8));
                }
            return bitmapData;
        }

        public async Task DisplayClearPattern()
        {
            var data = Enumerable.Repeat((byte)0xFF, Width * Height / 8).ToArray();
            var data2 = Enumerable.Repeat((byte)0xFF, Width * Height / 8).ToArray();

            Console.WriteLine("BlankFrame is " + BlankFrame.Length + " len");

            await DisplayFrameNative(BlankFrame, data2);
        }

        public async Task DisplayFrame(Bitmap monoFrame)
        {
            var data = GetFrameData(monoFrame);
            await DisplayFrameNative(BlankFrame, data);
        }

        public async Task DisplayFrame(Bitmap blackFrame, Bitmap redFrame)
        {
            var blackData = GetFrameData(blackFrame);
            var redData = GetFrameData(redFrame);
            await DisplayFrameNative(blackData, redData);
        }

        private async Task DisplayFrameNative(byte[] data1, byte[] data2)
        {
            await SetResolution();

            await SendCommand(DATA_START_TRANSMISSION_1);
            await SendData(data1);

            await SendCommand(DATA_START_TRANSMISSION_2);
            await SendData(data2);

            await SendCommand(DISPLAY_REFRESH);
            await SendData(0x00);

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

        protected async Task SetRotation(byte rotation)
        {
            if (rotation != ROTATE_0 &&
                rotation != ROTATE_90 &&
                rotation != ROTATE_180 &&
                rotation != ROTATE_270)
                throw new Exception("Must be one of ROTATE_* constants provided");

            Rotation = rotation;
            if (rotation == ROTATE_0 || rotation == ROTATE_180)
            {
                Width = PanelWidth;
                Height = PanelHeight;
            }
            else
            {
                Width = PanelHeight;
                Height = PanelWidth;
            }
        }
    }
}