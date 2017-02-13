using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Th145_palette_editor
{
    class Character
    {
        bool loaded;
        string path;

        public string name { get; set; }

        public List<string> pltNames;
        public List<string> bmpNames;

        Dictionary<string, Palette> palettes;
        Dictionary<string, Bitmap> bitmaps;

        public Palette selectedPalette;
        public Bitmap selectedBitmap;

        public Character(string name, string path)
        {
            this.name = name;
            this.path = path;
            this.loaded = false;
        }

        // Returns the name of the loaded bitmap.
        // The loaded palette is always this.pltNames[0].
        public void load()
        {
            if (this.loaded)
                return;

            // Load palettes
            pltNames = new List<string>();
            palettes = new Dictionary<string, Palette>();
            foreach (string f in Directory.EnumerateFiles(path, "palette*.bmp"))
            {
                string name = f.Substring(f.LastIndexOf('\\') + 1);
                pltNames.Add(name);
                palettes[name] = new Palette(f);
            }
            pltNames.Sort();
            pltNames.Add("Load palette file...");

            // Load BMP files. Ignore errors.
            bmpNames = new List<string>();
            bitmaps = new Dictionary<string, Bitmap>();
            foreach (string f in Directory.EnumerateFiles(path, "*.bmp"))
            {
                string name = f.Substring(f.LastIndexOf('\\') + 1);
                Bitmap bmp = new Bitmap(f, false);
                if (bmp.bmp != null)
                {
                    bmpNames.Add(name);
                    bitmaps[name] = bmp;
                }
            }
            bmpNames.Sort();

            //bmp = new Bitmap(game_path + @"\th145\data\actor\" + name + "\\stand0000.bmp");
            //palette = new Palette("palette000.tfpa"); Keeping the wrong one, because it is so funny
            //palette = new Palette(game_path + @"\th145\data\actor\" + name + "\\palette000.bmp");

            this.loaded = true;
        }

        public string getDefaultBmp()
        {
            if (bmpNames.Contains("stand0000.bmp"))
                return "stand0000.bmp";
            else if (bmpNames.Contains("test_stand0000.bmp"))
                return "test_stand0000.bmp";
            else
            {
                Console.WriteLine("No stand found for " + name);
                return bmpNames[0];
            }
        }

        public void selectBitmap(string name)
        {
            selectedBitmap = bitmaps[name];
            if (selectedBitmap != null && selectedPalette != null)
                selectedBitmap.setPalette(selectedPalette);
        }

        // Returns true if a new palette is created
        public bool selectPalette(string name)
        {
            bool created = false;

            if (name == pltNames.Last())
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = "Palette file (*.bmp)|*.bmp|All files (*.*)|*.*";
                if (dlg.ShowDialog() != true)
                    return false;
                name = dlg.FileName.Substring(dlg.FileName.LastIndexOf('\\') + 1);
                pltNames.Insert(pltNames.Count - 1, name);
                palettes[name] = new Palette(dlg.FileName);
                created = true;
            }
            selectedPalette = palettes[name];
            if (selectedBitmap != null && selectedPalette != null)
                selectedBitmap.setPalette(selectedPalette);
            return created;
        }
    }
}
