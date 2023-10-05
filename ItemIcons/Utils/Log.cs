using System;

namespace ItemIcons.Utils;

public static class Log
{
    public static void Debug(string message) => Service.PluginLog.Debug(message);
    public static void Error(string message) => Service.PluginLog.Error(message);
    public static void Error(Exception e, string message) => Service.PluginLog.Error(e, message);
}
