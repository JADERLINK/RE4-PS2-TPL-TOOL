using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace TPL_PS2_REPACK
{
    public static class TplPalette
    {


        public static void getPalette(int i, ref TplImageHeader header, 
            ref Dictionary<Color, int> mainAllColors, ref Dictionary<Color, int> mipmap1AllColors, ref Dictionary<Color, int> mipmap2AllColors,
            out Color[] finalPalette) 
        {
            Dictionary<Color, int> colors_base = mainAllColors;

            if (header.bitDepth == 0x8)
            {
                if (colors_base.Count > 16)
                {
                    colors_base = colors_base.Take(16).ToDictionary(k => k.Key, v => v.Value);
                }
            }
            else if (header.bitDepth == 0x9)
            {
                if (colors_base.Count > 256)
                {
                    colors_base = colors_base.Take(256).ToDictionary(k => k.Key, v => v.Value);
                }
            }
            else if (header.bitDepth == 0x6)
            {
                colors_base = new Dictionary<Color, int>();
            }
            else
            {
                Console.WriteLine("TPL ID: " + i.ToString("D0") + "_bitDepth: invalid value, setting value to 0x6");
                header.bitDepth = 0x6;
                colors_base = new Dictionary<Color, int>();
            }

            //=== mipmap1

            Dictionary<Color, int> colors_mipmap1 = null;
            if (header.mipmapHeader1 != null)
            {
                colors_mipmap1 = mipmap1AllColors;

                if (header.mipmapHeader1.bitDepth == 0x6)
                {
                    colors_mipmap1 = new Dictionary<Color, int>();
                }
                else if (!(header.mipmapHeader1.bitDepth == 0x8 || header.mipmapHeader1.bitDepth == 0x9 || header.mipmapHeader1.bitDepth == 0x6))
                {
                    Console.WriteLine("TPL ID: " + i.ToString("D0") + "_mipmap1_bitDepth: invalid value, setting value to 0x6");
                    header.mipmapHeader1.bitDepth = 0x6;
                    colors_mipmap1 = new Dictionary<Color, int>();
                }
            }


            //=== mipmap2

            Dictionary<Color, int> colors_mipmap2 = null;
            if (header.mipmapHeader2 != null)
            {
                colors_mipmap2 = mipmap2AllColors;

                if (header.mipmapHeader2.bitDepth == 0x6)
                {
                    colors_mipmap2 = new Dictionary<Color, int>();
                }
                else if (!(header.mipmapHeader2.bitDepth == 0x8 || header.mipmapHeader2.bitDepth == 0x9 || header.mipmapHeader2.bitDepth == 0x6))
                {
                    Console.WriteLine("TPL ID: " + i.ToString("D0") + "_mipmap2_bitDepth: invalid value, setting value to 0x6");
                    header.mipmapHeader2.bitDepth = 0x6;
                    colors_mipmap2 = new Dictionary<Color, int>();
                }
            }


            // verificações com a paleta

            Color[] ColorPalette = new Color[0];

            if (header.mipmapHeader1 != null || header.mipmapHeader2 != null)
            {
                ColorPalette = ColorFix(colors_base, header.bitDepth,
                header.mipmapHeader1 != null, colors_mipmap1, header.mipmapHeader1.bitDepth,
                header.mipmapHeader2 != null, colors_mipmap2, header.mipmapHeader2.bitDepth);
            }
            else if (header.bitDepth == 0x8)
            {
                ColorPalette = new Color[16];
                colors_base.Keys.CopyTo(ColorPalette, 0);
            }
            else if (header.bitDepth == 0x9)
            {
                ColorPalette = new Color[256];
                colors_base.Keys.CopyTo(ColorPalette, 0);
            }
            // if headers[i].bitDepth == 0x6 => ColorPalette = new Color[0];



            finalPalette = ColorPalette;

        }


        private static Color[] ColorFix(Dictionary<Color, int> colors_base, ushort bitDepth,
            bool Asmipmap1, Dictionary<Color, int> colors_mipmap1, ushort mipmap1bitDepth,
            bool Asmipmap2, Dictionary<Color, int> colors_mipmap2, ushort mipmap2bitDepth)
        {
            Color[] ColorPalette = new Color[0];

            // a fazer o codigo
            if (bitDepth == 0x8)
            {
                ColorPalette = new Color[16];
                colors_base.Keys.CopyTo(ColorPalette, 0);
            }
            else if (bitDepth == 0x9)
            {
                ColorPalette = new Color[256];
                colors_base.Keys.CopyTo(ColorPalette, 0);
            }

            return ColorPalette;
        }





        public static byte[] CreatePaletteArryBitDepth0x8(Color[] palette)
        {
            byte[] res = new byte[0x80];

            int cont = 0;
            for (int ic = 0; ic < palette.Length || ic < 16; ic++)
            {
                byte R = palette[ic].R;
                byte G = palette[ic].G;
                byte B = palette[ic].B;
                byte A = (byte)(palette[ic].A * 0x80 / 0xFF);

                res[cont + 0] = R;
                res[cont + 1] = G;
                res[cont + 2] = B;
                res[cont + 3] = A;

                cont += 4;
                if (ic == 7)
                {
                    cont = 0x40;
                }
            }

            return res;
        }


        public static byte[] CreatePaletteArryBitDepth0x9(Color[] palette)
        {
            byte[] res = new byte[0x400];

            int index = 0;
            int swap = 1;
            for (int ic = 0; ic < palette.Length || ic < 256; ic++)
            {
                if (ic % 8 == 0 && ic != 0)
                {
                    if (swap == 1)
                    {
                        index += 8;
                    }
                    else if (swap == 2)
                    {
                        index -= 16;

                    }
                    else if (swap == 3)
                    {
                        index += 8;
                    }
                    else
                    {
                        swap = 0;
                    }
                    swap++;

                }

                int cont = index * 4;

                byte R = palette[ic].R;
                byte G = palette[ic].G;
                byte B = palette[ic].B;
                byte A = (byte)(palette[ic].A * 0x80 / 0xFF);

                res[cont + 0] = R;
                res[cont + 1] = G;
                res[cont + 2] = B;
                res[cont + 3] = A;

                index++;
            }

            return res;
        }


    }
}
