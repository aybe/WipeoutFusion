using CommandLine;

namespace WacExtract
{
    internal class Options
    {
        [Option('c', "wac", Required = true)]
        public string WacPath { get; set; }

        [Option('d', "wad", Required = true)]
        public string WadPath { get; set; }

        [Option('o', "output", Required = true)]
        public string OutputPath { get; set; }
    }
}