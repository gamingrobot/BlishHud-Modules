using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace CursorLock
{
    public static class NativeMethods
    {
        [DllImport("user32.dll")]
        public static extern int GetWindowRect(IntPtr hwnd, out RECT rc);
        [DllImport("user32.dll")]
        public static extern bool ClipCursor(ref RECT rcClip);
        [DllImport("user32.dll")]
        public static extern bool ClipCursor(IntPtr rcClip); //So we can reset the cursor clip

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }
    }
}
