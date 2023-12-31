﻿using Common.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;


namespace Services.Broadcast.IntegrationTests
{
    public class FileServiceStub : IFileService
    {
        /// <summary>
        /// Gets the inputs from the CreateTextFile call.
        ///     Item1 = filePath
        ///     Item2 = lines
        /// </summary>
        public List<Tuple<string, List<string>>> CreatedTextFiles { get; } = new List<Tuple<string, List<string>>>();

        /// <summary>
        /// Gets the list of CreateDirectory calls
        ///     string = filePath
        /// </summary>
        public List<string> CreatedDirectories { get; } = new List<string>();

        /// <summary>
        /// If exists then when CreateTextFile is called this will be thrown.
        /// </summary>
        public static Exception ThrownOnCreateTextFile { get; set; }

        public virtual List<string> GetFiles(string path)
        {
            return new List<string>();
        }

        public virtual bool Exists(string path)
        {
            return true;
        }

        public virtual void Delete(params string[] path)
        {
        }

        public virtual string Move(string filePath, string destinationFolderPath)
        {
            return string.Empty;
        }

        public void CreateZipArchive(List<string> filePaths, string zipFileName)
        {
            throw new NotImplementedException();
        }

        public virtual string Copy(string filePath, string destinationPath, bool overwriteExisting = false)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the list of Copy calls with the Stream parameter.
        /// - Item1 = Stream inputStream
        /// - Item2 = string destinationPath
        /// - Item3 = bool overwriteExisting
        ///
        /// Set ThrowOnCopyStreamCall to True to have the call throw an exception.
        /// </summary>
        public List<Tuple<Stream, string, bool>> CopyStreamCalls { get; } = new List<Tuple<Stream, string, bool>>();

        /// <summary>
        /// True to force an exception thrown from the Copy  with the Stream parameter.
        /// </summary>
        public bool ThrowOnCopyStreamCall { get; set; } = false;

        /// <summary>
        /// Inputs are added to stub property CopyStreamCalls.
        /// </summary>
        /// <returns>destinationPath</returns>
        public virtual string Copy(Stream inputStream, string destinationPath, bool overwriteExisting = false)
        {
            CopyStreamCalls.Add(new Tuple<Stream, string, bool>(inputStream, destinationPath, overwriteExisting));

            if (ThrowOnCopyStreamCall)
            {
                throw new Exception("Test exception per property ThrowOnCopyStreamCall.");
            }

            return destinationPath;
        }

        public bool DirectoryExists(string filePath)
        {
            if (!CreatedDirectories.Contains(filePath))
            {
                CreatedDirectories.Add(filePath);
                return true;
            }
            return false;
        }

        public void CreateDirectory(string filePath)
        {
            CreatedDirectories.Add(filePath);
        }

        public virtual Stream CreateZipArchive(IDictionary<string, string> filePaths)
        {
            throw new NotImplementedException();
        }

        public virtual void CreateTextFile(string filePath, List<string> lines)
        {
            if (ThrownOnCreateTextFile != null)
            {
                throw ThrownOnCreateTextFile;
            }
            CreatedTextFiles.Add(new Tuple<string, List<string>>(filePath, lines));
        }
        public virtual Stream GetFileStream(string filePath)
        {
            throw new NotImplementedException();
        }

        public virtual void Create(string path, Stream stream)
        {
        }

        public Stream GetFileStream(string folderPath, string fileName)
        {
            var found = CreatedFileStreams.FirstOrDefault(s =>
                s.Item1.Equals(folderPath) && s.Item2.Equals(fileName));

            if (found == null)
            {
                throw new FileNotFoundException();
            }
            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream);
            writer.Write(found.Item3);
            writer.Flush();

            memoryStream.Position = 0;
            return memoryStream;
        }

        /// <summary>
        /// The parameters passed to the create method.
        /// </summary>
        /// <value>
        /// folderPath, fileName, fileContent
        /// </value>
        public List<Tuple<string, string, string>> CreatedFileStreams { get; } = new List<Tuple<string, string, string>>();

        public void Create(string folderPath, string fileName, Stream stream)
        {
            stream.Position = 0;
            using (var reader = new StreamReader(stream))
            {
                var fileContent = reader.ReadToEnd();
                CreatedFileStreams.Add(new Tuple<string, string, string>(folderPath, fileName, fileContent));
            }
        }
    }

    public class InMemoryFileServiceStubb : FileServiceStub
    {
        public List<string> Paths { get; set; } = new List<string>();
        public List<Stream> Streams { get; set; } = new List<Stream>();

        public override void Create(string path, Stream stream)
        {
            Paths.Add(path);
            Streams.Add(stream);
        }

        public override List<string> GetFiles(string path)
        {
            return Paths;
        }

        public override Stream GetFileStream(string filePath)
        {
            return Streams.FirstOrDefault();
        }
    }

    public class FailingFileServiceStub : FileServiceStub
    {
        public List<string> Paths { get; set; } = new List<string>();
        public List<Stream> Streams { get; set; } = new List<Stream>();

        public override void Create(string path, Stream stream)
        {
            throw new Exception("FailingFileServiceStub never creates files");
        }

        public override List<string> GetFiles(string path)
        {
            return Paths;
        }

        public override Stream GetFileStream(string filePath)
        {
            return Streams.FirstOrDefault();
        }
    }

    public class FileServiceSingleFileStubb : FileServiceStub
    {
        private readonly string _SingleFileName = "file1.txt";
        private readonly string _BasePath = "c:\\temp";
        public override List<string> GetFiles(string path)
        {
            return new List<string>() {_SingleFileName};
        }

        public override bool Exists(string path)
        {
            return string.Compare(Path.Combine(_BasePath, _SingleFileName), path,
                       StringComparison.CurrentCultureIgnoreCase) == 0;
        }
    }
}