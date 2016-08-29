using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace Formats
{
    public static class WacHelper
    {
        public static void Extract(
            [NotNull] Stream wacStream, [NotNull] Stream wadStream, [NotNull] string targetPath,
            [CanBeNull] Action<Tuple<double, string>> progress)
        {
            if (wacStream == null)
                throw new ArgumentNullException(nameof(wacStream));

            if (wadStream == null)
                throw new ArgumentNullException(nameof(wadStream));

            if (targetPath == null)
                throw new ArgumentNullException(nameof(targetPath));

            var folder = new WacFolder(wacStream, null);
            var files = GetFilesRecursive(folder);
            var done = 0;
            var progress1 = new Action<string>(s =>
            {
                progress?.Invoke(new Tuple<double, string>(1.0d/files.Length*done, s));
                done++;
            });
            Extract(folder, wadStream, targetPath, progress1);
        }

        private static WacFile[] GetFilesRecursive([NotNull] WacFolder wacFolder)
        {
            if (wacFolder == null)
                throw new ArgumentNullException(nameof(wacFolder));

            var folders = new List<WacFolder>();
            GetFoldersRecursive(wacFolder, ref folders);

            var files = folders.SelectMany(s => s.Files).ToArray();

            return files;
        }

        private static void GetFoldersRecursive([NotNull] WacFolder folder, [NotNull] ref List<WacFolder> list)
        {
            if (folder == null)
                throw new ArgumentNullException(nameof(folder));

            if (list == null)
                throw new ArgumentNullException(nameof(list));

            if (!folder.Any())
                return;

            list.AddRange(folder);

            foreach (var folder1 in folder)
            {
                GetFoldersRecursive(folder1, ref list);
            }
        }

        private static void Extract([NotNull] WacFolder wacFolder, [NotNull] Stream stream, [NotNull] string path,
            [CanBeNull] Action<string> progress)
        {
            if (wacFolder == null)
                throw new ArgumentNullException(nameof(wacFolder));

            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(path));

            var folderPath = wacFolder.Path;
            if (folderPath == @"\")
                folderPath = @".\";

            var path1 = path.TrimEnd('\\');

            var path2 = Path.Combine(path1, folderPath.TrimStart('\\'));
            if (!Directory.Exists(path2))
                Directory.CreateDirectory(path2);

            var files = wacFolder.Files;
            foreach (var file in files)
            {
                var filePath = file.Path;
                var fileSize = file.Length;
                var fileStart = file.Offset;

                stream.Position = fileStart;
                var buffer = new byte[fileSize];
                var read = stream.Read(buffer, 0, buffer.Length);
                if (read != buffer.Length)
                    throw new InvalidOperationException("Not enough bytes read.");

                var path3 = Path.Combine(path1, filePath.TrimStart('\\'));
                File.WriteAllBytes(path3, buffer);

                progress?.Invoke(filePath);
            }

            var folders = wacFolder.Folders;
            foreach (var folder in folders)
            {
                Extract(folder, stream, path, progress);
            }
        }

        internal static string ReadStringNullTerminated([NotNull] BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var builder = new StringBuilder();
            char c;
            while ((c = reader.ReadChar()) != '\0')
            {
                builder.Append(c);
            }
            var s = builder.ToString();
            return s;
        }
    }
}