using System;
using System.IO;

namespace CHROMA_8
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0 || !File.Exists(args[0]))
                    return;
                new Emulator(File.ReadAllBytes(args[0])).Run();
            } catch(Exception e)
            {
                Console.WriteLine("Error reading ROM: " + e.Message);
                Console.ReadKey();
                return;
            }

            Console.WriteLine("No ROM file provided. Please open a ROM with the emulator.");
            Console.ReadKey();
        }
    }
}
