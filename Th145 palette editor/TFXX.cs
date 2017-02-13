using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Th145_palette_editor
{
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

        public void comp_write(byte[] array, int count)
        {
            z.Write(array, 0, count);
        }

        // After calling this function, you should stop calling comp_write
        public void comp_write_size()
        {
            z.Dispose();
            z = null;
            result = comp_ms.ToArray();
            comp_ms.Dispose();
            comp_ms = null;
            write_dword((UInt32)result.Length);
        }

        public void comp_finalize()
        {
            fs.WriteByte(0x78);
            fs.WriteByte(0xDA);
            fs.Write(result, 0, result.Length);
            fs.Dispose();
            fs = null;
        }
    }
}
