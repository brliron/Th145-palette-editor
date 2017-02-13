using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Th145_palette_editor
{
    class MyColor
    {
        public System.Drawing.Color dval;
        private System.Windows.Media.Color _wval;
        public System.Windows.Media.Color wval
        {
            get
            {
                return _wval;
            }
            set
            {
                _wval = value;
                dval = System.Drawing.Color.FromArgb((value.A << 24) + (value.R << 16) + (value.G << 8) + value.B);
            }
        }

        public MyColor(System.Drawing.Color color)
        {
            this.dval = color;
            this._wval = System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public MyColor(System.Windows.Media.Color color)
        {
            this._wval = color;
            this.dval = System.Drawing.Color.FromArgb((color.A >> 24) + (color.R >> 16) + (color.G >> 8) + color.B);
        }

        public static implicit operator System.Drawing.Color(MyColor c)
        {
            return c.dval;
        }

        public static implicit operator System.Windows.Media.Color(MyColor c)
        {
            return c._wval;
        }
    }
}
