# InkedUI

## Quickly and Easily Write Stylish e-Paper UIs with Familiar Technologies: **WinForms** and **WPF**

![E-Ink display demo image showing an example layout](https://github.com/anthturner/InkedUI/raw/master/EInkDisplayDemo.png)

---

This library was written to be able to quickly prototype and deploy user interfaces that are drawn on e-Paper devices.

The Forms and WPF components are still a work in progress; PRs are accepted and welcome!

This project is released under the MIT license. More information is available in LICENSE.

---

## Basic Use

```csharp
static void Main(string[] args)
{
    // Define the resolution of the display
    var x = 640;
    var y = 384;

    // Create a new EInkCanvas, which is what handles conversion to an ePaper-aware image
    var canvas = new InkedUI.Shared.EInkCanvas(x, y);

    // Define what colors the display can physically recreate
    canvas.AvailableInkColors = new Color[] { Color.Red, Color.Black, Color.White };

    // This pattern will draw a 4x2 grid of rectangles on the screen in red, black, and 'white':
    var colors = new Brush[]
    {
        Brushes.Red, Brushes.White, Brushes.Black, Brushes.White,
        Brushes.White, Brushes.Black, Brushes.White, Brushes.Red
    };

    // Use standard GDI image commands to draw the rectangles on the canvas
    using (var g = Graphics.FromImage(canvas.CanvasSurface))
    {
        var boxWidth = x / 4;
        var boxHeight = y / 2;

        var c = 0;
        for (int j = 0; j < y / boxWidth; j++)
            for (int i = 0; i < x / boxWidth; i++)
            {
                var color = colors[c++ % colors.Length];
                g.FillRectangle(color, i * boxWidth, j * boxHeight, boxWidth, boxHeight);
                var txtBrush = (color == Brushes.Black ? Brushes.White : Brushes.Black);
                var font = new Font("Verdana", 72.0f);
                var measure = g.MeasureString(c.ToString(), font);
                var ptX = (i * boxWidth) + (boxWidth - measure.Width) / 2;
                var ptY = (j * boxHeight) + (boxHeight - measure.Height) / 2;
                g.DrawString(c.ToString(), font, txtBrush, new PointF(ptX, ptY));
            }
    }

    // Handy command that will export bitmaps of each defined color, as well as a dithered, consolidated image
    // This outputs to the running directory and should probably not be used in production.
    canvas.ExportDebugKit();

    // Show the consolidated output image using the default picture viewer
    Process.Start("cmd.exe", "/c debug_consolidated.png");

    Console.WriteLine("Drawing to device...");

    // In this case we can use a RemotableInkDevice, which is hosted by the server sample.
    var dev = new InkedUI.Devices.RemotableDevice.RemotableInkDevice("192.168.1.100");

    // Run the device's Init routine
    dev.Init().Wait();
    
    // Sleep for 1 second in between init and draw to give the device a moment to settle
    Thread.Sleep(1000);

    // Draw the canvas to the device
    // In this case, we serialize the canvas and transmit it to the server
    dev.Draw(canvas).Wait();

    Console.WriteLine("Done!");
}
```
---
## Hosting a Remote Canvas
This library uses the **Ether.Network** and **Newtonsoft.Json** NuGet packages to provide server functionality.
```csharp
static void Main(string[] args)
{
    if (args.Length < 1)
    {
        Console.WriteLine("dotnet sample_server.dll <listening ip>");
        return;
    }

    Console.WriteLine("InkedUI Remote Canvas");

    // Port 1161 is default, no particular reason
    var port = 1161;
    Console.WriteLine($"Running Remotable Ink Host on port {port}!");

    // Set the Active Device -- the device this server will render incoming requests to
    InkHostServer.ActiveDevice = new WaveShareRaspberryPiDevice();

    // Create the server, listening to the given IP and port
    var server = new InkHostServer(args[0], port);

    // On client disconnect, restart the server to force-free all resources
    server.ClientDisconnected += (s, e) =>
    {
        server.Stop();
        server.Start();
    };

    // Start listening on the server
    server.Start();
}
```

---

## Stencil Editor

This project contains a window named "Stencil Editor", which is intended as a quick way to edit, save, and load pixel-perfect patterns inside of Visual Studio. It is incomplete and not properly tied to the UI.

While clicking the modal dialog button in the property grid of a supported object *will* show the dialog box, any meaningful action in the box will cause it to close.

---

## Projects

### InkedUI.Devices.RemotableDevice

Contains code to host a lightweight server that can be remotely connected to, which receives draw commands. **This is not a secure implementation and is not designed for production use!**

### InkedUI.Devices.WaveShare

Native C# module to interface directly with WaveShare 2.7" and 7.5" tricolor displays. Can be hosted in RemotableDevice component.

### InkedUI.Forms

Contains components to be used in place of WinForms standard components.

### InkedUI.WPF

Contains components to be used in place of WPF standard components, as well as Brushes that can be used for pixel-perfect patterns in the application.

### InkedUI.Shared

Contains components to be shared among other parts of the library.

---
