using System;

namespace ConsoleTowerDefense
{
    internal class TankEnemy : Enemy
    {
        private const int SpawnHp = 350;
        private const int MaxHp = 350;
        private const int MoveSpeed = 1;
        private const float AttackSpeed = 0.5f;
        private const int Damage = 25;
        private const int Reward = 30;

        private static readonly byte[,] TankSprite = new byte[,]
        {
            { 0, 1, 1, 1, 0 },
            { 0, 3, 2, 3, 0 },
            { 1, 2, 2, 2, 1 },
            { 2, 2, 2, 2, 2 },
            { 2, 2, 2, 2, 2 },
            { 0, 1, 0, 1, 0 }
        };

        private static readonly ConsoleColor[] TankPalette = new ConsoleColor[]
        {
            ConsoleColor.Red, ConsoleColor.DarkRed, ConsoleColor.DarkMagenta
        };

        /// <summary>
        /// Creates a new Tank Enemy
        /// </summary>
        public TankEnemy() : base(SpawnHp, MaxHp, MoveSpeed, AttackSpeed, Damage, Reward, TankSprite, TankPalette)
        {
        }
    }
}