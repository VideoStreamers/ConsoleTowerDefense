using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ConsoleTowerDefense
{
    // Containers for storing temporary variables
    public struct TempWaveData
    {
        public int StartTime;
        public int SpawnDuration;
        public int BasicCount;
        public int FastCount;
        public int TankCount;
    }

    public struct TempTowerData
    {
        public string type;
        public int xPosition;
        public int yPosition;
    }

    internal class XmlHandler
    {
        public static XmlHandler Instance;

        // Configuration variables
        private int _startTime;
        private int _spawnDuration;
        private int _basicCount;
        private int _fastCount;
        private int _tankCount;
        private int _readWaveIndex = -1;

        private readonly string _filePathWave;
        private readonly string _filePathSaveGame;

        private int _playerDataCurrentLevel;
        private int _playerDataCurrentWave;
        private int _playerDataCastleHp;
        private int _playerDataCoins;

        private List<TempWaveData> _parsedWaves = new List<TempWaveData>();
        private List<Tower> _savedTowers = new List<Tower>();

        /// <summary>
        /// XmlHandler is responsible for reading the wave configuration XML file and the player save data XML file, and applying that data to the game state. 
        /// It also provides functionality to write player progress back to an XML file when saving, and to delete the save file when needed.
        /// </summary>
        /// <param name="waveFileName"></param>
        /// <param name="saveGameFileName"></param>
        /// <param name="folderPath"></param>
        /// <exception cref="Exception"></exception>
        public XmlHandler(string waveFileName, string saveGameFileName, string folderPath)
        {
            FileLogger.Instance.Log("");
            if (Instance != null)
            {
                throw new Exception("XmlHandler instance already exists!");
            }
            Instance = this;

            _filePathWave = Path.Combine(folderPath, waveFileName + ".xml");

            ParseWaveXmlFile();
            ApplyWaveDataToGame();

            _filePathSaveGame = Path.Combine(folderPath, saveGameFileName + ".xml");

            if (File.Exists(_filePathSaveGame))
            {
                ParseSaveDataXmlFile();
                ApplySaveDataToGame();
            }
            else
            {
                FileLogger.Instance.Log($"No existing save file found at \"{_filePathSaveGame}\". A new save file will be created upon saving.");
            }
        }

        /// <summary>
        /// Reads the wave configuration XML file and populates WaveManager
        /// </summary>
        private void ParseWaveXmlFile()
        {
            FileLogger.Instance.Log("");

            XmlReader waveReader = XmlReader.Create(_filePathWave);

            while (waveReader.Read())
            {
                if (waveReader.IsStartElement())
                {
                    string nameLower = waveReader.Name.ToLower();

                    if (nameLower == "wave")
                    {
                        _readWaveIndex++;
                        _basicCount = 0;
                        _fastCount = 0;
                        _tankCount = 0;

                        if (waveReader.HasAttributes)
                        {
                            while (waveReader.MoveToNextAttribute())
                            {
                                if (waveReader.Name.ToLower() == "timing")
                                {
                                    int.TryParse(waveReader.Value, out _startTime);
                                }
                                else if (waveReader.Name.ToLower() == "spawnduration")
                                {
                                    int.TryParse(waveReader.Value, out _spawnDuration);
                                }
                            }
                        }
                    }
                    else if (nameLower == "enemy")
                    {
                        string type = "";
                        int count = 0;

                        if (waveReader.HasAttributes)
                        {
                            while (waveReader.MoveToNextAttribute())
                            {
                                if (waveReader.Name.ToLower() == "type")
                                {
                                    type = waveReader.Value.ToLower();
                                }
                                else if (waveReader.Name.ToLower() == "count")
                                {
                                    int.TryParse(waveReader.Value, out count);
                                }
                            }
                        }

                        if (type == "basic") _basicCount += count;
                        else if (type == "fast") _fastCount += count;
                        else if (type == "tank") _tankCount += count;
                    }
                }
                else // End Element
                {
                    string nameLower = waveReader.Name.ToLower();

                    // When exiting wave, add temp copy to the list
                    if (nameLower == "wave")
                    {
                        TempWaveData tempDataStruct = new TempWaveData
                        {
                            StartTime = _startTime,
                            SpawnDuration = _spawnDuration,
                            BasicCount = _basicCount,
                            FastCount = _fastCount,
                            TankCount = _tankCount
                        };

                        _parsedWaves.Add(tempDataStruct);
                    }
                    else if (nameLower == "waves")
                    {
                        FileLogger.Instance.Log("EXITING - Finished parsing XML file structure.");
                    }
                }
            }

            // Clean up resources cleanly
            waveReader.Close();
            waveReader.Dispose();
        }

        /// <summary>
        /// Reads the player save data XML file and applies it to the game state. If no save file exists, this method will be skipped.
        /// </summary>
        private void ParseSaveDataXmlFile()
        {
            FileLogger.Instance.Log("");

            TempTowerData tempTower = new TempTowerData();

            XmlReader saveGameReader = XmlReader.Create(_filePathSaveGame);

            while (saveGameReader.Read())
            {
                if (saveGameReader.IsStartElement())
                {
                    string nameLower = saveGameReader.Name.ToLower();

                    if (nameLower == "playerdata")
                    {
                        if (saveGameReader.HasAttributes)
                        {
                            while (saveGameReader.MoveToNextAttribute())
                            {
                                if (saveGameReader.Name.ToLower() == "currentlevel")
                                {
                                    int.TryParse(saveGameReader.Value, out _playerDataCurrentLevel);
                                }
                                else if (saveGameReader.Name.ToLower() == "currentwave")
                                {
                                    int.TryParse(saveGameReader.Value, out _playerDataCurrentWave);
                                }
                                else if (saveGameReader.Name.ToLower() == "castlehp")
                                {
                                    int.TryParse(saveGameReader.Value, out _playerDataCastleHp);
                                }
                                else if (saveGameReader.Name.ToLower() == "coins")
                                {
                                    int.TryParse(saveGameReader.Value, out _playerDataCoins);
                                    GameManager.Instance.SetPlayerCoins(_playerDataCoins);
                                }
                            }
                        }
                    }
                    else if (nameLower == "placedtowers")
                    {
                        _savedTowers.Clear();
                    }
                    else if (nameLower == "tower")
                    {
                        tempTower = new TempTowerData();

                        if (saveGameReader.HasAttributes)
                        {
                            while (saveGameReader.MoveToNextAttribute())
                            {
                                if (saveGameReader.Name.ToLower() == "type")
                                {
                                    tempTower.type = saveGameReader.Value;
                                }
                                else if (saveGameReader.Name.ToLower() == "xposition")
                                {
                                    int.TryParse(saveGameReader.Value, out tempTower.xPosition);
                                }
                                else if (saveGameReader.Name.ToLower() == "yposition")
                                {
                                    int.TryParse(saveGameReader.Value, out tempTower.yPosition);
                                }
                            }
                        }
                    }
                }
                else // End Element
                {
                    string nameLower = saveGameReader.Name.ToLower();

                    // When exiting tower, add temp copy to the list
                    if (nameLower == "tower")
                    {
                        _savedTowers.Add(TowerManager.Instance.CreateTowerFromType(tempTower.type, tempTower.xPosition, tempTower.yPosition));
                    }
                }
            }

            // Clean up resources cleanly
            saveGameReader.Close();
            saveGameReader.Dispose();
        }

        /// <summary>
        /// Takes the parsed wave data and adds it to the WaveManager
        /// </summary>
        private void ApplyWaveDataToGame()
        {
            FileLogger.Instance.Log("");
            foreach (TempWaveData wave in _parsedWaves)
            {
                WaveManager.Instance.AddWave(new Wave(
                    wave.StartTime,
                    wave.SpawnDuration,
                    wave.BasicCount,
                    wave.FastCount,
                    wave.TankCount
                ));
            }
        }

        /// <summary>
        /// Takes the parsed player save data and applies it to the game state. 
        /// If no save file exists, this method will be skipped.
        /// </summary>
        private void ApplySaveDataToGame()
        {
            FileLogger.Instance.Log("");

            GameManager.Instance.SetCurrentLevel(_playerDataCurrentLevel);
            GameManager.Instance.CastleObj.SetCurrentHp(_playerDataCastleHp);
            WaveManager.Instance.SetCurrentWave(_playerDataCurrentWave);

            foreach (Tower tower in _savedTowers)
            {
                TowerManager.Instance.AddTower(tower, false);
            }
        }

        /// <summary>
        /// Write player progress to an XML file
        /// </summary>
        /// <param name="currentLevel"></param>
        /// <param name="currentWave"></param>
        /// <param name="castleHp"></param>
        /// <param name="placedTowers"></param>
        /// <param name="coins"></param>
        public void WritePlayerSaveData(int currentLevel, int currentWave, int castleHp, List<Tower> placedTowers, int coins)
        {
            FileLogger.Instance.Log("Saving player progress...");

            // Configure the writer settings for clean written XML with indentation
            XmlWriterSettings writerSettings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "    ", // 4 spaces indent
                NewLineOnAttributes = false
            };

            XmlWriter writer = XmlWriter.Create(_filePathSaveGame, writerSettings);

            writer.WriteStartDocument();

            // Open the master Root tag: <SaveData>
            writer.WriteStartElement("SaveData");

            // Write general player data attributes
            writer.WriteStartElement("PlayerData");
            writer.WriteAttributeString("currentLevel", currentLevel.ToString());
            writer.WriteAttributeString("currentWave", currentWave.ToString());
            writer.WriteAttributeString("castleHp", castleHp.ToString());
            writer.WriteAttributeString("coins", coins.ToString());
            writer.WriteFullEndElement(); // Closes </PlayerData>

            // Open the container element for towers: <PlacedTowers>
            writer.WriteStartElement("PlacedTowers");

            // Loop through every live tower in the game
            foreach (Tower tower in placedTowers)
            {
                writer.WriteStartElement("Tower");

                // Save the tower type name
                writer.WriteAttributeString("type", tower.GetType().Name);
                writer.WriteAttributeString("xPosition", tower.GetPosition().X.ToString());
                writer.WriteAttributeString("yPosition", tower.GetPosition().Y.ToString());

                writer.WriteFullEndElement(); // Closes </Tower>
            }

            writer.WriteFullEndElement(); // Closes </PlacedTowers>
            writer.WriteEndElement(); // Closes </SaveData>

            writer.WriteEndDocument(); // Explicitly flushes the document stream

            // Clean up resources cleanlyk
            writer.Close();
            writer.Dispose();
        }

        /// <summary>
        /// Deletes the player save data XML file.
        /// This resets the player's progress and forces the creation of a new save file upon the next save action.
        /// </summary>
        public void DeleteSaveDataFile()
        {
            FileLogger.Instance.Log("");
            if (File.Exists(_filePathSaveGame))
            {
                File.Delete(_filePathSaveGame);
                FileLogger.Instance.Log($"Deleted save file at \"{_filePathSaveGame}\".");
            }
        }
    }
}