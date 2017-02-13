using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Th145_palette_editor
{
    class TFPK
    {
        public string game_path;
        public string pak_path;
        public string extracted_path;
        public string pak_name;

        public TFPK(string game_path, string pak_file)
        {
            this.game_path = game_path;
            this.pak_path = Path.Combine(game_path, pak_file);
            this.pak_name = pak_file.Substring(0, pak_file.LastIndexOf('.'));
            this.extracted_path = Path.Combine(game_path, pak_name);
        }

        public bool IsExtracted
        {
            get
            {
                return Directory.Exists(extracted_path);
            }
        }

        public bool Exists
        {
            get
            {
                return File.Exists(pak_path);
            }
        }

        public bool ContainsDirectory(string dir)
        {
            return Directory.Exists(Path.Combine(extracted_path, dir));
        }

        public bool ContainsFile(string file)
        {
            return File.Exists(Path.Combine(extracted_path, file));
        }

        public bool Extract()
        {
            Process process = new Process();
            process.StartInfo.FileName = @".\th145arc.exe";
            process.StartInfo.Arguments = "/x \"" + pak_path + '"';
            process.StartInfo.UseShellExecute = false;
            process.Start();
            process.WaitForExit();
            return true;
        }

        public bool Repack()
        {
            Process process = new Process();
            process.StartInfo.FileName = @".\th145arc.exe";
            process.StartInfo.Arguments = "/p \"" + extracted_path + '"';
            process.StartInfo.UseShellExecute = false;
            process.Start();
            process.WaitForExit();
            return true;
        }
    }
}
