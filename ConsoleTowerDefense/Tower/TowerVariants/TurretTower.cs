using System;

namespace ConsoleTowerDefense
{
    internal class TurretTower : Tower
    {
        public TurretTower(Coord position) : base(position)
        {
            _cost = 400;
            _attackRange = 55;
            _attackRate = 0.05f;       // fire rate in seconds
            _projectileSpeed = 55f;
            _projectileDamage = 5;
            _projectileHitRange = 1;   // Splash damage range
            _projectileScale = 1;

            // Tower Rendering
            _towerPalette = new ConsoleColor[] { ConsoleColor.DarkGray, ConsoleColor.Gray, ConsoleColor.DarkYellow };
            _towerSprite = new byte[,]
            {
                { 0, 0, 3, 3, 3, 0, 0 },
                { 0, 0, 0, 3, 0, 0, 0 },
                { 1, 1, 1, 1, 1, 1, 1 },
                { 1, 2, 2, 2, 2, 2, 1 },
                { 1, 1, 2, 2, 2, 1, 1 },
                { 1, 1, 2, 1, 2, 1, 1 },
                { 1, 2, 1, 1, 1, 2, 1 },
                { 1, 2, 1, 1, 1, 2, 1 }
            };

            // Projectile Rendering
            _projectilePalette = new ConsoleColor[] { ConsoleColor.Gray, ConsoleColor.DarkYellow };
            _projectileSprite = new byte[,]
            {
                { 1, 2 }
            };
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
        }
    }
}