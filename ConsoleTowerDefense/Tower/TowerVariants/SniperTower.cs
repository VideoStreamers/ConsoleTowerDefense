using System;

namespace ConsoleTowerDefense
{
    internal class SniperTower : Tower
    {
        public SniperTower(Coord position) : base(position)
        {
            _cost = 200;
            _attackRange = 175;
            _attackRate = 1.5f;       // fire rate in seconds
            _projectileSpeed = 150f;
            _projectileDamage = 150;
            _projectileHitRange = 1;  // Splash damage range
            _projectileScale = 1;

            // Tower Rendering
            _towerPalette = new ConsoleColor[] { ConsoleColor.Black, ConsoleColor.DarkGray, ConsoleColor.Yellow };
            _towerSprite = new byte[,]
            {
                { 0, 0, 0, 3, 0, 0, 0 },
                { 0, 0, 0, 3, 0, 0, 0 },
                { 0, 0, 2, 2, 2, 0, 0 },
                { 0, 0, 2, 2, 2, 0, 0 },
                { 0, 2, 2, 2, 2, 2, 0 },
                { 0, 2, 2, 2, 2, 2, 0 },
                { 0, 2, 2, 2, 2, 2, 0 },
                { 0, 2, 2, 1, 2, 2, 0 },
                { 2, 2, 1, 1, 1, 2, 2 },
                { 2, 2, 1, 1, 1, 2, 2 }
            };

            // Projectile Rendering
            _projectilePalette = new ConsoleColor[] { ConsoleColor.Gray, ConsoleColor.DarkYellow };
            _projectileSprite = new byte[,]
            {
                { 1, 1, 2 }
            };
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
        }

        public override void DrawRange(int dotCount = 16)
        {
            base.DrawRange(32);
        }
    }
}