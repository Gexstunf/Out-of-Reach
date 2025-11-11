using UnityEngine;

namespace GlobalUtils {
    [CreateAssetMenu(fileName = "LoggerSettings", menuName = "GlobalUtils/Logger Settings")]
    public class LoggerSO : ScriptableObject {
        private static LoggerSO _instance;
        public static LoggerSO Instance {
            get {
                if (_instance == null) {
                    _instance = Resources.Load<LoggerSO>("Databases/LoggerSettings");
                    if (_instance == null)
                        Debug.LogError("LoggerSettings.asset not found in a Resources folder!");
                }
                return _instance;
            }
        }

        [Header("Log Toggles")]
        public bool logMinorInfo = true;
        public bool logInfo = true;
        public bool logWarnings = true;
        public bool logErrors = true;

        [Header("Optional")]
        public bool includeTimestamp = true;

        public void LogMinor(string message) {
            if (!logMinorInfo) return;
            Debug.Log(FormatMessage("MINOR INFO", message));
        }
        
        public void Log(string message) {
            if (!logInfo) return;
            Debug.Log(FormatMessage("INFO", message));
        }

        public void LogWarning(string message) {
            if (!logWarnings) return;
            Debug.LogWarning(FormatMessage("WARN", message));
        }

        public void LogError(string message) {
            if (!logErrors) return;
            Debug.LogError(FormatMessage("ERROR", message));
        }

        private string FormatMessage(string type, string message) {
            if (includeTimestamp)
                return $"[{type}] [{System.DateTime.Now:HH:mm:ss}] {message}";
            return $"[{type}] {message}";
        }
    }
}