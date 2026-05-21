using System;

namespace ConsoleTowerDefense
{
    internal class GameManager
    {
        // Public reference objects
        public static GameManager Instance;
        public Castle CastleObj;
        public Map MapObj;

        // Private references (has instance)
        private readonly EnemyManager _enemyManager;
        private readonly WaveManager _waveManagerObj;
        private readonly TowerManager _towerManagerObj;

        // Private variables
        private int _currentLevel = 0;
        private int _coins = 240;
        private bool _uiOpen = false;
        private readonly bool _uiValuesFetched = false;

        // Xml Parser
        private readonly XmlHandler _xmlParser;
        private const string WaveFileName = "game_wave_data";
        private const string SaveGameFileName = "player_save_data";

        // Private state management
        private enum GameState { StartScreen, Playing, Won, Lost }
        private GameState _currentState = GameState.StartScreen;

        // UI Positions
        private readonly int _uiX = 0;
        private readonly int _uiY = 0;

        // Main UI Layout Constants
        private const int UiXOffsetFromRight = 0;
        private const int UiYOffsetFromTop = 8;
        private const int UiTextOffsetX = 4;
        private const int UiTextOffsetY = 4;
        private const int UiWidth = 180;
        private const int UiBorderSize = 4;
        private const int UiHeight = 110;
        private const int UiTextScale = 1;
        private const int UiLineSpacing = 12;

        // Hidden Menu Constants
        private const int HiddenMenuWidth = 70;
        private const int HiddenMenuHeight = 14;
        private const int HiddenMenuBorderSize = 2;
        private const int HiddenMenuY = 4;
        private const int HiddenMenuTextOffsetX = 2;
        private const int HiddenMenuTextOffsetY = 2;

        public GameManager()
        {
            FileLogger.Instance.Log("");
            if (Instance != null) throw new Exception("GameManager already exists!");
            Instance = this;

            // Initialize game systems
            _enemyManager = new EnemyManager();
            _waveManagerObj = new WaveManager();
            _towerManagerObj = new TowerManager();

            // Initialize game entities
            CastleObj = new Castle(100, 100);
            MapObj = new Map();

            // Initialize XML Handler
            _xmlParser = new XmlHandler(WaveFileName, SaveGameFileName, GameEngine.Instance.GetFolderPath());
        }


        /// <summary>
        /// Starts the game by setting the initial state and performing any necessary setup.
        /// </summary>
        public void Start()
        {
            FileLogger.Instance.Log("");

            _currentState = GameState.StartScreen;
        }

        /// <summary>
        /// Updates the game logic based on the current state. Handles input and updates game entities when in the Playing state.
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Update(float deltaTime)
        {
            HandleInput();

            switch (_currentState)
            {
                case GameState.StartScreen:
                    return;

                case GameState.Playing:
                    WaveManager.Instance.UpdateWaves(deltaTime);
                    TowerManager.Instance.Update(deltaTime);
                    EnemyManager.Instance.UpdateEnemies(deltaTime);
                    break;
            }
        }

