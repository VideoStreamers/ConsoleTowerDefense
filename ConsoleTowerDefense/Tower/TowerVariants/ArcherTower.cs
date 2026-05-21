using System;

namespace ConsoleTowerDefense
{
    internal class ArcherTower : Tower
    {
        public ArcherTower(Coord position) : base(position)
        {
            _cost = 80;
            _attackRange = 45;
            _attackRate = 0.75f;       // fire rate in seconds
            _projectileSpeed = 25f;
            _projectileDamage = 15;
            _projectileHitRange = 1;  // Splash damage range
            _projectileScale = 1;

            // Tower Rendering
            _towerPalette = new ConsoleColor[] { ConsoleColor.DarkGray, ConsoleColor.Gray, ConsoleColor.Yellow };
            _towerSprite = new byte[,]
            {
                { 1, 0, 0, 3, 0, 0, 1 },
                { 1, 1, 1, 1, 1, 1, 1 },
                { 1, 2, 2, 2, 2, 2, 1 },
                { 1, 2, 2, 2, 2, 2, 1 },
                { 1, 2, 2, 1, 2, 2, 1 },
                { 1, 2, 1, 1, 1, 2, 1 },
                { 1, 2, 1, 1, 1, 2, 1 }
            };

            // Projectile Rendering
            _projectilePalette = new ConsoleColor[] { ConsoleColor.White, ConsoleColor.Gray, ConsoleColor.DarkGray };
            _projectileSprite = new byte[,]
            {
                { 3, 1, 2 }
            };
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
        }
    }
}