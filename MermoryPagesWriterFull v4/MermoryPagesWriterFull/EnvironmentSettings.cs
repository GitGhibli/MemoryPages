using System.IO;

namespace MemoryPagesWriterFull
{
    class EnvironmentSettings
    {
        public static string GetSharedAbsolutePath(string fileName)
        {
            return Path.Combine(File.ReadAllText("absolutePath.txt"), fileName);
        }
    }
}
