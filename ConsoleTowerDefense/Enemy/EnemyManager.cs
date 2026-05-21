using System;
using System.Collections.Generic;

namespace ConsoleTowerDefense
{
    internal class EnemyManager
    {
        public static EnemyManager Instance;

        private List<Enemy> _enemies;

        /// <summary>
        /// Creates a new EnemyManager instance.
        /// </summary>
        public EnemyManager()
        {
            FileLogger.Instance.Log("");
            if (Instance != null)
            {
                throw new Exception("WaveManager instance already exists!");
            }
            Instance = this;

            _enemies = new List<Enemy>();
        }

        /// <summary>
        /// Adds an enemy to the manager's list of active enemies.
        /// </summary>
        /// <param name="enemy"></param>
        public void AddEnemy(Enemy enemy)
        {
            FileLogger.Instance.Log("");
            _enemies.Add(enemy);
        }

        /// <summary>
        /// Updates all active enemies and removes any that have been killed.
        /// </summary>
        /// <param name="deltaTime"></param>
        public void UpdateEnemies(float deltaTime)
        {
            foreach (Enemy enemy in _enemies)
            {
                enemy.Update(deltaTime);
            }

            // Loop backwards to avoid out-of-range issues when removing enemies
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                if (_enemies[i].GetCurrentHp() <= 0)
                {
                    GameManager.Instance.ChangeCoins(_enemies[i].GetRewardValue()); // Reward player for killing enemy
                    _enemies.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Draws all active enemies to the console.
        /// </summary>
        public void DrawEnemies()
        {
            for (int i = 0; i < _enemies.Count; i++)
            {
                Enemy enemy = _enemies[i];

                if (enemy != null)
                {
                    enemy.Draw();
                }
            }
        }

        /// <summary>
        /// Damages all active enemies by the specified amount.
        /// </summary>
        /// <param name="damage">Damage Amount</param>
        public void DamageAllEnemies(int damage)
        {
            for(int i = 0; i < _enemies.Count; i++)
            {
                Enemy enemy = _enemies[i];
                enemy.TakeDamage(damage);
            }
        }

        public int GetActiveEnemyCount() => _enemies.Count;
        public List<Enemy> GetActiveEnemies() => _enemies;
    }
}
