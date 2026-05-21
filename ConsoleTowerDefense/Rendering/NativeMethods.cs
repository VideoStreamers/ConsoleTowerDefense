using System;
using System.Runtime.InteropServices;

namespace ConsoleTowerDefense
{
    /* 
    This file is the bridge to the Windows Operating System. 
    It defines the specific data structures (memory blueprints) and functions that the Windows Kernel expects so you can bypass 
    the slow standard text-output system and speak directly to the GPU for high-performance graphics.
     */

    // Tells C# to keep these variables in this exact order for Windows
    [StructLayout(LayoutKind.Sequential)]
    public struct Coord { public short X; public short Y; } // Simple X and Y coordinate point

    // Allows two different variable types to sit in the exact same memory spot
    [StructLayout(LayoutKind.Explicit)]
    public struct CharUnion
    {
        [FieldOffset(0)] public char UnicodeChar; // Spot for a 2-byte symbol
        [FieldOffset(0)] public byte AsciiChar;   // Same spot for a 1-byte symbol
    }

    // A complete "Pixel" package containing a character and its color data
    [StructLayout(LayoutKind.Explicit)]
    public struct CharInfo
    {
        [FieldOffset(0)] public ushort UnicodeChar; // This is the character
        [FieldOffset(2)] public ushort Attributes;  // This is the color
    }

    // Defines a rectangular area for the window boundaries
    [StructLayout(LayoutKind.Sequential)]
    public struct SmallRect { public short Left; public short Top; public short Right; public short Bottom; }

    // Settings for the console font (Size, Weight, Name, etc.)
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct CONSOLE_FONT_INFOEX
    {
        public uint cbSize;             // Size of this data structure
        public uint nFont;              // Index of the font
        public Coord dwFontSize;        // Width and Height of the characters
        public int FontFamily;          // The font category (e.g. Modern)
        public int FontWeight;          // The thickness of the font
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string FaceName;         // The actual name (e.g. "Consolas")
    }

    // Hotlines to the core Windows library (kernel32.dll)
    internal static class NativeMethods
    {
        /// <summary>
        /// Retrieves a handle to the specified standard device (input, output, or error).
        /// </summary>
        /// <param name="nStdHandle">The standard device ID. -11 is the output buffer.</param>
        /// <returns>A handle (pointer) to the requested device.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetStdHandle(int nStdHandle);

        /// <summary>
        /// Sets extended information about the current console font.
        /// Used to change the font face and the pixel size of the console "pixels".
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetCurrentConsoleFontEx(
            IntPtr hConsoleOutput,
            bool bMaximumWindow,
            ref CONSOLE_FONT_INFOEX lpConsoleCurrentFontEx);

        /// <summary>
        /// Sets the current size and position of a console screen buffer's window.
        /// This effectively resizes the physical window you see on the desktop.
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleWindowInfo(
            IntPtr hConsoleOutput,
            bool bAbsolute,
            ref SmallRect lpConsoleWindow);

        /// <summary>
        /// Sets the size of the internal screen buffer. 
        /// This defines the grid dimensions (Width x Height) of our "canvas".
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleScreenBufferSize(
            IntPtr hConsoleOutput,
            Coord dwSize);

        /// <summary>
        /// Writes character and color data to a specified rectangular block in the console buffer.
        /// This is the high-performance heart of the renderer.
        /// </summary>
        [DllImport("kernel32.dll", EntryPoint = "WriteConsoleOutputW", SetLastError = true)]
        public static extern bool WriteConsoleOutput(
            IntPtr hConsoleOutput,
            CharInfo[] lpBuffer,
            Coord dwBufferSize,
            Coord dwBufferCoord,
            ref SmallRect lpWriteRegion);
    }
}