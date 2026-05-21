using System;
using System.Collections.Generic;

namespace ConsoleTowerDefense
{
    internal class WaveManager
    {
        private int _currentWave = 0;

        public static WaveManager Instance;
        private List<Wave> _waves;

        private float _gameTime = 0f;
        private bool _doRecordGameTime = false;

        // Gradual spawn tracking variables
        private List<EnemyType> _pendingSpawnList = new List<EnemyType>();
        private float _spawnTimer = 0f;
        private float _timeBetweenSpawns = 0f;
        private bool _isSpawningActive = false;

        // Tracks how long the current wave has been active on the field
        private float _waveActiveTime = 0f;

        private enum EnemyType { Basic, Fast, Tank }

        public WaveManager()
        {
            FileLogger.Instance.Log("");
            if (Instance != null)
            {
                throw new Exception("WaveManager instance already exists!");
            }
            Instance = this;

            _waves = new List<Wave>();
        }

        /// <summary>
        /// Initializes the spawning sequence for a given wave index, setting up timers and staging arrays based on the wave's configuration.
        /// </summary>
        /// <param name="waveIndex"></param>
        private void StartWaveSpawning(int waveIndex)
        {
            if (waveIndex < 0 || waveIndex >= _waves.Count) return;

            Wave wave = _waves[waveIndex];

            wave.IsActive = true;
            _isSpawningActive = true;
            _pendingSpawnList.Clear();
            _waveActiveTime = 0f; // Reset individual runtime tracker

            float waveStartSeconds = wave.GetStartTime() / 1000f;
            if (_gameTime < waveStartSeconds)
            {
                _gameTime = waveStartSeconds;
            }

            FileLogger.Instance.Log($"Initializing gradual spawn sequence for Wave {waveIndex}.");

            XmlHandler.Instance.WritePlayerSaveData(
                GameManager.Instance.GetLevel(),
                waveIndex,
                GameManager.Instance.CastleObj.GetCurrentHp(),
                TowerManager.Instance.GetActiveTowers(),
                GameManager.Instance.GetCoins()
            );

            // Populate staging array
            for (int i = 0; i < wave.GetBasicCount(); i++) _pendingSpawnList.Add(EnemyType.Basic);
            for (int i = 0; i < wave.GetFastCount(); i++) _pendingSpawnList.Add(EnemyType.Fast);
            for (int i = 0; i < wave.GetTankCount(); i++) _pendingSpawnList.Add(EnemyType.Tank);

            // Calculate spawn intervals based on total duration and enemy count
            float spawnDurationSeconds = wave.GetSpawnDuration() / 1000f;
            int totalEnemies = _pendingSpawnList.Count;

            if (totalEnemies > 1)
            {
                _timeBetweenSpawns = spawnDurationSeconds / (totalEnemies - 1);
            }
            else
            {
                _timeBetweenSpawns = spawnDurationSeconds;
            }

            // Prime the timer to match the interval instantly, ensuring the first unit drops on frame 1
            _spawnTimer = _timeBetweenSpawns;
        }

        /// <summary>
        /// Manually advances to the next wave, checking if the current wave is still active. 
        /// If it is, it forces the transition, if not, it starts the next wave.
        /// </summary>
        public void NextWave()
        {
            FileLogger.Instance.Log("");
            if (_currentWave >= _waves.Count) return;

            if (!_waves[_currentWave].IsActive)
            {
                StartWaveSpawning(_currentWave);
            }
            else
            {
                _currentWave++;
                if (_currentWave < _waves.Count)
                {
                    StartWaveSpawning(_currentWave);
                }
                else
                {
                    GameManager.Instance.SetWinLoss(true);
                }
            }
        }

        /// <summary>
        /// Updates wave related timers and state transitions
        /// </summary>
        /// <param name="deltaTime"></param>
        public void UpdateWaves(float deltaTime)
        {
            if (!_doRecordGameTime) return;

            _gameTime += deltaTime;

            if (_currentWave >= _waves.Count) return;

            Wave activeWave = _waves[_currentWave];

            // start spawning the current wave's enemies
            if (_isSpawningActive && _pendingSpawnList.Count > 0)
            {
                _spawnTimer += deltaTime;

                if (_spawnTimer >= _timeBetweenSpawns)
                {
                    _spawnTimer = 0f; // Reset step interval slice

                    EnemyType nextEnemy = _pendingSpawnList[0];
                    _pendingSpawnList.RemoveAt(0); // Dequeue the next enemy type

                    switch (nextEnemy)
                    {
                        case EnemyType.Basic: 
                            EnemyManager.Instance.AddEnemy(new BasicEnemy()); 
                            break;

                        case 
                            EnemyType.Fast: EnemyManager.Instance.AddEnemy(new FastEnemy()); 
                            break;

                        case EnemyType.Tank: 
                            EnemyManager.Instance.AddEnemy(new TankEnemy()); 
                            break;
                    }
                }

                if (_pendingSpawnList.Count == 0)
                {
                    _isSpawningActive = false;
                }
            }

            // clear the wave when all enemies are dead and the spawn duration has elapsed
            if (activeWave.IsActive)
            {
                _waveActiveTime += deltaTime; // Update independent active tracking frame slice

                bool waveDurationElapsed = _waveActiveTime >= (activeWave.GetSpawnDuration() / 1000f);

                if (waveDurationElapsed && !_isSpawningActive && EnemyManager.Instance.GetActiveEnemyCount() == 0)
                {
                    FileLogger.Instance.Log($"Wave {_currentWave} cleared automatically. Advancing state loop.");
                    _currentWave++;
                    _isSpawningActive = false;

                    // Trigger the win scenario if all waves are depleted.
                    if (_currentWave >= _waves.Count)
                    {
                        GameManager.Instance.SetWinLoss(true);
                    }
                }
            }
            // If the wave isn't active yet, check if the start time has been reached to trigger it
            else
            {
                if (_gameTime >= (activeWave.GetStartTime() / 1000f))
                {
                    FileLogger.Instance.Log($"Wave {_currentWave} automated activation threshold met.");
                    StartWaveSpawning(_currentWave);
                }
            }
        }

        public void StartWaveTimer() => _doRecordGameTime = true;

        public void AddWave(Wave newWave) => _waves.Add(newWave);

        public void SetCurrentWave(int waveIndex) => _currentWave = waveIndex;

        public int GetCurrentWave() => _currentWave;
    }
}