using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Th145_palette_editor
{
    class Adler32
    {
        // Adler-32 algorightm, copy-paster from the ZLIB RFC (RFC 1950) and ported to C#.
        const UInt32 BASE = 65521; /* largest prime smaller than 65536 */

        private UInt32 adler;

        public Adler32()
        {
            this.adler = 1;
        }

        public static explicit operator UInt32(Adler32 self)
        {
            return self.adler;
        }

        /*
           Update a running Adler-32 checksum with the bytes buf[0..len-1]
         and return the updated checksum. The Adler-32 checksum should be
         initialized to 1.

         Usage example:

           unsigned long adler = 1L;

           while (read_buffer(buffer, length) != EOF) {
             adler = update_adler32(adler, buffer, length);
           }
           if (adler != original_adler) error();
        */
        private static UInt32 update_adler32(UInt32 adler,
            byte[] buf, int len)
        {
            UInt32 s1 = adler & 0xffff;
            UInt32 s2 = (adler >> 16) & 0xffff;
            int n;

            for (n = 0; n < len; n++)
            {
                s1 = (s1 + buf[n]) % BASE;
                s2 = (s2 + s1) % BASE;
            }
            return (s2 << 16) + s1;
        }

        /* Return the adler32 of the bytes buf[0..len-1] */
        public void update(byte[] buf, int len)
        {
            this.adler = update_adler32(this.adler, buf, len);
        }

    }

    class TFXX
    {
        bool verbose;
        string fn;
        string item_name;
        FileStream fs;
        DeflateStream z;

        public TFXX(bool verbose = true)
        {
            this.verbose = verbose;
        }

        public bool open_read(string fn, string magic, string item_name)
        {
            this.fn = fn;
            this.item_name = item_name;
            fs = new FileStream(fn, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[4];
            fs.Read(buffer, 0, 4);
            if (Encoding.Default.GetString(buffer) != magic)
            {
                if (verbose) System.Windows.MessageBox.Show("Could not load " + item_name + ": " + fn + " is not a valid " + magic + " file.");
                return false;
            }
            fs.Read(buffer, 0, 1);
            if (buffer[0] != 0)
            {
                if (verbose) System.Windows.MessageBox.Show("Could not load " + item_name + " " + fn + ": invalid version number.");
                return false;
            }
            return true;
        }

        public byte read_byte()
        {
            return (byte)fs.ReadByte();
        }

        public UInt32 read_dword()
        {
            byte[] buffer = new byte[4];
            fs.Read(buffer, 0, 4);
            return BitConverter.ToUInt32(buffer, 0);
        }

        public bool decomp_init(UInt32 comp_size)
        {
            byte[] comp_buffer = new byte[comp_size];
            if (fs.Read(comp_buffer, 0, (int)comp_size) != comp_size)
            {
                if (verbose) System.Windows.MessageBox.Show("Could not load " + this.item_name + " " + this.fn + ": file truncated or invalid header.");
                return false;
            }

            MemoryStream ms = new MemoryStream(comp_buffer);
            ms.Seek(2, SeekOrigin.Begin);

            z = new DeflateStream(ms, CompressionMode.Decompress);
            return true;
        }

        public bool decomp_skip(int count)
        {
            return z.Read(new byte[count], 0, count) == count;
        }

        public int decomp_read(byte[] array, int count)
        {
            return z.Read(array, 0, count);
        }



        MemoryStream comp_ms;
        Adler32 adler32;
        byte[] result;
        public bool open_write(string fn, string magic, string item_name)
        {
            this.fn = fn;
            this.item_name = item_name;
            this.fs = new FileStream(fn, FileMode.Create, FileAccess.Write);
            byte[] buffer = Encoding.Default.GetBytes(magic);
            fs.Write(buffer, 0, 4);
            buffer[0] = 0;
            fs.Write(buffer, 0, 1);

            // Init compression state
            comp_ms = new MemoryStream();
            z = new DeflateStream(comp_ms, CompressionLevel.Optimal, true);
            adler32 = new Adler32();
            return true;
        }

        public void write_byte(byte b)
        {
            fs.WriteByte(b);
        }

        public void write_dword(UInt32 n)
        {
            fs.Write(BitConverter.GetBytes(n), 0, 4);
        }

        public void write_dword_BE(UInt32 n)
        {
            byte[] array = BitConverter.GetBytes(n);
            Array.Reverse(array);
            fs.Write(array, 0, 4);
        }

        public void comp_write(byte[] array, int count)
        {
            z.Write(array, 0, count);
            adler32.update(array, count);
        }

        // After calling this function, you should stop calling comp_write
        public void comp_write_size()
        {
            z.Dispose();
            z = null;
            result = comp_ms.ToArray();
            comp_ms.Dispose();
            comp_ms = null;
            write_dword((UInt32)(result.Length + 6)); // Add space for the ZLIB header and footer
        }

        public void comp_finalize()
        {
            write_byte(0x78);
            write_byte(0xDA);
            fs.Write(result, 0, result.Length);
            write_dword_BE((UInt32)adler32);
            fs.Dispose();
            fs = null;
            adler32 = null;
        }
    }
}
