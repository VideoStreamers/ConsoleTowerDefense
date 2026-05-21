using System;
using System.Runtime.InteropServices;

namespace ConsoleTowerDefense
{
    /// <summary>
    /// Specifies the anchor point position for rendering sprites.
    /// </summary>
    public enum SpriteAlignment
    {
        TopLeft, TopCenter, TopRight,
        CenterLeft, Center, CenterRight,
        BottomLeft, BottomCenter, BottomRight
    }

    /// <summary>
    /// Handles high-performance rendering to the Windows console using double-buffering and raw Win32 APIs.
    /// Double buffering is a technique where you draw your entire frame to an off-screen buffer first, then push it to the screen in one fast operation.
    /// </summary>
    public class ConsoleRenderer
    {
        public static ConsoleRenderer Instance;
        public int Width;
        public int Height;

        // Native Windows handle and buffer structures
        private readonly IntPtr _hConsole;        // Pointer to the console window
        private readonly CharInfo[] _buffer;      // Internal array representing every "pixel" on screen
        private readonly Coord _bufferSize;       // Defines the width/height of the buffer
        private readonly Coord _bufferCoord;      // Starting coordinate (usually 0,0)
        private readonly ushort[] _colorCache;    // Cache of color values for fast lookup when drawing pixels
        private SmallRect _writeRegion;           // The actual rectangle area to draw in

        /// <summary>
        /// Initializes a new instance of the ConsoleRenderer with the specified dimensions and font size.
        /// </summary>
        public ConsoleRenderer(int width, int height, short fontSize)
        {
            if(Instance != null)
            {
                throw new Exception("ConsoleRenderer instance already exists!");
            }
            Instance = this;


            FileLogger.Instance.Log("");
            Width = width;
            Height = height;

            // -11 is the standard identifier for "Standard Output Handle" (STDOUT)
            _hConsole = NativeMethods.GetStdHandle(-11);

            // Initialize the memory buffer where we build our frames
            _buffer = new CharInfo[width * height];
            _bufferSize = new Coord { X = (short)width, Y = (short)height };
            _bufferCoord = new Coord { X = 0, Y = 0 };

            // Define the window region we are allowed to write to
            _writeRegion = new SmallRect
            {
                Left = 0,
                Top = 0,
                Right = (short)(width - 1),
                Bottom = (short)(height - 1)
            };

            // Precompute color attribute values for all ConsoleColor options to speed up pixel drawing
            _colorCache = new ushort[16];
            for (int i = 0; i < 16; i++)
            {
                // A Win32 console attribute byte maps [BBBBFFFF] (Background/Foreground).
                // Shifting the index left by 4 (i << 4) sets the background grid block color.
                // We combine it with 'i' using a bitwise OR (|) so that both the background color 
                // And the foreground text match perfectly. This prevents invisible text errors.
                ushort background = (ushort)(i << 4);
                ushort foreground = (ushort)i;

                _colorCache[i] = (ushort)(background | foreground);
            }

            InitializeConsole(fontSize);
        }

        /// <summary>
        /// Configures the Windows Console window properties (Font, Size, Buffers).
        /// </summary>
        private void InitializeConsole(short fontSize)
        {
            FileLogger.Instance.Log("");

            var cfi = new CONSOLE_FONT_INFOEX(); // ConsoleFontInfoEx is a struct that matches the Win32 API structure for console font settings
            cfi.cbSize = (uint)Marshal.SizeOf(cfi); // Marshal is a class that provides a collection of methods for allocating unmanaged memory, copying unmanaged memory blocks, and converting managed to unmanaged types, and vice versa.
            
            // dwFontSize is a Coord struct that specifies the width and height of each character cell in the console.
            // By setting both X and Y to the same value, each character cell is square.
            cfi.dwFontSize.X = fontSize;
            cfi.dwFontSize.Y = fontSize;
            cfi.FaceName = "Consolas";
            NativeMethods.SetCurrentConsoleFontEx(_hConsole, false, ref cfi);

            // Force the console window and buffer to match our game resolution
            var rect = new SmallRect { Left = 0, Top = 0, Right = 1, Bottom = 1 };
            NativeMethods.SetConsoleWindowInfo(_hConsole, true, ref rect);
            NativeMethods.SetConsoleScreenBufferSize(_hConsole, _bufferSize);
            NativeMethods.SetConsoleWindowInfo(_hConsole, true, ref _writeRegion);

            // Hide the blinking cursor
            Console.CursorVisible = false;
        }

