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
using System.Threading;

namespace NitroFilcher
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            if (args.Length != 1) {
                Console.WriteLine("USAGE: NitroFilcher.exe ROM_PATH");
                return;
            }

            string romPath = args[0];

            // Create the resolve (it will parse the ROM file)
            var resolver = new FileResolver(romPath);

            // Create the desmume process and start it.
            var desmume = new DesmumeProcess(romPath, resolver);
            desmume.Start();

            // Start the resolver thread
            Thread resolverThread = new Thread(new ThreadStart(resolver.StartProcessing));
            resolverThread.Start();

            // Block the main thread with the desmume process.
            desmume.WaitForExit();

            // Once we have closed desmume, request to the resolver thread to stop
            // when there are no more messages to parse. Wait for that.
            resolver.Stop();
            resolverThread.Join();

            // Export the output file.
            string outputFilename = "ninokuni" + DateTime.Now.ToBinary() + ".txt";
            resolver.Export(outputFilename);
        }
    }
}
