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
        private string ImageFolderDirectory = "";

        public BitmapManager(bool flipY, string ImageFolderDirectory)
        {
            this.flipY = flipY;
            this.ImageFolderDirectory = ImageFolderDirectory;
        }

        public void GetBitmapContent(string imageName, ref TplImageHeader header, out Dictionary<Color, int> allColors, out SimpleBitmap simpleBitmap)
        {
            Bitmap bitmap = null;
            LoadBitmap(ImageFolderDirectory, imageName, out bitmap);

            if (flipY)
            {
                bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            }

            if (bitmap.Height > bitmap.Width
                || (bitmap.Height == bitmap.Width && (header.Interlace == 0x1 || header.Interlace == 0x3))) //(header.Interlace == 0x1 || header.Interlace == 0x3)
            {
                bitmap.RotateFlip(RotateFlipType.Rotate90FlipX);
            }

            if ((header.Width != bitmap.Width) // && header.Width != bitmap.Height
                || (header.Height != bitmap.Height)) // && header.Height != bitmap.Width
            {
                Console.WriteLine("In the image: \"" + imageName +"\" The dimensions of the image are different from those of the .idxtplheader file:" + Environment.NewLine +
                    "Image: " + bitmap.Width + "x" + bitmap.Height + "   Header: " + header.Width + "x" + header.Height);
            }

            GetContent(ref bitmap, out allColors, out simpleBitmap);
        }


        private static bool LoadBitmap(string ImageFolderDirectory, string imageName, out Bitmap bitmap)
        {
            string filepath = ImageFolderDirectory + imageName;

            bitmap = null;
            if (!File.Exists(filepath))
            {
                Console.WriteLine("The file: \"" + imageName + "\" Does not exist, a 2x2 image will be used!");
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
                Console.WriteLine("Error loading file: \"" + imageName + "\" A 2x2 image will be used instead," + Environment.NewLine + ex.Message);
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
