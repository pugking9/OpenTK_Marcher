using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace OpenTK_Marcher
{
    public static class Program
    {
        public static Vector2i winSize = new Vector2i(800, 600);
        private static void Main()
        {
            // Establish settings for new openGL window
            var nativeWindowSettings = new NativeWindowSettings()
            {
                Size = winSize,
                Title = "OpenTK Ray Marcher",
                // Documentation says this is needed for MacOS support
                // However, deprecated features won't be usable
                Flags = ContextFlags.ForwardCompatible,
            };

            // Create and run new window
            using (var window = new Window(GameWindowSettings.Default, nativeWindowSettings))
            {
                window.Run();
            }

        }
    }
}
