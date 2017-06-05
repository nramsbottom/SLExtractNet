
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SLExtractNet
{
    class Program
    {
        static int Main(string[] args)
        {

            if (args.Length == 0)
            {
                Console.Error.WriteLine("No .hog filename specified.");
                return 1;
            }

            try
            {
                var hogPath = args.First(); // .hog filename
                var hogName = Path.GetFileNameWithoutExtension(hogPath);
                var destination = Path.Combine(Environment.CurrentDirectory, hogName); // write to working directory plus the name of the hog file

                var entries = GetEntries(hogPath);

                Console.WriteLine($"Extracting files to '{destination}'");

                foreach (var entry in entries)
                {
                    Console.WriteLine($"{entry.Name,-40} {entry.Offset,-10} {entry.Length,-10}");
                    Extract(hogPath, entry, destination);
                }

                Console.WriteLine("Done.");

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"ERROR: {ex.Message}");
                return 1;
            }

            return 0;
        }

        static void Extract(string hogPath, HogFileEntry entry, string destination)
        {
            if (!File.Exists(hogPath))
                throw new Exception("The source file does not exist.");
            if (!Directory.Exists(destination))
                Directory.CreateDirectory(destination);

            var filename = Path.Combine(destination, entry.Name);

            if (File.Exists(filename))
                throw new Exception("The destination file already exists.");

            using (var input = File.OpenRead(hogPath))
            using (var output = File.OpenWrite(filename))
            {
                CopyEntry(entry, input, output);
            }
        }

        private static void CopyEntry(HogFileEntry entry, Stream input, Stream output)
        {
            input.Seek(entry.Offset, SeekOrigin.Begin);

            // copy in 64k chunks
            byte[] buffer = new byte[0xFFFF];

            int write = (int)entry.Length;
            int read = 0;
            while (write > 0)
            {
                read = input.Read(buffer, 0, buffer.Length);
                output.Write(buffer, 0, read);
                write -= read;
            }
        }

        static IEnumerable<HogFileEntry> GetEntries(string path)
        {
            var entries = new List<HogFileEntry>();

            using (var stream = File.OpenRead(path))
            using (var reader = new BigEndianReader(new BinaryReader(stream)))
            {
                UInt32 magic = reader.ReadUInt32(); // file signature

                if (magic != 0x42494746)
                    throw new Exception("The specified file is not of the correct type (magic number failed)");

                UInt32 size = reader.ReadUInt32();  // size of this file
                UInt32 files = reader.ReadUInt32(); // number of files
                UInt32 data = reader.ReadUInt32();  // location at which the file data begins

                for (int count = 0; count < files; count++)
                {
                    UInt32 offset = reader.ReadUInt32();
                    UInt32 length = reader.ReadUInt32();

                    /* HACK: In most of the files that I've run through this there's
                     *       a block of padding data between the end of the list of
                     *       file info and the start of the file data. The padding bytes
                     *       are filled with runs of 0xCD.
                     * 
                     *       I've not been able to figure out what this is or how I should
                     *       process it. Luckily, I don't need to because all the file entries
                     *       can be read and this ignored.
                     */
                    if (offset == 0xcdcdcdcd && length == 0xcdcdcdcd)
                        break;

                    string name = reader.ReadAsciiStringZ();

                    entries.Add(new HogFileEntry()
                    {
                        Name = name,
                        Offset = offset,
                        Length = length
                    });

                }
            }

            return entries;
        }
    }
}
