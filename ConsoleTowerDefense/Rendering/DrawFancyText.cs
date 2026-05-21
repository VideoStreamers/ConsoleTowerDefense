using System;
using System.Collections.Generic;

namespace ConsoleTowerDefense
{
    internal static class DrawFancyText
    {
        // Custom "Alphabet" - 1 means pixel, 0 means empty
        // the reason of using a dictionary instead of an array or a list is to allow for easy lookup of characters by their char value, and to support a set of characters (like letters, numbers, and symbols) without needing to worry about indexing or gaps in the character set.
        private static readonly Dictionary<char, byte[,]> Font = new Dictionary<char, byte[,]>
        {
            {'A', new byte[,] { {0,1,1,0}  , {1,0,0,1}  , {1,1,1,1}  , {1,0,0,1}  , {1,0,0,1}   }},
            {'B', new byte[,] { {1,1,1,0}  , {1,0,0,1}  , {1,1,1,0}  , {1,0,0,1}  , {1,1,1,0}   }},
            {'C', new byte[,] { {0,1,1,1}  , {1,0,0,0}  , {1,0,0,0}  , {1,0,0,0}  , {0,1,1,1}   }},
            {'D', new byte[,] { {1,1,1,0}  , {1,0,0,1}  , {1,0,0,1}  , {1,0,0,1}  , {1,1,1,0}   }},
            {'E', new byte[,] { {1,1,1,1}  , {1,0,0,0}  , {1,1,1,0}  , {1,0,0,0}  , {1,1,1,1}   }},
            {'F', new byte[,] { {1,1,1,1}  , {1,0,0,0}  , {1,1,1,0}  , {1,0,0,0}  , {1,0,0,0}   }},
            {'G', new byte[,] { {0,1,1,1}  , {1,0,0,0}  , {1,0,1,1}  , {1,0,0,1}  , {0,1,1,1}   }},
            {'H', new byte[,] { {1,0,0,1}  , {1,0,0,1}  , {1,1,1,1}  , {1,0,0,1}  , {1,0,0,1}   }},
            {'I', new byte[,] { {1,1,1}    , {0,1,0}    , {0,1,0}    , {0,1,0}    , {1,1,1}     }},
            {'J', new byte[,] { {0,0,1,1}  , {0,0,0,1}  , {0,0,0,1}  , {1,0,0,1}  , {0,1,1,0}   }},
            {'K', new byte[,] { {1,0,0,1}  , {1,0,1,0}  , {1,1,0,0}  , {1,0,1,0}  , {1,0,0,1}   }},
            {'L', new byte[,] { {1,0,0,0}  , {1,0,0,0}  , {1,0,0,0}  , {1,0,0,0}  , {1,1,1,1}   }},
            {'M', new byte[,] { {1,0,0,0,1}, {1,1,0,1,1}, {1,0,1,0,1}, {1,0,0,0,1}, {1,0,0,0,1} }},
            {'N', new byte[,] { {1,0,0,1}  , {1,1,0,1}  , {1,0,1,1}  , {1,0,0,1}  , {1,0,0,1}   }},
            {'O', new byte[,] { {0,1,1,0}  , {1,0,0,1}  , {1,0,0,1}  , {1,0,0,1}  , {0,1,1,0}   }},
            {'P', new byte[,] { {1,1,1,0}  , {1,0,0,1}  , {1,1,1,0}  , {1,0,0,0}  , {1,0,0,0}   }},
            {'Q', new byte[,] { {0,1,1,0}  , {1,0,0,1}  , {1,0,0,1}  , {1,0,1,0}  , {0,1,0,1}   }},
            {'R', new byte[,] { {1,1,1,0}  , {1,0,0,1}  , {1,1,1,0}  , {1,0,1,0}  , {1,0,0,1}   }},
            {'S', new byte[,] { {0,1,1,1}  , {1,0,0,0}  , {0,1,1,0}  , {0,0,0,1}  , {1,1,1,0}   }},
            {'T', new byte[,] { {1,1,1,1,1}, {0,0,1,0,0}, {0,0,1,0,0}, {0,0,1,0,0}, {0,0,1,0,0} }},
            {'U', new byte[,] { {1,0,0,1}  , {1,0,0,1}  , {1,0,0,1}  , {1,0,0,1}  , {0,1,1,0}   }},
            {'V', new byte[,] { {1,0,0,1}  , {1,0,0,1}  , {1,0,0,1}  , {0,1,1,0}  , {0,0,1,0}   }},
            {'W', new byte[,] { {1,0,0,0,1}, {1,0,0,0,1}, {1,0,1,0,1}, {1,1,0,1,1}, {1,0,0,0,1} }},
            {'X', new byte[,] { {1,0,0,1}  , {0,1,1,0}  , {0,1,1,0}  , {0,1,1,0}  , {1,0,0,1}   }},
            {'Y', new byte[,] { {1,0,0,1}  , {1,0,0,1}  , {0,1,1,0}  , {0,1,0,0}  , {0,1,0,0}   }},
            {'Z', new byte[,] { {1,1,1,1}  , {0,0,0,1}  , {0,1,1,0}  , {1,0,0,0}  , {1,1,1,1}   }},
            {'0', new byte[,] { {0,1,1,0}  , {1,0,1,1}  , {1,1,0,1}  , {1,0,0,1}  , {0,1,1,0}   }},
            {'1', new byte[,] { {0,1,0}    , {1,1,0}    , {0,1,0}    , {0,1,0}    , {1,1,1}     }},
            {'2', new byte[,] { {1,1,1,0}  , {0,0,0,1}  , {0,1,1,0}  , {1,0,0,0}  , {1,1,1,1}   }},
            {'3', new byte[,] { {1,1,1,0}  , {0,0,0,1}  , {0,1,1,0}  , {0,0,0,1}  , {1,1,1,0}   }},
            {'4', new byte[,] { {1,0,1}    , {1,0,1}    , {1,1,1}    , {0,0,1}    , {0,0,1}     }},
            {'5', new byte[,] { {1,1,1,1}  , {1,0,0,0}  , {1,1,1,0}  , {0,0,0,1}  , {1,1,1,0}   }},
            {'6', new byte[,] { {0,1,1,1}  , {1,0,0,0}  , {1,1,1,0}  , {1,0,0,1}  , {0,1,1,0}   }},
            {'7', new byte[,] { {1,1,1,1}  , {0,0,0,1}  , {0,0,1,0}  , {0,1,0,0}  , {1,0,0,0}   }},
            {'8', new byte[,] { {0,1,1,0}  , {1,0,0,1}  , {0,1,1,0}  , {1,0,0,1}  , {0,1,1,0}   }},
            {'9', new byte[,] { {0,1,1,0}  , {1,0,0,1}  , {0,1,1,1}  , {0,0,0,1}  , {0,1,1,0}   }},
            {'!', new byte[,] { {0,1,0}    , {0,1,0}    , {0,1,0}    , {0,0,0}    , {0,1,0}     }},
            {'?', new byte[,] { {1,1,1,0}  , {0,0,0,1}  , {0,1,1,0}  , {0,0,0,0}  , {0,1,0,0}   }},
            {'>', new byte[,] { {1,0,0}    , {0,1,0}    , {0,0,1}    , {0,1,0}    , {1,0,0}     }},
            {'<', new byte[,] { {0,0,1}    , {0,1,0}    , {1,0,0}    , {0,1,0}    , {0,0,1}     }},
            {' ', new byte[,] { {0,0,0}    , {0,0,0}    , {0,0,0}    , {0,0,0}    , {0,0,0}     }}
        };

