//
//  FileResolver.cs
//
//  Author:
//       Benito Palacios Sánchez (aka pleonex) <benito356@gmail.com>
//
//  Copyright (c) 2016 Benito Palacios Sánchez (c) 2015
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
using System.Collections.Concurrent;
using System.Threading;
using Libgame;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Libgame.IO;
using Nitro.Rom;

namespace NitroFilcher
{
    public class FileResolver
    {
        private readonly ConcurrentQueue<string> outputQueue;
        private readonly List<string> filenames;

        private bool stopRequested;
        private GameFile rom;
        private int count;

        public FileResolver(string romPath)
        {
            outputQueue = new ConcurrentQueue<string>();
            filenames = new List<string>();
            InitializeRomSystem(romPath);
        }

        private void InitializeRomSystem(string romPath)
        {
            // Read the ROM file.
            DataStream romStream = new DataStream(romPath, FileMode.Open, FileAccess.Read);
            rom = new GameFile("Game.nds", romStream);
            Format romFormat = new Rom();
            romFormat.Initialize(rom);
            romFormat.Read();
        }

        public void Enqueue(string line)
        {
            outputQueue.Enqueue(line);
        }

        public void Stop()
        {
            stopRequested = true;
        }

        public void StartProcessing()
        {
            string line;
            stopRequested = false;
            count = 0;

            while (!stopRequested || !outputQueue.IsEmpty) {
                // If there is not more data, sleep a bit
                if (outputQueue.IsEmpty)
                    Thread.Sleep(1000);

                // Process the next line
                if (outputQueue.TryDequeue(out line))
                    ProcessLine(line);
            }
        }

        public void Export(string filePath)
        {
            File.WriteAllLines(filePath, filenames);
        }

        private void ProcessLine(string line)
        {
            string[] fields = line.Split(',');
            uint offset = Convert.ToUInt32(fields[0], 16);
            int size = Convert.ToInt32(fields[1], 16);

            string file = SearchFile(rom, offset, size);
            if (!string.IsNullOrEmpty(file) && !filenames.Contains(file))
                filenames.Add(file);
            UpdateProgress();
        }

        private static string SearchFile(FileContainer folder, long offset, int size)
        {
            foreach (var file in folder.Files.Cast<GameFile>())
                if (IsContained(file, offset, size))
                    return file.Path;

            foreach (var subfolder in folder.Folders) {
                var result = SearchFile(subfolder, offset, size);
                if (!string.IsNullOrEmpty(result))
                    return result;
            }

            return string.Empty;
        }

        private static bool IsContained(GameFile file, long offset, int size)
        {
            long endOffset = file.Stream.Position + file.Stream.Length;
            return (offset >= file.Stream.Position) && (offset + size <= endOffset);
        }

        private void UpdateProgress()
        {
            count++;

            int x = Console.CursorLeft;
            int y = Console.CursorTop;
            Console.WriteLine("Analyzed {0:06} entries.{1}",
                count, new String('.', count % 3));
            Console.SetCursorPosition(x, y);   
        }
    }
}

