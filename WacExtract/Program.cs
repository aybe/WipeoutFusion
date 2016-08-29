using System;
using System.IO;
using CommandLine;
using Formats;

namespace WacExtract
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<Options>(args);
            result.WithParsed(Action);
        }

        private static void Action(Options options)
        {
            var wac = options.WacPath;
            if (!File.Exists(wac))
                throw new FileNotFoundException("", wac);

            var wad = options.WadPath;
            if (!File.Exists(wad))
                throw new FileNotFoundException("", wad);

            var outputPath = options.OutputPath;

            // using an Action<T> since IProgress<T> is unordered from within a console
            var progress =
                new Action<Tuple<double, string>>(
                    tuple => { Console.WriteLine($"{$"{tuple.Item1:P}".PadLeft(8, '0')} : {tuple.Item2}"); });

            using (var stream1 = File.OpenRead(wac))
            using (var stream2 = File.OpenRead(wad))
            {
                WacHelper.Extract(stream1, stream2, outputPath, progress);
            }
            Console.WriteLine("All done :)");
        }
    }
}