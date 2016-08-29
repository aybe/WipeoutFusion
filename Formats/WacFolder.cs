using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace Formats
{
    public sealed class WacFolder : IEnumerable<WacFolder>
    {
        private readonly WacFolder _parent;

        public WacFolder([NotNull] Stream stream, [CanBeNull] WacFolder parent)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            _parent = parent;

            var reader = new BinaryReader(stream, Encoding.ASCII);

            var files = reader.ReadUInt32();
            var folders = reader.ReadUInt32();
            var folderOffset = reader.ReadUInt32();
            var filesOffset = reader.ReadUInt32();
            var foldersOffset = reader.ReadUInt32();

            // folder name
            stream.Position = folderOffset;
            Name = WacHelper.ReadStringNullTerminated(reader);

            // files
            stream.Position = filesOffset;
            var fileOffsets = new uint[files];
            for (var i = 0; i < files; i++)
            {
                fileOffsets[i] = reader.ReadUInt32();
            }
            Files = new WacFile[files];
            for (var i = 0; i < fileOffsets.Length; i++)
            {
                stream.Position = fileOffsets[i];
                Files[i] = new WacFile(stream, this);
            }

            // folders
            stream.Position = foldersOffset;
            var folderOffsets = new uint[folders];
            for (var i = 0; i < folders; i++)
            {
                folderOffsets[i] = reader.ReadUInt32();
            }
            Folders = new WacFolder[folders];
            for (var i = 0; i < folderOffsets.Length; i++)
            {
                stream.Position = folderOffsets[i];
                Folders[i] = new WacFolder(stream, this);
            }
        }

        public string Path
        {
            get
            {
                var stack = new Stack<string>();
                var folder = this;
                while (folder != null)
                {
                    stack.Push(folder.Name);
                    folder = folder._parent;
                }
                var concat = string.Concat(stack);
                return concat;
            }
        }

        public string Name { get; }

        public WacFolder[] Folders { get; }

        public WacFile[] Files { get; }

        #region IEnumerable<WacFolder> Members

        public IEnumerator<WacFolder> GetEnumerator()
        {
            return Folders.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Folders.GetEnumerator();
        }

        #endregion

        public override string ToString()
        {
            return Name;
        }
    }
}