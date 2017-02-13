using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;

namespace Th145_palette_editor
{
    class Palette
    {
        public List<MyColor> list;

        public Palette(string fn)
        {
            TFXX tfpa = new TFXX();
            tfpa.open_read(fn, "TFPA", "palette");
            this.list = new List<MyColor>(256);
            UInt32 comp_size = tfpa.read_dword();
            tfpa.decomp_init(comp_size);

            // Ignore the BGRA5551 palette
            if (tfpa.decomp_skip(0x200) == false) // Sometimes, the 1st SkipBytes fails, I don't know why.
                tfpa.decomp_skip(0x200);

            // Read the BGRA8888 palette
            byte[] c = new byte[4];
            for (int i = 0; i < 256; i++)
            {
                tfpa.decomp_read(c, 4);
                list.Add(new MyColor(Color.FromArgb(c[3], c[2], c[1], c[0])));
            }
        }

        public void update(int i, System.Windows.Media.Color c)
        {
            list[i] = new MyColor(c);
        }

        public void save(string fn)
        {
            TFXX tfpa = new TFXX();
            tfpa.open_write(fn, "TFPA", "palette");

            // Write the ARGB5551 LE palette
            for (int i = 0; i < 256; i++)
            {
                int a = (list[i].dval.A > 0x20) ? 1 : 0;
                int r = list[i].dval.R * 32 / 256;
                int g = list[i].dval.G * 32 / 256;
                int b = list[i].dval.B * 32 / 256;
                UInt16 c = (UInt16)((a << 15) + (r << 10) + (g << 5) + b);
                tfpa.comp_write(BitConverter.GetBytes(c), 2);
            }

            // Write the BGRA8888 palette
            for (int i = 0; i < 256; i++)
            {
                byte[] c = new byte[4];
                c[0] = list[i].dval.B;
                c[1] = list[i].dval.G;
                c[2] = list[i].dval.R;
                c[3] = list[i].dval.A;
                tfpa.comp_write(c, 4);
            }

            tfpa.comp_write_size();
            tfpa.comp_finalize();
        }
    }
}
