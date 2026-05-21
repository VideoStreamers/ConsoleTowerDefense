using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTowerDefense
{
    internal class TowerManager
    {
        public static TowerManager Instance;

        private List<Tower> _towers;
        private List<Projectile> _activeProjectiles;

        private int _selectedTowerIndex = 0;

        /// <summary>
        /// TowerManager manages all towers in the game.
        /// </summary>
        public TowerManager()
        {
            FileLogger.Instance.Log("");
            if (Instance != null)
            {
                throw new Exception("WaveManager instance already exists!");
            }
            Instance = this;

            _towers = new List<Tower>();
            _activeProjectiles = new List<Projectile>();
        }

        /// <summary>
        /// Creates a tower of the specified type at the given position.
        /// </summary>
        /// <param name="type">"ArcherTower", "BomberTower", "TurretTower", "SniperTower"</param>
        /// <param name="xPosition"></param>
        /// <param name="yPosition"></param>
        /// <returns>Tower Object</returns>
        public Tower CreateTowerFromType(string type, int xPosition, int yPosition)
        {
            Coord positions;
            positions.X = (short)xPosition;
            positions.Y = (short)yPosition;

            switch (type)
            {
                case "ArcherTower":
                    return new ArcherTower(positions);
                case "BomberTower":
                    return new BomberTower(positions);
                case "TurretTower":
                    return new TurretTower(positions);
                case "SniperTower":
                    return new SniperTower(positions);
                default:
                    throw new Exception($"Unknown tower type: {type}");
            }
        }


        /// <summary>
        /// Adds the given tower to the list of active towers
        /// </summary>
        /// <param name="tower"></param>
        /// <param name="doDeductCoins">If true, deducts the cost of the tower from the player's coins.</param>
        public void AddTower(Tower tower, bool doDeductCoins = true)
        {
            FileLogger.Instance.Log("");

            if (IsSpotOccupied(tower.GetPosition()))
            {
                return;
            }

            if (doDeductCoins)
            {
                if (!GameManager.Instance.ChangeCoins(-tower.GetCost())) 
                { 
                    return; 
                }
            }

            _towers.Add(tower);
        }

        /// <summary>
        /// Handles updating all towers and projectiles each frame.
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Update(float deltaTime)
        {
            for (int i = 0; i < _towers.Count; i++)
            {
                _towers[i].Update(deltaTime);
            }
            for (int i = 0; i < _activeProjectiles.Count; i++)
            {
                _activeProjectiles[i].Update(deltaTime);
            }
        }

        /// <summary>
        /// Handles drawing all towers and projectiles each frame.
        /// </summary>
        public void Draw()
        {
            DrawTowers();
            DrawProjectiles();
        }

        /// <summary>
        /// Draws all towers and their ranges.
        /// </summary>
        private void DrawTowers()
        {
            for (int i = 0; i < _towers.Count; i++)
            {
                if (i >= _towers.Count) break;

                Tower tower = _towers[i];

                if(tower != null)
                {
                    tower.DrawRange();
                    tower.Draw();
                }
            }
        }

        /// <summary>
        /// Changes the index of the currently selected tower.
        /// </summary>
        /// <param name="doCountUp"></param>
        public void ChangeSelectedTowerIndex(bool doCountUp)
        {
            if (doCountUp)
            {
                _selectedTowerIndex++;
                if (_selectedTowerIndex >= GameManager.Instance.MapObj.GetTowerSpots().Count)
                {
                    _selectedTowerIndex = 0;
                }
            }
            else
            {
                _selectedTowerIndex--;
                if (_selectedTowerIndex < 0)
                {
                    _selectedTowerIndex = _towers.Count - 1;
                }
            }
        }

        /// <summary>
        /// Adds the given projectile to the list of active projectiles.
        /// </summary>
        /// <param name="projectile"></param>
        public void AddProjectile(Projectile projectile)
        {
            _activeProjectiles.Add(projectile);
        }

        /// <summary>
        /// Removes the given projectile from the list of active projectiles, effectively destroying it.
        /// </summary>
        /// <param name="projectile"></param>
        public void DestroyProjectile(Projectile projectile)
        {
            _activeProjectiles.Remove(projectile);
        }

        /// <summary>
        /// Draws all active projectiles.
        /// </summary>
        private void DrawProjectiles()
        {
            for (int i = 0; i < _activeProjectiles.Count; i++)
            {
                if (i >= _activeProjectiles.Count) break;

                Projectile projectile = _activeProjectiles[i];

                if (projectile != null)
                {
                    projectile.Draw();
                }
            }
        }

        /// <summary>
        /// Sells the tower at the given position. Player receives a refund equal to the tower's cost.
        /// </summary>
        /// <param name="xPosition"></param>
        /// <param name="yPosition"></param>
        public void SellTowerAtPosition(int xPosition, int yPosition)
        {
            FileLogger.Instance.Log("");

            // Loop through towers to find the one at the given position
            for (int i = 0; i < _towers.Count; i++)
            {
                if (_towers[i].GetPosition().X == xPosition && _towers[i].GetPosition().Y == yPosition)
                {
                    GameManager.Instance.ChangeCoins(_towers[i].GetCost());
                    _towers.RemoveAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// Returns the cost of the tower type specified by the string parameter.
        /// </summary>
        /// <param name="towerType">"ArcherTower", "BomberTower", "TurretTower", "SniperTower"</param>
        /// <returns>Cost as integer</returns>
        public int GetTowerCost(string towerType)
        {
            switch(towerType)
            {
                case "ArcherTower":
                    return new ArcherTower(new Coord { }).GetCost();
                case "BomberTower":
                    return new BomberTower(new Coord { }).GetCost();
                case "TurretTower":
                    return new TurretTower(new Coord { }).GetCost();
                case "SniperTower":
                    return new SniperTower(new Coord { }).GetCost();
                default:
                    throw new Exception($"Unknown tower type: {towerType}");
            }
        }

        /// <summary>
        /// Checks if there is already a tower at the given position.
        /// Returns true if there is a tower, false otherwise.
        /// </summary>
        /// <param name="position"></param>
        private bool IsSpotOccupied(Coord position)
        {
            FileLogger.Instance.Log("");
            for (int i = 0; i < _towers.Count; i++)
            {
                if (_towers[i].GetPosition().Equals(position))
                {
                    return true; // Spot already occupied
                }
            }
            return false; // No tower at that location
        }

        public List<Tower> GetActiveTowers() => _towers;

        public int GetSelectedTowerIndex() => _selectedTowerIndex;
    }
}
