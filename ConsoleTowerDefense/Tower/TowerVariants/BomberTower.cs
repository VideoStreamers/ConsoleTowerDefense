using System;

namespace ConsoleTowerDefense
{
    internal class BomberTower : Tower
    {
        public BomberTower(Coord position) : base(position)
        {
            _cost = 160;
            _attackRange = 45;
            _attackRate = 4f;       // fire rate in seconds
            _projectileSpeed = 12f;
            _projectileDamage = 35;
            _projectileHitRange = 12;  // Splash damage range
            _projectileScale = 1;

            // Tower Rendering
            _towerPalette = new ConsoleColor[] { ConsoleColor.Black, ConsoleColor.DarkGray, ConsoleColor.DarkYellow };
            _towerSprite = new byte[,]
            {
                { 0, 0, 0, 3, 0, 0, 0 },
                { 0, 0, 2, 2, 2, 0, 0 },
                { 0, 2, 2, 2, 2, 2, 0 },
                { 0, 2, 2, 2, 2, 2, 0 },
                { 2, 2, 2, 1, 2, 2, 2 },
                { 2, 2, 1, 1, 1, 2, 2 },
                { 2, 2, 1, 1, 1, 2, 2 }
            };

            // Projectile Rendering
            _projectilePalette = new ConsoleColor[] { ConsoleColor.DarkGray, ConsoleColor.Black, ConsoleColor.DarkYellow };
            _projectileSprite = new byte[,]
            {
                { 1, 2, 1 },
                { 2, 3, 2 },
                { 1, 2, 1 }
            };
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
        }
    }
}