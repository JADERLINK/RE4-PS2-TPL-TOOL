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

            if (header.BitDepth == 0x8)
            {
                if (colors_base.Count > 16)
                {
                    colors_base = colors_base.Take(16).ToDictionary(k => k.Key, v => v.Value);
                }
            }
            else if (header.BitDepth == 0x9)
            {
                if (colors_base.Count > 256)
                {
                    colors_base = colors_base.Take(256).ToDictionary(k => k.Key, v => v.Value);
                }
            }
            else if (header.BitDepth == 0x6)
            {
                colors_base = new Dictionary<Color, int>();
            }
            else
            {
                Console.WriteLine("TPL ID: " + i.ToString("D0") + "_bitDepth: invalid value, setting value to 0x6");
                header.BitDepth = 0x6;
                colors_base = new Dictionary<Color, int>();
            }

            //=== mipmap1

            Dictionary<Color, int> colors_mipmap1 = null;
            if (header.MipmapHeader1 != null)
            {
                colors_mipmap1 = mipmap1AllColors;

                if (header.MipmapHeader1.BitDepth == 0x6)
                {
                    colors_mipmap1 = new Dictionary<Color, int>();
                }
                else if (!(header.MipmapHeader1.BitDepth == 0x8 || header.MipmapHeader1.BitDepth == 0x9 || header.MipmapHeader1.BitDepth == 0x6))
                {
                    Console.WriteLine("TPL ID: " + i.ToString("D0") + "_mipmap1_bitDepth: invalid value, setting value to 0x6");
                    header.MipmapHeader1.BitDepth = 0x6;
                    colors_mipmap1 = new Dictionary<Color, int>();
                }
            }


            //=== mipmap2

            Dictionary<Color, int> colors_mipmap2 = null;
            if (header.MipmapHeader2 != null)
            {
                colors_mipmap2 = mipmap2AllColors;

                if (header.MipmapHeader2.BitDepth == 0x6)
                {
                    colors_mipmap2 = new Dictionary<Color, int>();
                }
                else if (!(header.MipmapHeader2.BitDepth == 0x8 || header.MipmapHeader2.BitDepth == 0x9 || header.MipmapHeader2.BitDepth == 0x6))
                {
                    Console.WriteLine("TPL ID: " + i.ToString("D0") + "_mipmap2_bitDepth: invalid value, setting value to 0x6");
                    header.MipmapHeader2.BitDepth = 0x6;
                    colors_mipmap2 = new Dictionary<Color, int>();
                }
            }


            // verificações com a paleta

            Color[] ColorPalette = new Color[0];

            if (header.MipmapHeader1 != null || header.MipmapHeader2 != null)
            {
                ColorPalette = ColorFix(mainAllColors, header.BitDepth,
                header.MipmapHeader1 != null, colors_mipmap1, header.MipmapHeader1.BitDepth,
                header.MipmapHeader2 != null, colors_mipmap2, header.MipmapHeader2.BitDepth);
            }
            else if (header.BitDepth == 0x8)
            {
                ColorPalette = new Color[16];
                colors_base.Keys.CopyTo(ColorPalette, 0);
            }
            else if (header.BitDepth == 0x9)
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

            Dictionary<Color, int> temp = new Dictionary<Color, int>();
            foreach (var item in colors_base)
            {
                temp.Add(item.Key, item.Value);
            }
            if (Asmipmap1)
            {
                foreach (var item in colors_mipmap1)
                {
                    if (temp.ContainsKey(item.Key))
                    {
                        temp[item.Key] += item.Value;
                    }
                    else 
                    {
                        temp.Add(item.Key, item.Value);
                    }
                }
            }

            if (Asmipmap2)
            {
                foreach (var item in colors_mipmap2)
                {
                    if (temp.ContainsKey(item.Key))
                    {
                        temp[item.Key] += item.Value;
                    }
                    else
                    {
                        temp.Add(item.Key, item.Value);
                    }
                }
            }

            temp = (from obj in temp
                    orderby obj.Value
                    select obj).ToDictionary(k => k.Key, v => v.Value);

            //-------
            ushort usebitDepth = bitDepth;

            if (mipmap1bitDepth > usebitDepth)
            {
                usebitDepth = mipmap1bitDepth;
            }
            if (mipmap2bitDepth > usebitDepth)
            {
                usebitDepth = mipmap2bitDepth;
            }

            //--------

            if (usebitDepth == 0x8)
            {
                ColorPalette = new Color[16];
                if (temp.Count > 16)
                {
                    temp = temp.Take(16).ToDictionary(k => k.Key, v => v.Value);
                }
                temp.Keys.CopyTo(ColorPalette, 0);
            }
            else if (usebitDepth == 0x9)
            {
                ColorPalette = new Color[256];
                if (temp.Count > 256)
                {
                    temp = temp.Take(256).ToDictionary(k => k.Key, v => v.Value);
                }
                temp.Keys.CopyTo(ColorPalette, 0);
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
