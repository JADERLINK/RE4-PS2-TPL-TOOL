using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

namespace TPL_PS2_REPACK
{
    public class BitmapManager
    {
        private bool flipY = false;
        private bool rotateInterlace1and3 = false;
        private string parentDirectory = "";

        public BitmapManager(bool flipY, bool rotateInterlace1and3, string parentDirectory)
        {
            this.flipY = flipY;
            this.rotateInterlace1and3 = rotateInterlace1and3;
            this.parentDirectory = parentDirectory;
        }

        public void GetBitmapContent(ref TplImageHeader header, out Dictionary<Color, int> allColors, out SimpleBitmap simpleBitmap)
        {
            Bitmap bitmap = null;
            LoadBitmap(parentDirectory + header.texturePath, out bitmap);

            if (flipY)
            {
                bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            }

            if (rotateInterlace1and3 && (header.interlace == 0x1 || header.interlace == 0x3))
            {
                bitmap.RotateFlip(RotateFlipType.Rotate90FlipX);
            }

            header.width = (ushort)bitmap.Width;
            header.height = (ushort)bitmap.Height;

            GetContent(ref bitmap, out allColors, out simpleBitmap);
        }


        private static bool LoadBitmap(string filepath, out Bitmap bitmap)
        {
            bitmap = null;
            if (!File.Exists(filepath))
            {
                // "O arquivo: " + filepath + "não existe, sera usado uma imagem 2x2"
                Console.WriteLine("The file: " + filepath + "does not exist, a 2x2 image will be used!");
                bitmap = new Bitmap(2, 2);
                return false;
            }

            try
            {
                FileInfo info = new FileInfo(filepath);
                string Extension = info.Extension.ToUpperInvariant().Replace(".", "");

                if (Extension == "TGA")
                {
                    var tga = new TGASharpLib.TGA(filepath);
                    bitmap = tga.ToBitmap();
                }
                else
                {
                    bitmap = new Bitmap(filepath);
                }

            }
            catch (Exception ex)
            {
                // "erro ao carregar o arquivo: " + filepath + "no lugar sera usado uma imagem 2x2," + Environment.NewLine + ex.Message
                Console.WriteLine("error loading file: " + filepath + "a 2x2 image will be used instead," + Environment.NewLine + ex.Message);
                bitmap = new Bitmap(2, 2);
                return false;
            }

            return true;
        }

        private static void GetContent(ref Bitmap bitmap, out Dictionary<Color, int> allColor, out SimpleBitmap simpleBitmap)
        {
            int Width = bitmap.Width;
            int Height = bitmap.Height;

            Dictionary<Color, int> colors = new Dictionary<Color, int>();
            SimpleBitmap sb = new SimpleBitmap(Width, Height);

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Color color = bitmap.GetPixel(x, y);
                    sb.SetPixel(x, y, color);
                    if (colors.ContainsKey(color))
                    {
                        colors[color]++;
                    }
                    else
                    {
                        colors.Add(color, 1);
                    }
                }
            }

            simpleBitmap = sb;
            allColor = (from obj in colors
                        orderby obj.Value
                        select obj).ToDictionary(k => k.Key, v => v.Value);

        }

    }


    public class SimpleBitmap 
    {
        public int Width { get => width; }
        public int Height { get => height; }
        private int width = 0;
        private int height = 0;
        private Color[,] bitmap;

        public SimpleBitmap(int width, int height)
        {
            this.width = width;
            this.height = height;
            bitmap = new Color[width, height];
        }

        public Color GetPixel(int x, int y) 
        {
            return bitmap[x, y];
        }

        public void SetPixel(int x, int y, Color color)
        {
            bitmap[x, y] = color;
        }
    }


}