        /// <summary>
        /// Draws a string of text using the custom font defined in the Font dictionary. 
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="x">x position</param>
        /// <param name="y">y position</param>
        /// <param name="text"></param>
        /// <param name="color"></param>
        /// <param name="scale"></param>
        public static void DrawString(ConsoleRenderer renderer, int x, int y, string text, ConsoleColor color, int scale = 1)
        {
            int currentX = x;
            text = text.ToUpper();

            foreach (char character in text)
            {
                if (Font.ContainsKey(character))
                {
                    byte[,] sprite = Font[character];
                    int spriteWidth = sprite.GetLength(1);
                    int spriteHeight = sprite.GetLength(0);

                    // Draw the character
                    for (int row = 0; row < spriteHeight; row++)
                    {
                        for (int collumn = 0; collumn < spriteWidth; collumn++)
                        {
                            if (sprite[row, collumn] == 1)
                            {
                                DrawScaledPixel(renderer, currentX + (collumn * scale), y + (row * scale), color, scale);
                            }
                        }
                    }

                    // Set position for next character, adding 1 space between characters
                    currentX += (spriteWidth + 1) * scale;
                }
            }
        }

        /// <summary>
        /// Draws a single pixel at the specified position with the given color and scale.
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        /// <param name="scale"></param>
        private static void DrawScaledPixel(ConsoleRenderer renderer, int x, int y, ConsoleColor color, int scale)
        {
            // Do not draw if outside screen bounds
            if (x < 0 || x >= GameEngine.ScreenWidth || y < 0 || y >= GameEngine.ScreenHeight) return;

            // Draw a block of pixels to create a scaled effect
            for (int i = 0; i < scale; i++)
            {
                for (int j = 0; j < scale; j++)
                {
                    renderer.DrawPixel(x + i, y + j, color);
                }
            }
        }
    }
}