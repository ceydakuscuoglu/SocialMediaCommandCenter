using System;
using System.IO;

namespace ShakyFruits.Core.Helpers
{
    public static class AssetPathHelper
    {
        /// <summary>
        /// Combines the base directory with the input file name or relative path.
        /// If the input is already rooted (full path like D:\... or C:\...), it is returned as is.
        /// Quotes, leading slashes, and redundant folder prefixes are safely handled.
        /// </summary>
        public static string ResolvePath(string? baseDirectory, string? inputPath)
        {
            if (string.IsNullOrWhiteSpace(inputPath))
                return string.Empty;

            var cleanInput = inputPath.Trim().Trim('"', '\'');
            if (string.IsNullOrWhiteSpace(cleanInput))
                return string.Empty;

            // If user passed a full Windows path (e.g. C:\... or D:\...)
            if (Path.IsPathRooted(cleanInput) && !cleanInput.StartsWith("/") && !cleanInput.StartsWith("\\"))
            {
                return cleanInput;
            }

            // Remove leading slashes if someone wrote /banana.png or \banana.png
            cleanInput = cleanInput.TrimStart('/', '\\');

            if (string.IsNullOrWhiteSpace(baseDirectory))
            {
                return cleanInput;
            }

            // Avoid duplicate directory name if user wrote e.g. "fruit_images/banana.png"
            var folderName = Path.GetFileName(baseDirectory.TrimEnd('/', '\\'));
            if (!string.IsNullOrEmpty(folderName))
            {
                if (cleanInput.StartsWith(folderName + "/", StringComparison.OrdinalIgnoreCase) ||
                    cleanInput.StartsWith(folderName + "\\", StringComparison.OrdinalIgnoreCase))
                {
                    cleanInput = cleanInput.Substring(folderName.Length + 1);
                }
            }

            return Path.Combine(baseDirectory, cleanInput);
        }

        /// <summary>
        /// Extracts clean filename or relative path from a full path.
        /// </summary>
        public static string GetRelativeOrFileName(string? baseDirectory, string? fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
                return string.Empty;

            var cleanPath = fullPath.Trim().Trim('"', '\'');

            if (!string.IsNullOrWhiteSpace(baseDirectory) &&
                cleanPath.StartsWith(baseDirectory, StringComparison.OrdinalIgnoreCase))
            {
                var rel = cleanPath.Substring(baseDirectory.Length).TrimStart('/', '\\');
                if (!string.IsNullOrEmpty(rel))
                    return rel;
            }

            return Path.GetFileName(cleanPath);
        }
    }
}
