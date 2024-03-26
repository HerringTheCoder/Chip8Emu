using Microsoft.Extensions.Logging;

namespace Chip8Emu.DebugTool;

public static class LoggerHelper
{
  public static ILogger<T> GetLogger<T>() where T : class
  {
    var loggerFactory = LoggerFactory.Create(builder =>
    {
      builder.AddConsole();
    });

    // Create a logger
    var logger = loggerFactory.CreateLogger<T>();

    // Example logging
    logger.LogInformation("Information log test");
    logger.LogWarning("Warning log test");
    logger.LogError("Error log test");
    
    if (loggerFactory is IDisposable disposableLoggerFactory)
    {
      disposableLoggerFactory.Dispose();
    }

    return logger;
  }
}