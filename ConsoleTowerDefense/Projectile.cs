using System;
using System.Collections.Generic;

namespace ConsoleTowerDefense
{
    internal class Projectile
    {
        private float _preciseX;
        private float _preciseY;
        private float _speed;
        private int _damage;
        private float _damageRange; // Used as the explosion/splash radius

        private Enemy _target;
        private bool _targetLost = false;

        // Rendering
        private byte[,] _sprite;
        private ConsoleColor[] _colorPalette;
        private int _scale;

        private float targetX;
        private float targetY;


        /// <summary>
        /// Projectiles are created by towers when they fire, and are responsible for moving towards their target and applying damage upon impact.
        /// </summary>
        /// <param name="towerPositionX"></param>
        /// <param name="towerPositionY"></param>
        /// <param name="speed"></param>
        /// <param name="damage"></param>
        /// <param name="damageRange"></param>
        /// <param name="sprite"></param>
        /// <param name="target"></param>
        /// <param name="colorPalette"></param>
        /// <param name="projectileScale"></param>
        public Projectile(float towerPositionX, float towerPositionY, float speed, int damage, float damageRange, byte[,] sprite, Enemy target, ConsoleColor[] colorPalette, int projectileScale)
        {
            _preciseX = towerPositionX;
            _preciseY = towerPositionY;
            _speed = speed;
            _damage = damage;
            _damageRange = damageRange;
            _sprite = sprite;
            _target = target;
            _scale = projectileScale;
            _colorPalette = colorPalette;

            if (_target != null)
            {
                targetX = _target.GetPositionX();
                targetY = _target.GetPositionY();
            }
        }

        /// <summary>
        /// Updates the projectile's position each frame, moving it towards its target.
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Update(float deltaTime)
        {
            if (!_targetLost)
            {
                if (_target == null || !EnemyManager.Instance.GetActiveEnemies().Contains(_target))
                {
                    _targetLost = true;
                }
            }

            LerpPosition(deltaTime);
        }

        /// <summary>
        /// Smoothly interpolates the projectile's position towards its target. If the target is lost, it continues towards the last known position.
        /// </summary>
        /// <param name="deltaTime"></param>
        private void LerpPosition(float deltaTime)
        {
            // Dynamic tracking update if the enemy is still around
            if (!_targetLost && _target != null)
            {
                targetX = _target.GetPositionX();
                targetY = _target.GetPositionY();
            }

            float deltaX = targetX - _preciseX;
            float deltaY = targetY - _preciseY;
            float distance = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
            float moveDistance = _speed * deltaTime;

            if (distance <= moveDistance) // Impact reached
            {
                _preciseX = targetX;
                _preciseY = targetY;

                Explode();
            }
            else // Move towards the target
            {
                _preciseX += (deltaX / distance) * moveDistance;
                _preciseY += (deltaY / distance) * moveDistance;
            }
        }

        private void Explode()
        {
            // Grab all enemies currently alive on the map
            List<Enemy> activeEnemies = EnemyManager.Instance.GetActiveEnemies();

            // Check every enemy to see if they are within the blast radius
            for (int i = 0; i < activeEnemies.Count; i++)
            {
                Enemy enemy = activeEnemies[i];

                float deltaX = enemy.GetPositionX() - _preciseX;
                float deltaY = enemy.GetPositionY() - _preciseY;
                float distance = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

                // If the enemy is inside the splash zone, they take damage
                if (distance <= _damageRange)
                {
                    enemy.TakeDamage(_damage);
                }
            }

            // Remove from game loop
            TowerManager.Instance.DestroyProjectile(this);
        }

        /// <summary>
        /// Renders the projectile at its current position.
        /// </summary>
        public void Draw()
        {
            ConsoleRenderer renderer = ConsoleRenderer.Instance;
            renderer.DrawIndexedSprite((int)_preciseX, (int)_preciseY, _sprite, _colorPalette, _scale, SpriteAlignment.Center);
        }
    }
}