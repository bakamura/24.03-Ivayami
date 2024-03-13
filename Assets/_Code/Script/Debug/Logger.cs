using System.Collections.Generic;
using UnityEngine;

public static class Logger {

    private static List<LogType> _typesToLog;

    static Logger() {
        _typesToLog = (Resources.Load("LoggerConfig") as LoggerConfig).typesToLog;
    }

    public static void Log(LogType type, string message) {
        if (_typesToLog.Contains(type)) Debug.Log($"{type.ToString()}: {message}");
    }

}
