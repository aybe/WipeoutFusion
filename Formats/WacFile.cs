using System;
using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace Formats
{
    public sealed class WacFile
    {
        private readonly WacFolder _parent;

        public WacFile([NotNull] Stream stream, [NotNull] WacFolder parent)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            _parent = parent;

            var reader = new BinaryReader(stream, Encoding.ASCII);

            var nameOffset = reader.ReadUInt32();
            Length = reader.ReadUInt32();
            Unknown = reader.ReadUInt32();
            Offset = reader.ReadUInt32();

            stream.Position = nameOffset;
            Name = WacHelper.ReadStringNullTerminated(reader);
        }

        public string Path => $"{_parent.Path}{Name}";
        public string Name { get; }
        public uint Length { get; }
        public uint Offset { get; }
        public uint Unknown { get; }

        public override string ToString()
        {
            return Name;
        }
    }
}