        /// <summary>
        /// Wipes the entire screen buffer to prepare for a new frame.
        /// </summary>
        public void Clear()
        {
            //FileLogger.Instance.Log("");
            Array.Clear(_buffer, 0, _buffer.Length);
        }

        /// <summary>
        /// Sets a specific coordinate to a color.
        /// </summary>
        public void DrawPixel(int x, int y, ConsoleColor color)
        {
            // Bounds check to prevent exceptions.
            if (x < 0 || x >= Width || y < 0 || y >= Height) return;

            // Calculate the index in the buffer for the given (x,y) coordinate.
            int i = y * Width + x;
            _buffer[i].Attributes = _colorCache[(int)color]; // Set the color attributes for this "pixel"
            _buffer[i].UnicodeChar = 32; // ASCII for space
        }


        /// <summary>
        /// Draws a line between two points using repeated circle fills for smooth corners.
        /// </summary>
        /// <param name="x1">Starting X coordinate.</param>
        /// <param name="y1">Starting Y coordinate.</param>
        /// <param name="x2">Ending X coordinate.</param>
        /// <param name="y2">Ending Y coordinate.</param>
        /// <param name="thickness">The thickness of the line in pixels.</param>
        /// <param name="color">The color of the line.</param>
        public void DrawLine(int x1, int y1, int x2, int y2, int thickness, ConsoleColor color)
        {
            // Calculate the distance between the two points
            float dx = x2 - x1;
            float dy = y2 - y1;

            // Calculate the total length of the line
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);

            // Step at half-pixel intervals to ensure total coverage
            int steps = (int)(distance * 2);
            if (steps == 0) return;

            float xStep = dx / steps;
            float yStep = dy / steps;

            // Draw circles along the line path to create a thick line effect. This also smooths out corners and joins.
            for (int i = 0; i <= steps; i++)
            {
                float curX = x1 + (xStep * i);
                float curY = y1 + (yStep * i);

                DrawFilledCircle((int)curX, (int)curY, thickness / 2, color);
            }
        }

        /// <summary>
        /// Fast and optimized method for drawing solid, filled rectangles.
        /// </summary>
        /// <param name="x">The X coordinate of the rectangle's top-left corner.</param>
        /// <param name="y">The Y coordinate of the rectangle's top-left corner.</param>
        /// <param name="w">The width of the rectangle in pixels.</param>
        /// <param name="h">The height of the rectangle in pixels.</param>
        /// <param name="color">The color to fill the rectangle with.</param>
        public void DrawRectangle(int x, int y, int w, int h, ConsoleColor color)
        {
            ushort attributes = _colorCache[(int)color]; // Get the precomputed color attribute for the specified ConsoleColor

            // Clamp bounds once
            int xStart = Math.Max(0, x);
            int yStart = Math.Max(0, y);
            int xEnd = Math.Min(Width, x + w);
            int yEnd = Math.Min(Height, y + h);

            for (int row = yStart; row < yEnd; row++)
            {
                // Calculate the starting index for this row in the buffer
                int index = row * Width + xStart;
                for (int col = xStart; col < xEnd; col++)
                {
                    // Set the color attributes for this "pixel" in the buffer. We use a space character (ASCII 32) to create a solid block of color.
                    _buffer[index].Attributes = attributes;
                    _buffer[index].UnicodeChar = 32;
                    index++;
                }
            }
        }

        /// <summary>
        /// Draws an indexed color sprite onto the screen, applying palette and optional scaling/alignment.
        /// </summary>
        /// <param name="x">x position</param>
        /// <param name="y">y position</param>
        /// <param name="sprite">sprite as an array of bytes</param>
        /// <param name="palette">the color pallet corresponding to the byte values</param>
        /// <param name="scale">size drawn on screen</param>
        /// <param name="alignment">anchor point positioning</param>
        public void DrawIndexedSprite(int x, int y, byte[,] sprite, ConsoleColor[] palette, int scale = 1, SpriteAlignment alignment = SpriteAlignment.Center)
        {
            int rows = sprite.GetLength(0);
            int cols = sprite.GetLength(1);

            // Calculate offsets once
            int drawX = x;
            int drawY = y;
            ApplyAlignment(alignment, cols * scale, rows * scale, ref drawX, ref drawY);

            // Loop through the sprite's byte array and draw each pixel according to the palette.
            for (int row = 0; row < rows; row++)
            {
                for (int collumn = 0; collumn < cols; collumn++)
                {
                    byte index = sprite[row, collumn];
                    if (index == 0) continue;

                    ConsoleColor color = palette[index - 1];

                    if (scale == 1)
                    {
                        DrawPixel(drawX + collumn, drawY + row, color);
                    }
                    else
                    {
                        // Draw a mini-rectangle for the scaled pixel
                        DrawRectangle(drawX + (collumn * scale), drawY + (row * scale), scale, scale, color);
                    }
                }
            }
        }

