using System;
using System.Collections.Generic;

namespace ConsoleTowerDefense
{
    /// <summary>
    /// Represents the configuration for a specific game level.
    /// </summary>
    internal class LevelConfig
    {
        public int Level;
        public int[,] PathData;
        public int[,] TowerPercents;
    }

    /// <summary>
    /// Manages the game world layout, including the rendering and positioning of enemy paths and tower placement spots.
    /// </summary>
    internal class Map
    {
        private readonly List<Coord> _pathWaypoints = new List<Coord>();
        private readonly List<Coord> _towerSpots = new List<Coord>();

        // Preconfigured levels containing waypoint data (X,Y percent) and tower placement spots
        private readonly List<LevelConfig> _levels = new List<LevelConfig>
        {
            new LevelConfig {
                Level = 0,
                PathData = new int[,] {
                    { 0, 20 }, 
                    { 80, 20 },
                    { 80, 95 },
                    { 60, 95 },
                    { 60, 80 }, 
                    { 25, 80 }, 
                    { 25, 93 }, 
                    { 12, 93 }
                },
                TowerPercents = new int[,] {
                    { 30, 28 }, // Top straight path (below)
                    { 50, 12 }, // Top straight path (above)
                    { 72, 88 }, // Right-side loop gap
                    { 69, 50 }, // Middle straight
                    { 33, 88 }  // Bottom-left gap
                }
            }
        };

        private const int PathThickness = 12;
        public bool _mapLoaded = false;

        // tower patch rendering
        private const int TowerSpotSize = 14;
        private const int TowerSpotInnerSize = 12;
        private const int TowerSpotOutlineWidth = 2;

        public Map()
        {
            FileLogger.Instance.Log("");
        }

        /// <summary>
        /// Loads the specified level configuration and processes its waypoints and tower spots.
        /// And extracts data as percentages and maps them to actual screen coordinates.
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="levelIndex"></param>
        public void LoadLevel(ConsoleRenderer renderer, int levelIndex)
        {
            FileLogger.Instance.Log("");
            if (levelIndex >= _levels.Count) return;

            _pathWaypoints.Clear();
            _towerSpots.Clear();
            LevelConfig config = _levels[levelIndex];

            // Process Path Waypoints
            for (int i = 0; i < config.PathData.GetLength(0); i++)
            {
                _pathWaypoints.Add(renderer.GetPosFromPercent(config.PathData[i, 0], config.PathData[i, 1]));
            }

            // Process Towers
            for (int i = 0; i < config.TowerPercents.GetLength(0); i++)
            {
                _towerSpots.Add(renderer.GetPosFromPercent(config.TowerPercents[i, 0], config.TowerPercents[i, 1]));
            }

            _mapLoaded = true;
        }

        /// <summary>
        /// Draws the background, paths, and available tower spots for the currently loaded level
        /// </summary>
        /// <param name="renderer"></param>
        public void Draw(ConsoleRenderer renderer)
        {
            if (_pathWaypoints.Count == 0 || !_mapLoaded) return; // No level loaded, nothing to draw

            // Draw solid grassy colored background
            renderer.DrawRectangle(0, 0, renderer.Width, renderer.Height, ConsoleColor.Green);

            // Draw Path (Connecting Waypoints)
            for (int i = 0; i < _pathWaypoints.Count - 1; i++)
            {
                renderer.DrawLine(
                    _pathWaypoints[i].X, _pathWaypoints[i].Y,
                    _pathWaypoints[i + 1].X, _pathWaypoints[i + 1].Y,
                    PathThickness, ConsoleColor.DarkYellow
                );
            }

            // Draw Tower Placement Slots
            for (int i = 0; i < _towerSpots.Count; i++)
            {
                Coord spot = _towerSpots[i];
                if(i == TowerManager.Instance.GetSelectedTowerIndex())
                {
                    renderer.DrawRectangle(spot.X - (TowerSpotSize + TowerSpotOutlineWidth) / 2, spot.Y - (TowerSpotSize + TowerSpotOutlineWidth) / 2, TowerSpotSize + TowerSpotOutlineWidth, TowerSpotSize + TowerSpotOutlineWidth, ConsoleColor.White); // Draw a highlight selected color as base/outline
                }

                renderer.DrawRectangle(spot.X - TowerSpotSize / 2, spot.Y - TowerSpotSize / 2, TowerSpotSize, TowerSpotSize, ConsoleColor.DarkGray); // Draw a larger dark green square as the base
                renderer.DrawRectangle(spot.X - TowerSpotInnerSize / 2, spot.Y - TowerSpotInnerSize / 2, TowerSpotInnerSize, TowerSpotInnerSize, ConsoleColor.Gray); // Draw a smaller gray square on top to create a border effect
            }
        }

        public List<Coord> GetPathWaypoints() => _pathWaypoints;
        public List<Coord> GetTowerSpots() => _towerSpots;
        public int[] GetSelectedTowerSpot() => new int[] { _towerSpots[TowerManager.Instance.GetSelectedTowerIndex()].X, _towerSpots[TowerManager.Instance.GetSelectedTowerIndex()].Y };
        
        public bool GetMapLoaded() => _mapLoaded;
    }
}