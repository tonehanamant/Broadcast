
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Common.Services
{
    public interface IFileService
    {
        List<string> GetFiles(string path);
        bool Exists(string path);

        /// <summary>
        /// Delete all the files in the filePaths
        /// </summary>
        /// <param name="filePaths">List of file paths to be deleted</param>
        void Delete(params string[] filePaths);

        /// <summary>
        /// Moved a file to another destination
        /// </summary>
        /// <param name="filePath">Current file path</param>
        /// <param name="destinationFolderPath">Destination directory</param>
        /// <returns>New file path</returns>
        string Move(string filePath, string destinationFolderPath);
    }

    public class FileService : IFileService
    {
        public List<string> GetFiles(string path)
        {
            return Directory.GetFiles(path).ToList();
        }

        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        /// Delete all the files in the filePaths
        /// </summary>
        /// <param name="filePaths">List of file paths to be deleted</param>
        public void Delete(params string[] filePaths)
        {
            foreach(string path in filePaths)
            {
                File.Delete(path);
            }            
        }

        /// <summary>
        /// Moved a file to another destination
        /// </summary>
        /// <param name="filePath">Current file path</param>
        /// <param name="destinationFolderPath">Destination directory</param>
        /// <returns>New file path</returns>
        public string Move(string filePath, string destinationFolderPath)
        {
            var destinationPath = Path.Combine(destinationFolderPath, Path.GetFileName(filePath));

            if (Exists(destinationPath))
                Delete(destinationPath);

            File.Move(filePath, destinationPath);

            return destinationPath;
        }
    }
}