        /// <summary>
        /// Handles player input based on the current game state.
        /// </summary>
        private void HandleInput()
        {
            if (Console.KeyAvailable)
            {
                ConsoleKey pressedKey = Console.ReadKey(true).Key;

                if (_currentState == GameState.StartScreen)
                {
                    if (pressedKey == ConsoleKey.Spacebar)
                    {
                        _currentState = GameState.Playing;
                        WaveManager.Instance.StartWaveTimer();
                        return;
                    }
                }

                if (_currentState == GameState.Playing)
                {
                    // Debug Keys
                    // Do Damage to Castle (for testing)
                    if (pressedKey == ConsoleKey.K) { CastleObj.TakeDamage(10); }

                    // Skip Wave (for testing)
                    if (pressedKey == ConsoleKey.L) { WaveManager.Instance.NextWave(); }

                    // Damage all enemies (for testing)
                    if (pressedKey == ConsoleKey.M) { EnemyManager.Instance.DamageAllEnemies(10); }

                    // Skip to failed state (for testing)
                    if (pressedKey == ConsoleKey.N) { SetWinLoss(false); }

                    // Skip to win state (for testing)
                    if (pressedKey == ConsoleKey.B) { SetWinLoss(true); }

                    // Game Keys
                    // Toggle UI
                    if (pressedKey == ConsoleKey.H) { _uiOpen = !_uiOpen; }

                    // Cycle through towers with left and right arrow keys
                    if (pressedKey == ConsoleKey.RightArrow) { TowerManager.Instance.ChangeSelectedTowerIndex(true); }
                    if (pressedKey == ConsoleKey.LeftArrow) { TowerManager.Instance.ChangeSelectedTowerIndex(false); }

                    // Buy Archer Tower
                    if (pressedKey == ConsoleKey.Q)
                    {
                        int[] selectedTowerSpot = MapObj.GetSelectedTowerSpot();
                        int x = selectedTowerSpot[0];
                        int y = selectedTowerSpot[1];

                        Tower tower = TowerManager.Instance.CreateTowerFromType("ArcherTower", x, y);

                        TowerManager.Instance.AddTower(tower);
                    }
                    if (pressedKey == ConsoleKey.W)
                    {
                        int[] selectedTowerSpot = MapObj.GetSelectedTowerSpot();
                        int x = selectedTowerSpot[0];
                        int y = selectedTowerSpot[1];

                        Tower tower = TowerManager.Instance.CreateTowerFromType("BomberTower", x, y);

                        TowerManager.Instance.AddTower(tower);
                    }
                    if (pressedKey == ConsoleKey.E)
                    {
                        int[] selectedTowerSpot = MapObj.GetSelectedTowerSpot();
                        int x = selectedTowerSpot[0];
                        int y = selectedTowerSpot[1];

                        Tower tower = TowerManager.Instance.CreateTowerFromType("SniperTower", x, y);

                        TowerManager.Instance.AddTower(tower);
                    }
                    if (pressedKey == ConsoleKey.R)
                    {
                        int[] selectedTowerSpot = MapObj.GetSelectedTowerSpot();
                        int x = selectedTowerSpot[0];
                        int y = selectedTowerSpot[1];

                        Tower tower = TowerManager.Instance.CreateTowerFromType("TurretTower", x, y);

                        TowerManager.Instance.AddTower(tower);
                    }

                    // Sell Tower
                    if (pressedKey == ConsoleKey.Backspace)
                    {
                        int[] selectedTowerSpot = MapObj.GetSelectedTowerSpot();
                        int x = selectedTowerSpot[0];
                        int y = selectedTowerSpot[1];

                        TowerManager.Instance.SellTowerAtPosition(x, y);
                    }
                }
                if (GameState.Lost == _currentState || GameState.Won == _currentState)
                {
                    if (pressedKey == ConsoleKey.Escape)
                    {
                        // delete player file and close game
                        XmlHandler.Instance.DeleteSaveDataFile();
                        Environment.Exit(0);
                    }

                    if (pressedKey == ConsoleKey.Enter)
                    {
                        // close game
                        Environment.Exit(0);
                    }
                }
            }
        }

        /// <summary>
        /// Renders the game based on the current state.
        /// </summary>
        /// <param name="renderer"></param>
        public void Render(ConsoleRenderer renderer)
        {
            switch (_currentState)
            {
                case GameState.StartScreen:
                    ShowStartScreen(renderer);
                    break;

                case GameState.Playing:
                    if (!MapObj.GetMapLoaded()) // Check if map needs loading (first time entering Playing state)
                    {
                        MapObj.LoadLevel(renderer, _currentLevel);
                    }

                    // draw call for all managers and entities
                    MapObj.Draw(renderer);
                    EnemyManager.Instance.DrawEnemies();
                    CastleObj.Draw(renderer);
                    TowerManager.Instance.Draw();
                    DrawUI();
                    
                    break;

                case GameState.Won:
                    DrawWinLossScreen();
                    break;

                case GameState.Lost:
                    DrawWinLossScreen();
                    break;
            }
        }

