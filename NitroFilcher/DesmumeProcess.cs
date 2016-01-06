//
//  DesmumeProcess.cs
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
using System.Diagnostics;
using System.Reflection;
using System.IO;

namespace NitroFilcher
{
    public class DesmumeProcess
    {
        private const string DefaultProcessName = "desmume_nitrofilcher.exe";

        private readonly FileResolver resolver;
        private readonly string gamePath;
        private Process desmume;

        public DesmumeProcess(string gamePath, FileResolver resolver)
        {
            this.resolver = resolver;
            this.gamePath = gamePath;
            ProcessName = DefaultProcessName;
        }

        public string ProcessName {
            get;
            set;
        }

        public bool Start()
        {
            desmume = new Process();

            // Set the program and argument
            string cwd = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            desmume.StartInfo.FileName = Path.Combine(cwd, ProcessName);
            desmume.StartInfo.Arguments = "\"" + gamePath + "\"";
            desmume.StartInfo.WorkingDirectory = cwd;

            // Show error dialog if the process fail to start
            desmume.StartInfo.ErrorDialog = true;

            // Redirect the output
            desmume.StartInfo.UseShellExecute = false;
            desmume.StartInfo.RedirectStandardOutput = true;
            desmume.OutputDataReceived += ProcessOutput;

            // Start the process and the output reading
            bool result = desmume.Start();
            desmume.BeginOutputReadLine();

            return result;
        }

        public void WaitForExit()
        {
            desmume.WaitForExit();
        }

        private void ProcessOutput(object sender, DataReceivedEventArgs e)
        {
            const string Prefix = "NITROFILCHER: ";
            if (e.Data != null && e.Data.StartsWith(Prefix))
                resolver.Enqueue(e.Data.Substring(Prefix.Length));
        }
    }
}

