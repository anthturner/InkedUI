using InkedUI.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace InkedUI.Forms
{
    public static class WinFormsExtensions
    {
        public class RegisteredInkControl
        {
            public Control Control { get; set; }
            public Timer Timer { get; set; }
            public EInkCanvas Canvas { get; set; }

            public static void Create(Control control, EInkCanvas canvas, int pollingIntervalMs)
            {
                if (control.Width > canvas.Width || control.Height > canvas.Height)
                    throw new Exception($"Control must be equal to or smaller than the device's canvas (device canvas is {canvas.Width}x{canvas.Height})");
                
                var existingControl = _registeredControls.FirstOrDefault(c => c.Control == control);
                if (existingControl != null)
                {
                    existingControl.Timer.Stop();
                    _registeredControls.Remove(existingControl);
                }

                var registeredControl = new RegisteredInkControl()
                {
                    Control = control,
                    Canvas = canvas,
                    Timer = new Timer() { Interval = pollingIntervalMs }
                };
                registeredControl.Timer.Tick += (s, e) =>
                {
                    var bmp = new Bitmap(canvas.Width, canvas.Height);
                    using (Graphics gb = Graphics.FromImage(bmp))
                    using (Graphics gc = Graphics.FromHwnd(control.Handle))
                    {

                        IntPtr hdcDest = IntPtr.Zero;
                        IntPtr hdcSrc = IntPtr.Zero;

                        try
                        {
                            hdcDest = gb.GetHdc();
                            hdcSrc = gc.GetHdc();

                            // BitBlt copies directly from the gfx buffer; gets around GDI vs DX rendering issues
                            BitBlt(hdcDest, 0, 0, canvas.Width, canvas.Height, hdcSrc, 0, 0, SRC_COPY);
                        }
                        finally
                        {
                            if (hdcDest != IntPtr.Zero) gb.ReleaseHdc(hdcDest);
                            if (hdcSrc != IntPtr.Zero) gc.ReleaseHdc(hdcSrc);
                        }
                    }
                    canvas.UpdateSurface(bmp);
                };
                registeredControl.Timer.Start();

                _registeredControls.Add(registeredControl);
            }
        }

        private static List<RegisteredInkControl> _registeredControls = new List<RegisteredInkControl>();
        
        public static bool IsChildOf(this Control c, Control parent)
        {
            return ((c.Parent != null && c.Parent == parent) || (c.Parent != null ? c.Parent.IsChildOf(parent) : false));
        }

        public static RegisteredInkControl GetRegisteredControl(this Control control)
        {
            var firstAttempt = _registeredControls.FirstOrDefault(c => c.Control == control);
            if (firstAttempt != null)
                return firstAttempt;

            foreach (var rootControl in _registeredControls)
                if (control.IsChildOf(rootControl.Control))
                    return rootControl;

            return null;
        }

        [DllImport("gdi32.dll", EntryPoint = "BitBlt")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool BitBlt(
            [In()] System.IntPtr hdc, int x, int y, int cx, int cy,
            [In()] System.IntPtr hdcSrc, int x1, int y1, uint rop);

        private const int SRC_COPY = 0xCC0020;

        public static void RegisterForInk(this Control control, EInkCanvas canvas, int pollingIntervalMs = 500)
        {
            RegisteredInkControl.Create(control, canvas, pollingIntervalMs);
        }
    }
}
