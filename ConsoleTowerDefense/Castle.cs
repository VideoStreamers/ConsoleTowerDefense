using System;
using System.Collections.Generic;

namespace ConsoleTowerDefense
{
    internal enum CastleState { Pristine, Cracked, Crumbling }

    internal class Castle
    {
        public static Castle Instance;
        private int _maxHp;
        private int _currentHp;
        private const int Scale = 3;
        private CastleState currentState = CastleState.Pristine;

        private int currentLevel;

        // castleSprites indexed by [damageLevel, y, x]
        // Level 0: Pristine | Level 1: Cracked | Level 2: Crumbling
        private readonly Dictionary<CastleState, byte[,]> _sprites = new Dictionary<CastleState, byte[,]>{
            { CastleState.Pristine, new byte[,] {
                { 0, 0, 0, 0, 2, 0, 0, 0, 0 },
                { 0, 0, 0, 2, 3, 2, 0, 0, 0 },
                { 0, 0, 1, 2, 2, 2, 1, 0, 0 },
                { 0, 1, 2, 2, 2, 2, 2, 1, 0 },
                { 0, 1, 2, 2, 3, 2, 2, 1, 0 },
                { 0, 1, 1, 3, 3, 3, 1, 1, 0 },
                { 0, 1, 2, 3, 3, 3, 2, 1, 0 },
                { 0, 1, 2, 3, 3, 3, 2, 1, 0 },
                { 1, 1, 1, 1, 1, 1, 1, 1, 1 }
            }},
            { CastleState.Cracked, new byte[,] {
                { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 2, 2, 0, 0, 0, 0 },
                { 0, 0, 0, 2, 2, 1, 0, 0, 0 },
                { 0, 0, 1, 2, 2, 2, 1, 1, 0 },
                { 0, 0, 1, 2, 3, 2, 2, 1, 0 },
                { 0, 0, 1, 3, 3, 3, 2, 1, 0 },
                { 0, 0, 1, 3, 3, 3, 2, 1, 0 },
                { 0, 1, 2, 3, 3, 3, 2, 1, 2 },
                { 0, 1, 1, 1, 1, 1, 1, 1, 1 }
            }},
            { CastleState.Crumbling, new byte[,] {
                { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 1, 1, 0, 0, 0 },
                { 0, 0, 0, 1, 2, 2, 1, 0, 0 },
                { 0, 0, 0, 1, 2, 2, 2, 0, 0 },
                { 0, 0, 1, 2, 1, 3, 2, 1, 0 },
                { 0, 0, 1, 2, 3, 3, 2, 1, 2 },
                { 1, 1, 2, 1, 3, 3, 2, 1, 2 },
                { 1, 2, 1, 1, 1, 1, 1, 1, 1 }
            }}
        };

        private readonly ConsoleColor[] _castleColors = { ConsoleColor.Gray, ConsoleColor.DarkGray, ConsoleColor.Black, ConsoleColor.White };

        private readonly int[,] _levelPositions = new int[,] {
            { 1, 99 }, // level 1
            // more positions if more levels/maps are added
        };

        /// <summary>
        /// Initializes the castle's stats.
        /// </summary>
        /// <param name="startHp"></param>
        /// <param name="maxHp"></param>
        public Castle(int startHp, int maxHp)
        {
            FileLogger.Instance.Log("");

            if (Instance != null)
            {
                throw new Exception("Castle instance already exists!");
            }
            Instance = this;

            _currentHp = startHp;
            _maxHp = maxHp;

            currentLevel = GameManager.Instance.GetLevel();
        }

        /// <summary>
        /// Reduces the castle's HP by the damage amount and updates the castle's state accordingly. 
        /// If HP drops to 0 or below, triggers a loss condition.
        /// </summary>
        /// <param name="damage"></param>
        public void TakeDamage(int damage)
        {
            FileLogger.Instance.Log("");
            _currentHp -= damage;
            if (_currentHp <= 0)
            {
                GameManager.Instance.SetWinLoss(false);
            }

            if (_currentHp < _maxHp * 0.3)
            {
                currentState = CastleState.Crumbling;
            }
            else if (_currentHp < _maxHp * 0.7)
            {
                currentState = CastleState.Cracked;
            }
            else
            {
                currentState = CastleState.Pristine;
            }
        }

        /// <summary>
        /// Draws the castle on the console using the ConsoleRenderer.
        /// </summary>
        /// <param name="renderer"></param>
        public void Draw(ConsoleRenderer renderer)
        {
            Coord screenPos = renderer.GetPosFromPercent(_levelPositions[currentLevel, 0], _levelPositions[currentLevel, 1]);

            renderer.DrawIndexedSprite(screenPos.X, screenPos.Y, _sprites[currentState], _castleColors, Scale, SpriteAlignment.BottomLeft);
        }

        public void SetCurrentHp(int hp) => _currentHp = hp;
        public int GetCurrentHp() => _currentHp;
    }
}
