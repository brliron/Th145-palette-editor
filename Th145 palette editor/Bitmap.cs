using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Th145_palette_editor
{
    class Bitmap
    {
        byte[] bytes;
        System.Drawing.Bitmap bmp;
        GCHandle pinnedArray;

        public int width  { get { return this.bmp.Width;  } }
        public int height { get { return this.bmp.Height; } }

        public byte getPixel(int x, int y)
        {
            return bytes[y * width + x];
        }

        public Bitmap(string fn, bool verbose = true)
        {
            // If the loading failed, this.bmp will contain null.
            this.bmp = null;

            TFXX tfbm = new TFXX(verbose);
            if (tfbm.open_read(fn, "TFBM", "image") == false)
                return;
            byte bits = tfbm.read_byte();
            UInt32 width = tfbm.read_dword();
            UInt32 height = tfbm.read_dword();
            UInt32 padding_width = tfbm.read_dword();
            UInt32 comp_size = tfbm.read_dword();

            if (bits != 8)
            {
                if (verbose) MessageBox.Show("Could not load image: only 8-bits images are supported.");
                return;
            }
            tfbm.decomp_init(comp_size);

            this.bytes = new byte[width * height];
            if (tfbm.decomp_read(bytes, (int)(width * height)) != (int)(width * height))
            {
                if (verbose) MessageBox.Show("Could not load image: decompression error.");
                return;
            }

            pinnedArray = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            IntPtr pointer = pinnedArray.AddrOfPinnedObject();
            this.bmp = new System.Drawing.Bitmap((int)width, (int)height, (int)padding_width, PixelFormat.Format8bppIndexed, pointer);
        }

        ~Bitmap()
        {
            if (this.bmp != null)
                pinnedArray.Free();
        }

        public void setPalette(Palette palette)
        {
            ColorPalette pal = bmp.Palette;
            for (int i = 0; i < pal.Entries.Length; i++)
                pal.Entries[i] = palette.list[i];
            bmp.Palette = pal;
        }

        public System.Windows.Media.Imaging.BitmapSource toBitmapSource()
        {
            return StaticStuff.ToBitmapSource(this.bmp);
        }

        public static implicit operator bool(Bitmap bmp)
        {
            return bmp.bmp != null;
        }
    }
}
