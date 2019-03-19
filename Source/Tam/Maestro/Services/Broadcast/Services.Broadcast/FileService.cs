
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Common.Services
{
    public interface IFileService
    {
        /// <summary>
        /// Returns a list with the file paths contined in the folder
        /// </summary>
        /// <param name="folderPath">Path to the directory</param>
        /// <returns>List of file paths contained in the directory</returns>
        List<string> GetFiles(string path);

        /// <summary>
        /// Checks if the path exists
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <returns>True or false</returns>
        bool Exists(string path);

        /// <summary>
        /// Delete all the files in the filePaths
        /// </summary>
        /// <param name="filePaths">List of file paths to be deleted</param>
        void Delete(params string[] filePaths);

        /// <summary>
        /// Moves a file to another destination
        /// </summary>
        /// <param name="filePath">Current file path</param>
        /// <param name="destinationFolderPath">Destination directory</param>
        /// <returns>New file path</returns>
        string Move(string filePath, string destinationFolderPath);

        /// <summary>
        /// Copies a file to another destination
        /// </summary>
        /// <param name="filePath">Current file path</param>
        /// <param name="destinationPath">Destination directory</param>
        /// <param name="deleteExisting">Optional: Flag to delete the existing file if there is one at the same location</param>
        /// <returns>New file path</returns>
        string Copy(string filePath, string destinationPath, bool overwriteExisting = false);

        /// <summary>
        /// Copies a stream to a file
        /// </summary>
        /// <param name="inputStream">Input stream</param>
        /// <param name="destinationPath">Destination directory</param>
        /// <param name="deleteExisting">Optional: Flag to delete the existing file if there is one at the same location</param>
        /// <returns>New file path</returns>
        string Copy(Stream inputStream, string destinationPath, bool overwriteExisting = false);

        /// <summary>
        /// Creates a zip archive file from the file paths
        /// </summary>
        /// <param name="filePaths">List of file paths to add to the archive</param>
        /// <param name="zipFileName">Zip archive file name</param>
        void CreateZipArchive(List<string> filePaths, string zipFileName);
    }

    public class FileService : IFileService
    {
        /// <summary>
        /// Returns a list with the file paths contined in the folder
        /// </summary>
        /// <param name="folderPath">Path to the directory</param>
        /// <returns>List of file paths contained in the directory</returns>
        public List<string> GetFiles(string folderPath)
        {
            List<string> filepathList;
            try
            {
                filepathList = Directory.GetFiles(folderPath).ToList();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Could not find {folderPath}.  Please check it is created.", e);
            }

            return filepathList;
        }

        /// <summary>
        /// Checks if the path exists
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <returns>True or false</returns>
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

        /// <summary>
        /// Copies a file to another destination
        /// </summary>
        /// <param name="filePath">Current file path</param>
        /// <param name="destinationPath">Destination directory</param>
        /// <param name="deleteExisting">Optional: Flag to delete the existing file if there is one at the same location</param>
        /// <returns>New file path</returns>
        public string Copy(string filePath, string destinationPath, bool overwriteExisting = false)
        {
            if (Exists(destinationPath))
            {
                if (overwriteExisting)
                {
                    Delete(destinationPath);
                }
                else
                {
                    throw new Exception($"Cannot overwrite {filePath} with {destinationPath}");
                }
            }

            File.Copy(filePath, destinationPath, overwriteExisting);            

            return destinationPath;
        }

        /// <summary>
        /// Copies a stream to a file
        /// </summary>
        /// <param name="inputStream">Input stream</param>
        /// <param name="destinationPath">Destination directory</param>
        /// <param name="deleteExisting">Optional: Flag to delete the existing file if there is one at the same location</param>
        /// <returns>New file path</returns>
        public string Copy(Stream inputStream, string destinationPath, bool overwriteExisting = false)
        {
            if (Exists(destinationPath))
            {
                if (overwriteExisting)
                {
                    Delete(destinationPath);
                }
                else
                {
                    throw new Exception($"Cannot overwrite {destinationPath}");
                }
            }

            using (var fileStream = new FileStream(destinationPath, FileMode.CreateNew, FileAccess.Write))
            {
                inputStream.CopyTo(fileStream);
            }

            return destinationPath;
        }

        public void CreateZipArchive(List<string> filePaths, string zipFileName)
        {
            using (ZipArchive zip = ZipFile.Open(zipFileName, ZipArchiveMode.Create))
            {
                foreach (var filePath in filePaths)
                {
                    // Add the entry for each file
                    zip.CreateEntryFromFile(filePath, Path.GetFileName(filePath), CompressionLevel.Fastest);
                }
            }
        }
    }
}