        /// <summary>
        /// Renders the start screen with a big title and dynamic instructions.
        /// </summary>
        /// <param name="renderer"></param>
        public void ShowStartScreen(ConsoleRenderer renderer)
        {
            // Big Title
            DrawFancyText.DrawString(renderer, 10, 40, "Console", ConsoleColor.Yellow, 3);
            DrawFancyText.DrawString(renderer, 10, 60, "Tower", ConsoleColor.Yellow, 3);
            DrawFancyText.DrawString(renderer, 10, 80, "Defense", ConsoleColor.Yellow, 3);

            // Dynamic instructions
            DrawFancyText.DrawString(renderer, 10, 110, "PROTECT THE CASTLE", ConsoleColor.Gray, 2);
            DrawFancyText.DrawString(renderer, 10, 130, "AT ALL COSTS", ConsoleColor.Red, 2);

            if (DateTime.Now.Millisecond < 500) // Flash "Press Space to Start" every half second
            {
                DrawFancyText.DrawString(renderer, 10, 180, "PRESS SPACE TO START", ConsoleColor.White, 2);
            }
        }

        
        /// <summary>
        /// Attempts to change the player's coin count by the specified amount.
        /// Failed transaction returns false, successful transaction returns true.
        /// </summary>
        /// <param name="coins">Positive or negative number to subtract or add to the coin count</param>
        /// <returns></returns>
        public bool ChangeCoins(int coins)
        {
            FileLogger.Instance.Log("");
            if (_coins + coins < 0)
            {
                return false;
            }
            else
            {
                _coins += coins;
                return true;
            }
        }


