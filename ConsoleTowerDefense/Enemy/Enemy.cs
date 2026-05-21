using System;
using System.Collections.Generic;

namespace ConsoleTowerDefense
{
    abstract class Enemy
    {
        // Constants
        private const float EnemyMoveSpeedMultiplier = 2.5f;
        private const float WaypointReachedTreshold = 2.0f;

        // Enemy stats
        protected int _currentHp;
        protected int _maxHp;
        protected int _moveSpeed;
        protected float _attackSpeed;
        protected int _attackDamage;
        protected int _rewardValue;

        // Positioning
        protected float _preciseX;
        protected float _preciseY;

        // Drawing
        protected byte[,] _sprite;
        protected int _scale = 1;
        protected ConsoleColor[] _myPalette;
        
        // Pathing
        protected int _currentWaypointIndex = 0;
        protected Coord _pathOffset;

        // Hit feedback
        private bool _isHitThisFrame = false;
        private int _lastUpdateHp;

        public Enemy(
            int spawnHp,
            int maxHp,
            int moveSpeed,
            float attackSpeed,
            int attackDamage,
            int rewardValue,
            byte[,] sprite,
            ConsoleColor[] colorPalette
        )
        {
            FileLogger.Instance.Log("");

            _currentHp = spawnHp;
            _maxHp = maxHp;
            _moveSpeed = moveSpeed;
            _attackSpeed = attackSpeed;
            _attackDamage = attackDamage;
            _rewardValue = rewardValue;

            _sprite = sprite;
            _myPalette = colorPalette;

            // Initialize position to the first waypoint of the path
            List<Coord> waypoints = GameManager.Instance.MapObj.GetPathWaypoints();
            if (waypoints != null && waypoints.Count > 0)
            {
                _preciseX = waypoints[0].X;
                _preciseY = waypoints[0].Y;
            }
            else
            {
                _preciseX = 0;
                _preciseY = 0;
            }

            // Generate a random offset for this enemy to make movement look more natural
            Random rand = new Random(Guid.NewGuid().GetHashCode());
            _pathOffset = new Coord();
            _pathOffset.X = (short)rand.Next(-4, 4);
            _pathOffset.Y = (short)rand.Next(-5, 5);

            _lastUpdateHp = _currentHp;
        }

        /// <summary>
        /// Updates the enemy's position.
        /// </summary>
        public void Update(float deltaTime)
        {
            if(_currentHp < _lastUpdateHp)
            {
                _isHitThisFrame = true;
            }
            else
            {
                _isHitThisFrame = false;
            }

            Move(deltaTime);
            _lastUpdateHp = _currentHp;
        }

        /// <summary>
        /// Moves the enemy towards the next waypoint. If the enemy reaches the end of the path, it attacks the base and dies.
        /// </summary>
        private void Move(float deltaTime)
        {
            List<Coord> waypoints = GameManager.Instance.MapObj.GetPathWaypoints();
            if (waypoints.Count == 0 || _currentWaypointIndex >= waypoints.Count) return;

            float targetX = waypoints[_currentWaypointIndex].X + _pathOffset.X;
            float targetY = waypoints[_currentWaypointIndex].Y + _pathOffset.Y;

            float dirX = targetX - _preciseX;
            float dirY = targetY - _preciseY;
            float distance = (float)Math.Sqrt(dirX * dirX + dirY * dirY);

            if (distance < WaypointReachedTreshold)
            {
                _currentWaypointIndex++;
                if (_currentWaypointIndex >= waypoints.Count)
                {
                    AttackBase();
                }
                return;
            }

            _preciseX += (dirX / distance) * _moveSpeed * deltaTime * EnemyMoveSpeedMultiplier;
            _preciseY += (dirY / distance) * _moveSpeed * deltaTime * EnemyMoveSpeedMultiplier;
        }

        /// <summary>
        /// Attacks the base, dealing damage to the castle's HP.
        /// </summary>
        public void AttackBase()
        {
            FileLogger.Instance.Log("");
            Castle.Instance.TakeDamage(_attackDamage);
        }

        /// <summary>
        /// Applies damage to the enemy. If HP drops to 0 or below, the enemy dies.
        /// </summary>
        /// <param name="damage"></param>
        public void TakeDamage(int damage)
        {
            FileLogger.Instance.Log("");

            _currentHp -= damage;
        }

        /// <summary>
        /// Draws the enemy on the console at its current position. If the enemy was hit this frame, it flashes white.
        /// </summary>
        public void Draw()
        {
            ConsoleRenderer renderer = ConsoleRenderer.Instance;

            ConsoleColor[] renderPalette;

            if (_isHitThisFrame)
            {
                // flash white when hit
                renderPalette = new ConsoleColor[] { ConsoleColor.White, ConsoleColor.White, ConsoleColor.White };
            }
            else
            {
                // use normal palette
                renderPalette = _myPalette;
            }

            renderer.DrawIndexedSprite((int)_preciseX, (int)_preciseY, _sprite, renderPalette, _scale, SpriteAlignment.Center);
        }

        public int GetCurrentHp() => _currentHp;
        public float GetPositionX() => _preciseX;
        public float GetPositionY() => _preciseY;
        public int GetRewardValue() => _rewardValue;
    }
}
