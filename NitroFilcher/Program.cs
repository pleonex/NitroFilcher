//
//  Program.cs
//
//  Author:
//       Benito Palacios Sánchez <benito356@gmail.com>
//
//  Copyright (c) 2015 Benito Palacios Sánchez
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace NitroFilcher
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            if (args.Length != 2) {
                Console.WriteLine("USAGE: NitroFilcher.exe ROM_PATH OUTPUT_PREFIX");
                return;
            }
                
            string romPath = args[0];
            string prefix = args[1];

            // Check that the file exists
            if (!File.Exists(romPath)) {
                Console.WriteLine("ERROR: The ROM file does not exist.");
                return;
            }

            if (Path.GetExtension(romPath) != ".nds") {
                Console.WriteLine("ERROR: Invalid file extension. It must be .nds");
                return;
            }

            // Create the resolve (it will parse the ROM file)
            Console.WriteLine("Reading ROM file...");
            var resolver = new FileResolver(romPath);

            // Create the desmume process and start it.
            Console.WriteLine("Starting DeSmuME emulator...");
            Console.WriteLine("\tPlay until you reach the scene you want.");
            Console.WriteLine("\tThe program will parse the files read by the game.");
            Console.WriteLine("\tWhen you are done, close the emulator (DeSmuME).");
            var desmume = new DesmumeProcess(romPath, resolver);
            desmume.Start();

            // Start the resolver thread (files may be already enqueued but no problem).
            Thread resolverThread = new Thread(new ThreadStart(resolver.StartProcessing));
            resolverThread.Start();

            // Block the main thread with the desmume process.
            desmume.WaitForExit();

            // Once we have closed desmume, request to the resolver thread to stop
            // when there are no more messages to parse. Wait for that.
            resolver.Stop();
            resolverThread.Join();

            Console.WriteLine();
            Console.WriteLine("DeSmuME closed.");
            Console.WriteLine("Writing output file...");

            // Export the output file.
            string cwd = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string outputFilename = prefix + DateTime.Now.ToBinary() + ".txt";
            resolver.Export(Path.Combine(cwd, outputFilename));
        }
    }
}
