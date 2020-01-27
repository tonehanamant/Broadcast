
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
        /// <param name="path">Path to the directory</param>
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
        /// <param name="overwriteExisting">Optional: Flag to overwrite the existing file if there is one at the same location</param>
        /// <returns>New file path</returns>
        string Copy(string filePath, string destinationPath, bool overwriteExisting = false);

        /// <summary>
        /// Copies a stream to a file
        /// </summary>
        /// <param name="inputStream">Input stream</param>
        /// <param name="destinationPath">Destination directory</param>
        /// <param name="overwriteExisting">Optional: Flag to overwrite the existing file if there is one at the same location</param>
        /// <returns>New file path</returns>
        string Copy(Stream inputStream, string destinationPath, bool overwriteExisting = false);

        void Create(string path, Stream stream);

        /// <summary>
        /// Creates a zip archive file from the file paths
        /// </summary>
        /// <param name="filePaths">File paths to add to the archive</param>
        /// <param name="zipFileName">Zip archive file name</param>
        void CreateZipArchive(List<string> filePaths, string zipFileName);
        bool DirectoryExists(string filePath);
        void CreateDirectory(string filePath);

        /// <summary>
        /// Creates a stream containing a zip file from the file paths
        /// </summary>
        /// <param name="filePaths">Dictionary of path and name file pairs</param>
        /// <returns>Stream containing the zip archive</returns>
        Stream CreateZipArchive(IDictionary<string, string> filePaths);

        Stream GetFileStream(string filePath);
        Stream GetFileStream(string folderPath, string fileName);

        /// <summary>
        /// Creates a txt file with the content passed
        /// </summary>
        /// <param name="filePath">File path where to create the file</param>
        /// <param name="lines">New file content</param>
        void CreateTextFile(string filePath, List<string> lines);

        /// <summary>
        /// Creates a file with the content passed
        /// </summary>
        /// <param name="folderPath">Folder path where file should be created. The folder will be created automatically if it does not exist</param>
        /// <param name="fileName">File name</param>
        /// <param name="stream">File content</param>
        void Create(string folderPath, string fileName, Stream stream);
    }

    public class FileService : IFileService
    {
        ///<inheritdoc/>
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

        ///<inheritdoc/>
        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        ///<inheritdoc/>
        public void Delete(params string[] filePaths)
        {
            foreach (string path in filePaths)
            {
                File.Delete(path);
            }
        }

        ///<inheritdoc/>
        public string Move(string filePath, string destinationFolderPath)
        {
            var destinationPath = Path.Combine(destinationFolderPath, Path.GetFileName(filePath));

            if (Exists(destinationPath))
                Delete(destinationPath);

            File.Move(filePath, destinationPath);

            return destinationPath;
        }

        ///<inheritdoc/>
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

        ///<inheritdoc/>
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

        ///<inheritdoc/>
        public Stream CreateZipArchive(IDictionary<string, string> filePaths)
        {
            MemoryStream archiveFile = new MemoryStream();
            using (var archive = new ZipArchive(archiveFile, ZipArchiveMode.Create, true))
            {
                foreach (var pair in filePaths)
                {
                    archive.CreateEntryFromFile(pair.Key, pair.Value, CompressionLevel.Fastest);
                }
            }
            archiveFile.Seek(0, SeekOrigin.Begin);
            return archiveFile;
        }

        ///<inheritdoc/>
        public bool DirectoryExists(string filePath)
        {
            return Directory.Exists(filePath);
        }

        ///<inheritdoc/>
        public void CreateDirectory(string filePath)
        {
            Directory.CreateDirectory(filePath);
        }

        ///<inheritdoc/>
        public Stream GetFileStream(string filePath)
        {
            if (File.Exists(filePath))
            {
                using (var stream = File.OpenRead(filePath))
                {
                    var result = new MemoryStream();

                    stream.CopyTo(result);

                    return result;
                }
            }

            throw new ApplicationException($"File not found: {filePath}");
        }

        public Stream GetFileStream(string folderPath, string fileName)
        {
            var filePath = $@"{folderPath}\{fileName}";

            return GetFileStream(filePath);
        }

        ///<inheritdoc/>
        public void CreateTextFile(string filePath, List<string> lines)
        {
            File.WriteAllLines(filePath, lines);
        }

        ///<inheritdoc/>
        public void Create(string path, Stream stream)
        {
            if (File.Exists(path))
                File.Delete(path);

            using (var file = File.Create(path))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(file);
            }
        }

        public void Create(string folderPath, string fileName, Stream stream)
        {
            Directory.CreateDirectory(folderPath);

            var filePath = $@"{folderPath}\{fileName}";

            Create(filePath, stream);
        }
    }
}