        /// <summary>
        /// Adjusts the top-left drawing coordinates in-place to anchor a sprite relative to its intended alignment point.
        /// </summary>
        /// <param name="alignment">The anchor point rule used to calculate the offset.</param>
        /// <param name="width">The total width of the sprite in pixels (including scaling).</param>
        /// <param name="height">The total height of the sprite in pixels (including scaling).</param>
        /// <param name="x">The original X variable. Passed by reference (ref) so this method can rewrite the original variable in memory.</param>
        /// <param name="y">The original Y variable. Passed by reference (ref) so this method can rewrite the original variable in memory.</param>
        private void ApplyAlignment(SpriteAlignment alignment, int width, int height, ref int x, ref int y)
        {
            // The "ref" keyword tells the compiler to pass a pointer to the ACTUAL original variable instead.
            // This allows us to modify both 'x' and 'y' directly inside this helper method, and those
            // modifications will instantly apply to the variables on the outside calling this method.

            if (alignment == SpriteAlignment.Center || alignment == SpriteAlignment.TopCenter || alignment == SpriteAlignment.BottomCenter)
            {
                x -= width / 2; 
            }
            if (alignment == SpriteAlignment.TopRight || alignment == SpriteAlignment.CenterRight || alignment == SpriteAlignment.BottomRight)
            {
                x -= width;
            }

            // --- Vertical Alignment Handling (Y-Axis) ---
            if (alignment == SpriteAlignment.Center || alignment == SpriteAlignment.CenterLeft || alignment == SpriteAlignment.CenterRight)
            {
                y -= height / 2;
            }
            if (alignment == SpriteAlignment.BottomLeft || alignment == SpriteAlignment.BottomCenter || alignment == SpriteAlignment.BottomRight)
            {
                y -= height;
            }
        }

        /// <summary>
        /// Optimized circle drawing using the midpoint circle algorithm approach.
        /// </summary>
        /// <param name="centerX"></param>
        /// <param name="centerY"></param>
        /// <param name="radius"></param>
        /// <param name="color"></param>
        public void DrawFilledCircle(int centerX, int centerY, int radius, ConsoleColor color)
        {
            if (radius <= 0) { 
                DrawPixel(centerX, centerY, color); 
                return; 
            }

            for (int y = -radius; y <= radius; y++)
            {
                // Calculate the horizontal length of the circle at this vertical offset using the circle equation: x^2 + y^2 = r^2
                int xLenght = (int)Math.Sqrt(radius * radius - y * y);

                // Draw a horizontal line from (centerX - xLength) to (centerX + xLength) at the current vertical offset (centerY + y).
                int startX = centerX - xLenght;
                int endX = centerX + xLenght;

                DrawRectangle(startX, centerY + y, endX - startX + 1, 1, color);
            }
        }

        /// <summary>
        /// A helper method to draw a sprite using percentages directly, avoiding manual math in the ame objects.
        /// </summary>
        /// <param name="xPercentage"></param>
        /// <param name="yPercentage"></param>
        /// <param name="sprite"></param>
        /// <param name="palette"></param>
        /// <param name="scale"></param>
        /// <param name="alignment"></param>
        public void DrawIndexedSpritePercent(float xPercentage, float yPercentage, byte[,] sprite, ConsoleColor[] palette, int scale = 1, SpriteAlignment alignment = SpriteAlignment.Center)
        {
            Coord screenPos = GetPosFromPercent(xPercentage, yPercentage);
            DrawIndexedSprite(screenPos.X, screenPos.Y, sprite, palette, scale, alignment);
        }

        /// <summary>
        /// Converts simple standard percentages (0-100) into actual pixel coordinates.
        /// </summary>
        /// <param name="xPercentage"></param>
        /// <param name="yPercentage"></param>
        /// <returns></returns>
        public Coord GetPosFromPercent(float xPercentage, float yPercentage)
        {
            return new Coord
            {
                X = (short)(xPercentage * 0.01f * Width),
                Y = (short)(yPercentage * 0.01f * Height)
            };
        }

        /// <summary>
        /// Pushes the current memory buffer to the actual physical console screen.
        /// </summary>
        public void Render()
        {
            NativeMethods.WriteConsoleOutput(
                _hConsole,
                _buffer,
                _bufferSize,
                _bufferCoord,
                ref _writeRegion
            );
        }
    }
}