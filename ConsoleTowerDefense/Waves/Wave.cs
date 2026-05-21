namespace ConsoleTowerDefense
{
    internal class Wave
    {
        public bool IsActive = false;

        private int _startTime;
        private int _spawnDuration;
        private int _basicCount;
        private int _fastCount;
        private int _tankCount;

        /// <summary>
        /// Initializes a new wave with the specified parameters.
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="spawnDuration"></param>
        /// <param name="basicCount"></param>
        /// <param name="fastCount"></param>
        /// <param name="tankCount"></param>
        public Wave(int startTime, int spawnDuration, int basicCount, int fastCount, int tankCount)
        {
            _startTime = startTime;
            _spawnDuration = spawnDuration;
            _basicCount = basicCount;
            _fastCount = fastCount;
            _tankCount = tankCount;
        }
        public int GetStartTime() => _startTime;

        public int GetSpawnDuration() => _spawnDuration;

        public int GetBasicCount() => _basicCount;

        public int GetFastCount() => _fastCount;

        public int GetTankCount() => _tankCount;
    }
}
