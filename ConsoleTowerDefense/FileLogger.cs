using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace ConsoleTowerDefense
{
    internal class FileLogger
    {
        /* * ARCHITECTURE OVERVIEW: STREAMING vs INDIVIDUAL DISK I/O
           * * 1. HIGH-SPEED PERSISTENT PIPELINE (StreamWriter)
             * Previous iterations used File.AppendAllText, which forces the OS to locate, open, 
             * write, and close the physical file on every single call. During tight rendering loops 
             * running at 60-300+ FPS, Windows file system locks inevitably choke, causing dropped 
             * logs or severe frame rate stutter. StreamWriter opens a dedicated pipeline to RAM 
             * exactly once on initialization, streaming data with near-zero overhead.
           * * 2. MEMORY DEDUPLICATION GATEWAY (RAM Buffering)
             * To protect disk throughput, identical sequential entries (such as empty rendering loops 
             * like DrawRectangle) are intercepted in memory. The engine defers physical I/O and increments 
             * a tracking multiplier (_repeatCount) until a new distinct log signature breaks the sequence.
           * * 3. EXPLICIT LIFECYCLE MANAGEMENT (Shutdown)
             * Because StreamWriter locks the file stream in memory for exclusive use by the game process, 
             * the file will appear empty or inaccessible to external editors until the stream is formally 
             * flushed and closed via FileLogger.Instance.Shutdown() upon application exit.
         */

        private bool _isLoggingEnabled = true; // Set to true to enable logging, false to disable. -> quick toggle for enable/disable without removing calls.

        public static FileLogger Instance;
        private StreamWriter _writer;

        // State tracking
        private string _lastMessage = null;
        private string _lastMethod = null;
        private string _lastClass = null;
        private int _repeatCount = 0;
        private bool _hasPendingLog = false;


        /// <summary>
        /// Initializes the FileLogger by creating a new log file in the specified folder with the given name.
        /// </summary>
        /// <param name="fileLoggerName">The name for the actual .log file</param>
        /// <param name="folderPath">The location where the .log file should sit</param>
        public FileLogger(string fileLoggerName, string folderPath)
        {
            if (Instance != null)
            {
                throw new Exception("FileLogger already exists!");
            }
            Instance = this;

            string filePath = Path.Combine(folderPath, fileLoggerName + ".log");

            // Initializes an unbuffered stream; append: false overwrites previous session files
            _writer = new StreamWriter(filePath, append: false);
            _writer.AutoFlush = true;

            _writer.WriteLine($"--- Launched Console Tower Defense on \"{DateTime.Now}\" ---");
        }

        /// <summary>
        /// Logs a message. Intercepts and groups consecutive identical calls into a single line.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="methodName"></param>
        /// <param name="sourceFilePath"></param>
        public void Log(string message, [CallerMemberName] string methodName = "", [CallerFilePath] string sourceFilePath = "")
        {
            // automatically grabs method name and file name if none are provided
            // -> allows to quickly add logs by just typing FileLogger.Instance.Log("message")
            // without worrying about parameters, while still maintaining rich context for debugging and performance tracing.
            // -> Message is also optional
            // Dev note: This is actually really handy... might use again later in other projects ;)

            if (!_isLoggingEnabled) return;

            string className = Path.GetFileNameWithoutExtension(sourceFilePath);

            if (methodName == ".ctor") // Constructor method is always named as .ctor by the compiler, so for clarity it is renamed to "Constructor".
            {
                methodName = "Constructor";
            }

            // if the current log entry matches the previous one, increment the repeat count and hold writing to disk
            if (_hasPendingLog && message == _lastMessage && methodName == _lastMethod && className == _lastClass)
            {
                _repeatCount++;
                return;
            }

            // send previous log to disk before processing the new one, ensuring that sequential patterns are properly grouped and timestamped
            FlushRepeats();

            // Update baseline tracking variables
            _lastMessage = message;
            _lastMethod = methodName;
            _lastClass = className;
            _repeatCount = 1;
            _hasPendingLog = true;
        }

        /// <summary>
        /// Commits accumulated sequential log patterns from memory to the physical file stream.
        /// </summary>
        private void FlushRepeats()
        {
            if (!_hasPendingLog) return;

            try
            {
                string timestampedMessage = $"[{DateTime.Now:HH:mm:ss.fff}] [{_lastClass} : {_lastMethod}]";

                // Formats optionally to support clean execution tracing for parameterless rendering loops
                if (!string.IsNullOrEmpty(_lastMessage))
                {
                    timestampedMessage += $" {_lastMessage}";
                }

                // Appends repetition multiplier to indicate compressed data volume
                if (_repeatCount > 1)
                {
                    timestampedMessage += $" (x{_repeatCount})";
                }

                _writer.WriteLine(timestampedMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LOGGER ERROR: {ex.Message}");
            }

            _hasPendingLog = false;
        }

        /// <summary>
        /// Flushes remaining memory buffers and safely releases the active file stream.
        /// </summary>
        public void Shutdown()
        {
            FlushRepeats();
            try
            {
                _writer.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [FileLogger : Shutdown] --- Game Session Closed ---");
                _writer.Close();
                _writer.Dispose();
            }
            catch { }
        }
    }
}