        /// <summary>
        /// Draws the in-game UI panel on the right side of the screen when open, and a minimal tab when closed.
        /// </summary>
        private void DrawUI()
        {
            ConsoleRenderer renderer = ConsoleRenderer.Instance;
            if (renderer == null) return;

            // Fetch dynamic values for UI display
            int currentCoins = _coins;
            int archerCost = TowerManager.Instance.GetTowerCost("ArcherTower");
            int bomberCost = TowerManager.Instance.GetTowerCost("BomberTower");
            int sniperCost = TowerManager.Instance.GetTowerCost("SniperTower");
            int turretCost = TowerManager.Instance.GetTowerCost("TurretTower");

            if (_uiOpen)
            {
                // Calculate open UI bounding coordinates
                int panelX = renderer.Width - UiWidth - UiXOffsetFromRight;
                int panelY = _uiY + UiYOffsetFromTop;

                //// Draw structural outer border and inner container panels
                renderer.DrawRectangle(panelX - UiBorderSize, panelY - UiBorderSize, UiWidth, UiHeight, ConsoleColor.Gray);
                renderer.DrawRectangle(panelX - (UiBorderSize / 2), panelY - (UiBorderSize / 2), UiWidth - UiBorderSize, UiHeight - UiBorderSize, ConsoleColor.DarkGray);

                // Map textual starting position
                int textX = panelX + UiTextOffsetX;
                int textY = panelY + UiTextOffsetY;

                // Render instruction context strings line by line
                DrawFancyText.DrawString(renderer, textX, textY, $"YOU HAVE {currentCoins} COINS", ConsoleColor.Yellow, UiTextScale);
                textY += UiLineSpacing * UiTextScale;

                DrawFancyText.DrawString(renderer, textX, textY, $"Q > BUILD ARCHER > COSTS {archerCost}", ConsoleColor.Cyan, UiTextScale);
                textY += UiLineSpacing * UiTextScale;
                DrawFancyText.DrawString(renderer, textX, textY, $"W > BUILD BOMBER > COSTS {bomberCost}", ConsoleColor.Cyan, UiTextScale);
                textY += UiLineSpacing * UiTextScale;
                DrawFancyText.DrawString(renderer, textX, textY, $"E > BUILD SNIPER > COSTS {sniperCost}", ConsoleColor.Cyan, UiTextScale);
                textY += UiLineSpacing * UiTextScale;
                DrawFancyText.DrawString(renderer, textX, textY, $"R > BUILD TURRET > COSTS {turretCost}", ConsoleColor.Cyan, UiTextScale);
                textY += UiLineSpacing * UiTextScale;

                DrawFancyText.DrawString(renderer, textX, textY, "ARR LEFT AND RIGHT > CYCLE SPOT", ConsoleColor.Green, UiTextScale);
                textY += UiLineSpacing * UiTextScale;

                DrawFancyText.DrawString(renderer, textX, textY, "BACKSPACE > SELL > REFUNDS COST", ConsoleColor.Green, UiTextScale);
                textY += UiLineSpacing * UiTextScale;

                DrawFancyText.DrawString(renderer, textX, textY, "H > CLOSE MENU", ConsoleColor.Gray, UiTextScale);
            }
            else
            {
                // Calculate compressed hidden notification coordinates 
                int hiddenMenuPosX = renderer.Width - HiddenMenuWidth;
                int hiddenMenuTextPosX = hiddenMenuPosX + HiddenMenuTextOffsetX;
                int hiddenMenuTextPosY = HiddenMenuY + HiddenMenuTextOffsetY;

                // Draw structural tabs
                renderer.DrawRectangle(hiddenMenuPosX - HiddenMenuBorderSize, HiddenMenuY - HiddenMenuBorderSize, HiddenMenuWidth, HiddenMenuHeight, ConsoleColor.Gray);
                renderer.DrawRectangle(hiddenMenuPosX - (HiddenMenuBorderSize / 2), HiddenMenuY - (HiddenMenuBorderSize / 2), HiddenMenuWidth - HiddenMenuBorderSize, HiddenMenuHeight - HiddenMenuBorderSize, ConsoleColor.DarkGray);

                DrawFancyText.DrawString(renderer, hiddenMenuTextPosX, hiddenMenuTextPosY, "H > OPEN MENU", ConsoleColor.Gray, UiTextScale);
            }
        }

        /// <summary>
        /// Draws the win or loss screen with appropriate messaging and options to exit the game, either keeping or deleting the save file.
        /// </summary>
        private void DrawWinLossScreen()
        {
            int textX = ConsoleRenderer.Instance.Width / 8; // Start text more towards the left side of the screen for better centering of long messages
            int textY = ConsoleRenderer.Instance.Height / 2;

            ConsoleRenderer renderer = ConsoleRenderer.Instance;
            if (renderer == null) return;
    
            string message = _currentState == GameState.Won ? "LEVEL COMPLETED!" : "CASTLE DESTROYED";

            DrawFancyText.DrawString(renderer, textX, textY, message, ConsoleColor.Gray, UiTextScale);
            textY += UiLineSpacing * UiTextScale;

            DrawFancyText.DrawString(renderer, textX, textY, "ESC > REMOVE SAVE FILE AND EXIT GAME", ConsoleColor.Gray, UiTextScale);
            textY += UiLineSpacing * UiTextScale;

            DrawFancyText.DrawString(renderer, textX, textY, "ENTER > KEEP SAVE FILE AND EXIT GAME", ConsoleColor.Gray, UiTextScale);
        }

        public void SetWinLoss(bool hasWon) => _currentState = hasWon ? GameState.Won : GameState.Lost;
        public void SetPlayerCoins(int newCoinCount) => _coins = newCoinCount;
        public void SetCurrentLevel(int levelIndex) => _currentLevel = levelIndex;

        public int GetLevel() => _currentLevel;
        public int GetCoins() => _coins;
    }
}