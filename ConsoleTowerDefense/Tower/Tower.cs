using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTowerDefense
{
    abstract internal class Tower
    {
        // Tower Stats
        protected int _cost;
        protected int _attackRange;
        protected float _attackRate;

        // Tower Rendering
        protected byte[,] _towerSprite;
        protected Coord _currentPosition;
        protected ConsoleColor[] _towerPalette;
        private int _towerScale = 2;

        // Projectile
        protected float _projectileSpeed;
        protected int _projectileDamage;
        protected int _projectileHitRange;

        // Projectile Rendering
        protected byte[,] _projectileSprite;
        protected int _projectileScale;
        protected ConsoleColor[] _projectilePalette;

        private readonly float _lastUpdateTime = 0;
        private readonly float _deltaTime = 0;
        private float _cooldownTimer = 0f;

        /// <summary>
        /// Tower is the base class for all tower types.
        /// </summary>
        /// <param name="position"></param>
        public Tower(Coord position)
        {
            FileLogger.Instance.Log("");
            _currentPosition = position;
        }

        /// <summary>
        /// Update handles the logic execution of the towers.
        /// </summary>
        /// <param name="deltaTime"></param>
        public virtual void Update(float deltaTime)
        {
            if (_cooldownTimer > 0)
            {
                _cooldownTimer -= deltaTime;
            }

            // Only look for a target if weapon is ready
            if (_cooldownTimer <= 0)
            {
                Enemy target = FindTarget();
                if (target != null)
                {
                    Fire(target);
                    _cooldownTimer = _attackRate; // Reset cooldown
                }
            }
        }

        /// <summary>
        /// Finds the first enemy within attack range.
        /// </summary>
        /// <returns>Returns Enemy or null if no enemy is found</returns>
        private Enemy FindTarget()
        {
            List<Enemy> activeEnemies = EnemyManager.Instance.GetActiveEnemies();

            // Loop through all active enemies and check if there are any within attack range. If there are multiple, the first one found will be targeted.
            foreach (Enemy enemy in activeEnemies)
            {
                float deltaX = enemy.GetPositionX() - _currentPosition.X;
                float deltaY = enemy.GetPositionY() - _currentPosition.Y;
                float distance = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

                if (distance <= _attackRange)
                {
                    return enemy;
                }
            }
            return null;
        }

        /// <summary>
        /// Fires a projectile towards the target enemy.
        /// </summary>
        /// <param name="target"></param>
        public void Fire(Enemy target)
        {
            // Instantiate a projectile and hand it over to a projectile tracking list
            Projectile projectile = new Projectile(
                _currentPosition.X, _currentPosition.Y,
                _projectileSpeed, _projectileDamage, _projectileHitRange,
                _projectileSprite, target, _projectilePalette, _projectileScale
            );

            TowerManager.Instance.AddProjectile(projectile);
        }

        /// <summary>
        /// Renders the tower on the console using the ConsoleRenderer.
        /// </summary>
        public void Draw()
        {
            ConsoleRenderer renderer = ConsoleRenderer.Instance;
            renderer.DrawIndexedSprite(_currentPosition.X, _currentPosition.Y, _towerSprite, _towerPalette, _towerScale, SpriteAlignment.Center);
        }

        /// <summary>
        /// Render the attack range of the tower as a circle of dots around the tower.
        /// </summary>
        /// <param name="dotCount"></param>
        public virtual void DrawRange(int dotCount = 16)
        {
            ConsoleRenderer renderer = ConsoleRenderer.Instance;

            // Loop to draw dots around the tower in a circle pattern.
            for (int i = 0; i < dotCount; i++)
            {
                // Calculate the angle for this specific dot around the circle (in radians)
                float angle = i * (2f * (float)Math.PI / dotCount);
        
                // Trigonometry math to find the precise edge point:
                int dotX = (int)(_currentPosition.X + Math.Cos(angle) * _attackRange);
                int dotY = (int)(_currentPosition.Y + Math.Sin(angle) * _attackRange);

                // Don't try to draw outside the console screen boundaries
                if (dotX >= 0 && dotX < Console.WindowWidth && dotY >= 0 && dotY < Console.WindowHeight)
                {
                    // Draw a single character at the edge coordinate. 
                    renderer.DrawIndexedSprite(dotX, dotY, new byte[,] { { 1, 1 }, { 1, 1 } }, new ConsoleColor[] { ConsoleColor.Red });
                }
            }
        }

        public int GetCost() => _cost;

        public Coord GetPosition() => _currentPosition;
    }
}
