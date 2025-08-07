namespace Utilities;

/// <summary>
/// Provides functionality to load environment variables from a .env file.
/// This class is responsible for parsing .env files and setting environment variables in the current process.
/// </summary>
public static class EnvLoader
{
    /// <summary>
    /// Loads environment variables from a specified .env file.
    /// </summary>
    /// <param name="filePath">The path to the .env file to load.</param>
    /// <exception cref="FileNotFoundException">Thrown when the specified .env file is not found.</exception>
    /// <remarks>
    /// Reads the file line by line, skipping comments and empty lines, then parses key-value pairs and sets them as environment variables.
    /// </remarks>
    public static void LoadEnvFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($".env file not found at: {filePath}");

        foreach (var line in File.ReadLines(filePath))
        {
            var trimmed = line.Trim();

            // Skip empty lines or comments
            if (string.IsNullOrEmpty(trimmed) ||
                trimmed.StartsWith('#') ||
                trimmed.StartsWith("//") ||
                trimmed.StartsWith(';'))
                continue;

            var separatorIndex = trimmed.IndexOf('=');
            if (separatorIndex <= 0 || separatorIndex == trimmed.Length - 1)
                continue;

            var key = trimmed[..separatorIndex].Trim();
            var value = trimmed[(separatorIndex + 1)..].Trim().Trim('"');

            if (!string.IsNullOrEmpty(key))
                Environment.SetEnvironmentVariable(key, value);
        }
    }
}