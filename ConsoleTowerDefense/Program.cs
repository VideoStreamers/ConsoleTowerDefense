namespace ConsoleTowerDefense
{
    internal class Program
    {
        static void Main()
        {
            // This code has been made by Seppe Dorissen
            // If you have any questions or want to use this code, please contact me at seppe.dorissen@outlook.com
            // more of my work can be found at https://seppedorissen.be

            // This project is a console-based tower defense game made in C# using .NET Framework.
            // My goal was to create a simple yet enjoyable tower defense game that runs in the console, showcasing my programming skills and creativity.
            // Whilst also stretching the limits of what can be achieved in a console application, by implementing features such as a game loop, rendering system, and basic game mechanics.

            GameEngine gameEngine = new GameEngine();
            GameManager gameManager = new GameManager();

            // Start the render loop
            gameEngine.Start();

            // Start the game loop
            gameManager.Start();
        }
    }
}