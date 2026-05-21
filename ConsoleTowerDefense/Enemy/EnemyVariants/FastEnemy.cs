using System;

namespace ConsoleTowerDefense
{
    internal class FastEnemy : Enemy
    {
        private const int SpawnHp = 75;
        private const int MaxHp = 75;
        private const int MoveSpeed = 4;
        private const float AttackSpeed = 1.25f;
        private const int Damage = 7;
        private const int Reward = 15;

        private static readonly byte[,] FastSprite = new byte[,]
        {
            { 0, 0, 1, 1, 0, 2, 0 },
            { 1, 1, 1, 1, 1, 2, 2 },
            { 0, 1, 0, 0, 1, 0, 0 }
        };

        private static readonly ConsoleColor[] FastPalette = new ConsoleColor[]
        {
            ConsoleColor.Blue, ConsoleColor.DarkBlue, ConsoleColor.DarkCyan
        };

        /// <summary>
        /// Creates a new Fast Enemy
        /// </summary>
        public FastEnemy() : base(SpawnHp, MaxHp, MoveSpeed, AttackSpeed, Damage, Reward, FastSprite, FastPalette)
        {
        }
    }
}