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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Libgame;
using Libgame.IO;
using Nitro.Rom;
using System.Threading;

namespace NitroFilcher
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            const string romPath = "/home/benito/Ninokuni [PATCHED].nds";

            var resolver = new FileResolver(romPath);
            var desmume = new DesmumeProcess(romPath, resolver);
            desmume.Start();

            Thread resolverThread = new Thread(new ThreadStart(resolver.StartProcessing));
            resolverThread.Start();

            desmume.WaitForExit();

            resolver.Stop();
            resolverThread.Join();

            resolver.Export("/home/benito/nino.txt");
        }
    }
}
