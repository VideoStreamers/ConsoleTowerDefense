using System;

namespace ConsoleTowerDefense
{
    internal class BasicEnemy : Enemy
    {
        private const int SpawnHp = 100;
        private const int MaxHp = 100;
        private const int MoveSpeed = 2;
        private const float AttackSpeed = 1f;
        private const int Damage = 12;
        private const int Reward = 10;

        private static readonly byte[,] BasicSprite = new byte[,]
        {
            { 0, 0, 3, 0, 0 },
            { 0, 1, 1, 1, 0 },
            { 1, 1, 2, 1, 1 },
            { 0, 1, 0, 1, 0 },
            { 0, 1, 0, 1, 0 }
        };

        private static readonly ConsoleColor[] BasicPalette = new ConsoleColor[]
        {
            ConsoleColor.Magenta, ConsoleColor.DarkMagenta, ConsoleColor.Cyan
        };

        /// <summary>
        /// Creates a new Basic Enemy
        /// </summary>
        public BasicEnemy() : base(SpawnHp, MaxHp, MoveSpeed, AttackSpeed, Damage, Reward, BasicSprite, BasicPalette)
        {
        }
    }
}
