using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ConsoleTowerDefense
{
    public class GameEngine
    {
        // Public variables
        public static GameEngine Instance;

        // Display Settings
        public const int ScreenWidth = 280;
        public const int ScreenHeight = 240;
        private const short FontPixelSize = 2; // 2x2 pixel characters for higher resolution, also makes it so we get a square pixel instead of a tall rectangular one.
        // Game Logic Settings
        private const int FixedUpdateHz = 30;

        // Private fields for game state and rendering
        private bool _isRunning = true;
        private int _frameCount = 0;
        private readonly Stopwatch _fpsTimer = Stopwatch.StartNew(); // Stopwatch is a high-resolution timer
        private readonly ConsoleRenderer _rendererObj;
        private int _currentDisplayFps = 0;

        // File Logger
        private readonly FileLogger _logger;
        private const string LogFileName = "game_log";
        private const string CustomFilesFolderName = "ConsoleTowerDefense_Custom_Files";
        private readonly string _folderPath;

        /// <summary>
        /// GameEngine is responsible for managing the main game loop, including both the update and render loops.
        /// </summary>
        public GameEngine()
        {
            _folderPath = CreateFolderPath(CustomFilesFolderName);

            _logger = new FileLogger(LogFileName, _folderPath);
            FileLogger.Instance.Log("");

            if (Instance != null)
            {
                throw new Exception("GameManager instance already exists!");
            }
            Instance = this;

            // Set up the renderer once when the game is created
            _rendererObj = new ConsoleRenderer(ScreenWidth, ScreenHeight, FontPixelSize);
        }

        /// <summary>
        /// This method creates a folder path for storing custom files (like logs) within the project directory.
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>
        private string CreateFolderPath(string folderName)
        {
            DirectoryInfo currentDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            DirectoryInfo projectRootDir = currentDir.Parent.Parent.Parent;
            string logsFolderPath = Path.Combine(projectRootDir.FullName, folderName);

            if (!Directory.Exists(logsFolderPath))
            {
                Directory.CreateDirectory(logsFolderPath);
            }

            return logsFolderPath;
        }

        /// <summary>
        /// Starts the game engine by launching the update loop on a separate thread and running the render loop on the main thread.
        /// </summary>
        public void Start()
        {
            FileLogger.Instance.Log("");

            // Start the physics thread
            Task.Run(() => UpdateLoop());

            // Start the render loop
            RenderLoop();
        }

        /// <summary>
        /// This loop is responsible for updating the game logic at a fixed rate.
        /// This allows the game to maintain smooth and consistent physics and game logic, even if the rendering takes variable time.
        /// </summary>
        private void UpdateLoop()
        {
            long ticksPerUpdate = TimeSpan.TicksPerSecond / FixedUpdateHz; // Calculate how many ticks should elapse for each update based on the desired fixed update rate
            Stopwatch timer = Stopwatch.StartNew(); // Start the timer to track elapsed time for updates
            long previousTicks = timer.ElapsedTicks; // Accumulator to keep track of how many ticks have passed since the last update
            long accumulator = 0; // This variable will accumulate the elapsed ticks and determine when to perform the next update

            while (_isRunning)
            {
                long currentTicks = timer.ElapsedTicks;
                accumulator += (currentTicks - previousTicks);
                previousTicks = currentTicks;

                while (accumulator >= ticksPerUpdate)
                {
                    GameTick();
                    accumulator -= ticksPerUpdate;
                }

                System.Threading.Thread.Sleep(1); // Sleep briefly to prevent this loop from consuming 100% CPU while waiting for the next update tick
            }
        }

        /// <summary>
        /// This method is called on each update tick to update the game logic. 
        /// Acting as a bridge between other logic systems that need to be updated at a fixed rate.
        /// </summary>
        private void GameTick()
        {
            GameManager.Instance.Update(GetDeltaTime());
        }

        /// <summary>
        /// This method is called as fast as possible on the main thread to render the game state to the console.
        /// Acting as a bridge between the other rendering systems and the ConsoleRenderer, allowing them to render the current game state to the console each frame.
        /// </summary>
        private void RenderLoop()
        {
            while (_isRunning)
            {
                ConsoleRenderer.Instance.Clear();

                GameManager.Instance.Render(ConsoleRenderer.Instance);

                ConsoleRenderer.Instance.Render();
                UpdateConsoleTitle();
            }
        }

        /// <summary>
        /// Tracks FPS count
        /// </summary>
        private int GetTrackFPS()
        {
            _frameCount++;
            // Update the console title with the current FPS and reset the frame count and timer every second
            if (_fpsTimer.ElapsedMilliseconds >= 1000)
            {
                _currentDisplayFps = _frameCount;

                _frameCount = 0;
                _fpsTimer.Restart();
            }

            return _currentDisplayFps;
        }

        /// <summary>
        /// Updates the console window title to display game and preformance information.
        /// </summary>
        private void UpdateConsoleTitle()
        {
            Console.Title = $"ConsoleTowerDefense | FPS: {GetTrackFPS()} | Logic: {FixedUpdateHz}Hz | Coins: {GameManager.Instance.GetCoins()} | Wave: {WaveManager.Instance.GetCurrentWave()}";
        }


        public string GetFolderPath() => _folderPath;
        public float GetDeltaTime() => 1/(float)FixedUpdateHz;
    }
}