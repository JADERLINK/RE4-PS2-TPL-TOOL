using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

namespace TPL_PS2_REPACK
{

    public static class TplIndexes
    {
    
        public static bool GetImageBytesArry(ushort bitDepth, ushort interlace, ref SimpleBitmap bitmap, ref Color[] colors, out byte[] indexesArry)
        {

            if (bitDepth == 0x8 && (interlace == 0x0 || interlace == 0x1))
            {
                return bitDepth8interlace0and1(bitDepth, interlace, ref bitmap, ref colors, out indexesArry);
            }
            else if (bitDepth == 0x9 && (interlace == 0x0 || interlace == 0x1))
            {
                return bitDepth9interlace0and1(bitDepth, interlace, ref bitmap, ref colors, out indexesArry);
            }
            else if (bitDepth == 0x6 && (interlace == 0x0 || interlace == 0x1))
            {
                return bitDepth6interlace0and1(bitDepth, interlace, ref bitmap, ref colors, out indexesArry);
            }
            else if (bitDepth == 0x8 && (interlace == 0x2 || interlace == 0x3))
            {
                return bitDepth8interlace2and3(bitDepth, interlace, ref bitmap, ref colors, out indexesArry);
            }
            else if (bitDepth == 0x9 && (interlace == 0x2 || interlace == 0x3))
            {
                return bitDepth9interlace2and3(bitDepth, interlace, ref bitmap, ref colors, out indexesArry);
            }
            else if (bitDepth == 0x6 && (interlace == 0x2 || interlace == 0x3))
            {
                //não suportado atualmente
            }
            else
            {
                //outros casos não existem
            }

            indexesArry = new byte[0];
            return false;
        }

        private static bool bitDepth8interlace0and1(ushort bitDepth, ushort interlace, ref SimpleBitmap bitmap, ref Color[] colors, out byte[] indexesArry)
        {
     
            int width = bitmap.Width;
            int height = bitmap.Height;

            int indexesbytesCount = (height * width) / 2;

            byte[] indexes = new byte[indexesbytesCount];

            int Xcont = 0;
            int Ycont = 0;
            for (int IN = 0; IN < indexes.Length; IN++)
            {
                Color c1 = bitmap.GetPixel(Xcont, Ycont);
                Color c2 = bitmap.GetPixel(Xcont + 1, Ycont);

                byte index1 = 0;
                if (colors.Contains(c1))
                {
                    index1 = (byte)Array.IndexOf(colors, c1);
                }
                byte index2 = 0;
                if (colors.Contains(c2))
                {
                    index2 = (byte)Array.IndexOf(colors, c2);
                }


                byte left = (byte)(index1 << 4);
                byte[] arr = new byte[2] { left, index2 };
                ushort us = BitConverter.ToUInt16(arr, 0);
                byte Endindex = (byte)(us >> 4);


                indexes[IN] = Endindex;

                Xcont += 2;
                if (Xcont >= width)
                {
                    Xcont = 0;
                    Ycont += 1;
                }
            }


            indexesArry = indexes;
            return true;

        }

        private static bool bitDepth9interlace0and1(ushort bitDepth, ushort interlace, ref SimpleBitmap bitmap, ref Color[] colors, out byte[] indexesArry)
        {
           
            int width = bitmap.Width;
            int height = bitmap.Height;

            int indexesbytesCount = (height * width);

            byte[] indexes = new byte[indexesbytesCount];

            int Xcont = 0;
            int Ycont = 0;
            for (int IN = 0; IN < indexes.Length; IN++)
            {
                Color c = bitmap.GetPixel(Xcont, Ycont);
                byte index = 0;
                if (colors.Contains(c))
                {
                    index = (byte)Array.IndexOf(colors, c);
                }

                indexes[IN] = index;

                Xcont += 1;
                if (Xcont >= width)
                {
                    Xcont = 0;
                    Ycont += 1;
                }
            }

            indexesArry = indexes;

            return true;
        }

        private static bool bitDepth6interlace0and1(ushort bitDepth, ushort interlace, ref SimpleBitmap bitmap, ref Color[] colors, out byte[] indexesArry)
        {

            int width = bitmap.Width;
            int height = bitmap.Height;

            int bytesCount = (height * width) * 4;

            byte[] ColorBytes = new byte[bytesCount];


            int Xcont = 0;
            int Ycont = 0;
            int lenght = height * width;
            int cont = 0;
            for (int IN = 0; IN < lenght; IN++)
            {
                Color c = bitmap.GetPixel(Xcont, Ycont);

                ColorBytes[cont + 0] = c.R;
                ColorBytes[cont + 1] = c.G;
                ColorBytes[cont + 2] = c.B;
                ColorBytes[cont + 3] = c.A;
                cont += 4;

                Xcont++;
                if (Xcont >= width)
                {
                    Xcont = 0;
                    Ycont += 1;
                }
            }

            indexesArry = ColorBytes;
            return true;
        }

        private static bool bitDepth8interlace2and3(ushort bitDepth, ushort interlace, ref SimpleBitmap bitmap, ref Color[] colors, out byte[] indexesArry)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;

            int indexesbytesCount = (height * width) / 2;

            byte[] indexes = new byte[indexesbytesCount];


            // correção da imagem baseado em gambiarra
            SimpleBitmap copia = bitmap;


            if (width > 128 || height > 128)
            {
                SimpleBitmap bitmapFix = new SimpleBitmap(height, width);

                int blockAmounts = (width / 128) * (height / 128);

                int copyX = 0;
                int copyY = 0;
                int setX = 0;
                int setY = 0;

                for (int iB = 0; iB < blockAmounts; iB++)
                {
                    for (int y = 0; y < 128; y++)
                    {
                        for (int x = 0; x < 128; x++)
                        {
                            Color colorGet = bitmap.GetPixel(x + copyX, y + copyY);
                            bitmapFix.SetPixel(x + setX, y + setY, colorGet);

                        }
                    }

                    setX += 128;
                    if (setX >= height)
                    {
                        setX = 0;
                        setY += 128;
                    }

                    copyY += 128;
                    if (copyY >= height)
                    {
                        copyY = 0;
                        copyX += 128;
                    }

                }

                bitmap = bitmapFix;

            }



            // no comentario esta a imagem na posição correta
            //bitmap = new Bitmap(width, height);
            //bitmap = new Bitmap(height, width);

            //Notas
            // a cada 32 bytes é uma nova sequencia
            // parede de 16 bytes, sendo 32 pixel
            // sendo 2 linhas


            int Xcont = 0;
            int Ycont = 0;

            int altCont = 0;
            int niveis = 0;

            //int nivelMax = (height / 16); // o correto é ser height
            int nivelMax = (width / 16);

            bool flipInX = false;

            for (int IN = 0; IN < indexes.Length; IN += 32)
            {

                preenche8(ref bitmap, Xcont, Ycont, ref colors, ref indexes, IN, flipInX);

                Ycont += 16;
                niveis += 1;

                if (niveis == nivelMax)
                {
                    niveis = 0;
                    altCont += 1;
                    if (altCont % 2 == 0)
                    {
                        altCont += 2;

                        if (flipInX)
                        {
                            flipInX = false;
                        }
                        else
                        {
                            flipInX = true;
                        }
                    }
                    Ycont = altCont;

                    if (altCont == 16)
                    {
                        flipInX = false;
                        Ycont = 0;
                        altCont = 0;
                        niveis = 0;
                        Xcont += 32;

                    }
                }

            }


            bitmap = copia;

            indexesArry = indexes;

            return true;
        }

        private static bool bitDepth9interlace2and3(ushort bitDepth, ushort interlace, ref SimpleBitmap bitmap, ref Color[] colors, out byte[] indexesArry)
        {
        
            int width = bitmap.Width;
            int height = bitmap.Height;


            int indexesbytesCount = (height * width);


            byte[] indexes = new byte[indexesbytesCount];

            int Xcont = 0;
            int Ycont = 0;
            bool flipInX = false;
            for (int IN = 0; IN < indexes.Length; IN += 32)
            {
                preenche9(ref bitmap, Xcont, Ycont, ref colors, ref indexes, IN, flipInX);

                Xcont += 16;

                if (Xcont >= width)
                {
                    Xcont = 0;
                    Ycont += 1;
                    if (Ycont % 2 == 0)
                    {
                        Ycont += 2;

                        if (flipInX)
                        {
                            flipInX = false;
                        }
                        else
                        {
                            flipInX = true;
                        }

                    }

                }

            }


            indexesArry = indexes;
            return true;
        }


        private static void preenche9(ref SimpleBitmap bitmap, int Xcont, int Ycont, ref Color[] colors, ref byte[] indexes, int IN, bool flipInX)
        {
            if (flipInX == false)
            {
                indexes[IN + 0x0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x0, Ycont + 0));
                indexes[IN + 0x1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x4, Ycont + 2));
                indexes[IN + 0x2] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x8, Ycont + 0));
                indexes[IN + 0x3] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0xC, Ycont + 2));

                indexes[IN + 0x4] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x1, Ycont + 0));
                indexes[IN + 0x5] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x5, Ycont + 2));
                indexes[IN + 0x6] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x9, Ycont + 0));
                indexes[IN + 0x7] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0xD, Ycont + 2));

                indexes[IN + 0x8] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x2, Ycont + 0));
                indexes[IN + 0x9] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x6, Ycont + 2));
                indexes[IN + 0xA] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0xA, Ycont + 0));
                indexes[IN + 0xB] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0xE, Ycont + 2));

                indexes[IN + 0xC] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x3, Ycont + 0));
                indexes[IN + 0xD] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x7, Ycont + 2));
                indexes[IN + 0xE] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0xB, Ycont + 0));
                indexes[IN + 0xF] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0xF, Ycont + 2));

                indexes[IN + 0x10] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x4, Ycont + 0));
                indexes[IN + 0x11] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x0, Ycont + 2));
                indexes[IN + 0x12] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0xC, Ycont + 0));
                indexes[IN + 0x13] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x8, Ycont + 2));

                indexes[IN + 0x14] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x5, Ycont + 0));
                indexes[IN + 0x15] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x1, Ycont + 2));
                indexes[IN + 0x16] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0xD, Ycont + 0));
                indexes[IN + 0x17] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x9, Ycont + 2));

                indexes[IN + 0x18] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x6, Ycont + 0));
                indexes[IN + 0x19] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x2, Ycont + 2));
                indexes[IN + 0x1A] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0xE, Ycont + 0));
                indexes[IN + 0x1B] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0xA, Ycont + 2));

                indexes[IN + 0x1C] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x7, Ycont + 0));
                indexes[IN + 0x1D] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x3, Ycont + 2));
                indexes[IN + 0x1E] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0xF, Ycont + 0));
                indexes[IN + 0x1F] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0xB, Ycont + 2));
            }
            else
            {
                indexes[IN + 0x0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x4, Ycont + 0));
                indexes[IN + 0x1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x0, Ycont + 2));
                indexes[IN + 0x2] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0xC, Ycont + 0));
                indexes[IN + 0x3] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x8, Ycont + 2));

                indexes[IN + 0x4] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x5, Ycont + 0));
                indexes[IN + 0x5] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x1, Ycont + 2));
                indexes[IN + 0x6] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0xD, Ycont + 0));
                indexes[IN + 0x7] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x9, Ycont + 2));

                indexes[IN + 0x8] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x6, Ycont + 0));
                indexes[IN + 0x9] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x2, Ycont + 2));
                indexes[IN + 0xA] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0xE, Ycont + 0));
                indexes[IN + 0xB] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0xA, Ycont + 2));

                indexes[IN + 0xC] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x7, Ycont + 0));
                indexes[IN + 0xD] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x3, Ycont + 2));
                indexes[IN + 0xE] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0xF, Ycont + 0));
                indexes[IN + 0xF] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0xB, Ycont + 2));

                indexes[IN + 0x10] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x0, Ycont + 0));
                indexes[IN + 0x11] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x4, Ycont + 2));
                indexes[IN + 0x12] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x8, Ycont + 0));
                indexes[IN + 0x13] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0xC, Ycont + 2));

                indexes[IN + 0x14] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x1, Ycont + 0));
                indexes[IN + 0x15] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x5, Ycont + 2));
                indexes[IN + 0x16] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x9, Ycont + 0));
                indexes[IN + 0x17] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0xD, Ycont + 2));

                indexes[IN + 0x18] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x2, Ycont + 0));
                indexes[IN + 0x19] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x6, Ycont + 2));
                indexes[IN + 0x1A] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0xA, Ycont + 0));
                indexes[IN + 0x1B] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0xE, Ycont + 2));

                indexes[IN + 0x1C] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x3, Ycont + 0));
                indexes[IN + 0x1D] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x7, Ycont + 2));
                indexes[IN + 0x1E] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0xB, Ycont + 0));
                indexes[IN + 0x1F] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0xF, Ycont + 2));
            }


        }


        private static void preenche8(ref SimpleBitmap bitmap, int Xcont, int Ycont, ref Color[] colors, ref byte[] indexes, int IN, bool flipInX)
        {
            byte[][] idxs = new byte[0x20][]; // indice de base;
            for (int i = 0; i < 0x20; i++)
            {
                idxs[i] = new byte[2];
            }

            if (flipInX == false)
            {
                idxs[0x00][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x04, Ycont + 2));
                idxs[0x00][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x00, Ycont + 0));
                idxs[0x01][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x0C, Ycont + 2));
                idxs[0x01][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x08, Ycont + 0));
                idxs[0x02][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x14, Ycont + 2));
                idxs[0x02][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x10, Ycont + 0));
                idxs[0x03][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x1C, Ycont + 2));
                idxs[0x03][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x18, Ycont + 0));
                idxs[0x04][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x05, Ycont + 2));
                idxs[0x04][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x01, Ycont + 0));
                idxs[0x05][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x0D, Ycont + 2));
                idxs[0x05][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x09, Ycont + 0));
                idxs[0x06][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x15, Ycont + 2));
                idxs[0x06][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x11, Ycont + 0));
                idxs[0x07][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x1D, Ycont + 2));
                idxs[0x07][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x19, Ycont + 0));
                idxs[0x08][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x06, Ycont + 2));
                idxs[0x08][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x02, Ycont + 0));
                idxs[0x09][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x0E, Ycont + 2));
                idxs[0x09][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x0A, Ycont + 0));
                idxs[0x0A][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x16, Ycont + 2));
                idxs[0x0A][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x12, Ycont + 0));
                idxs[0x0B][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x1E, Ycont + 2));
                idxs[0x0B][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x1A, Ycont + 0));
                idxs[0x0C][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x07, Ycont + 2));
                idxs[0x0C][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x03, Ycont + 0));
                idxs[0x0D][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x0F, Ycont + 2));
                idxs[0x0D][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x0B, Ycont + 0));
                idxs[0x0E][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x17, Ycont + 2));
                idxs[0x0E][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x13, Ycont + 0));
                idxs[0x0F][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x1F, Ycont + 2));
                idxs[0x0F][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x1B, Ycont + 0));
                idxs[0x10][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x00, Ycont + 2));
                idxs[0x10][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x04, Ycont + 0));
                idxs[0x11][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x08, Ycont + 2));
                idxs[0x11][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x0C, Ycont + 0));
                idxs[0x12][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x10, Ycont + 2));
                idxs[0x12][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x14, Ycont + 0));
                idxs[0x13][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x18, Ycont + 2));
                idxs[0x13][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x1C, Ycont + 0));
                idxs[0x14][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x01, Ycont + 2));
                idxs[0x14][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x05, Ycont + 0));
                idxs[0x15][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x09, Ycont + 2));
                idxs[0x15][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x0D, Ycont + 0));
                idxs[0x16][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x11, Ycont + 2));
                idxs[0x16][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x15, Ycont + 0));
                idxs[0x17][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x19, Ycont + 2));
                idxs[0x17][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x1D, Ycont + 0));
                idxs[0x18][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x02, Ycont + 2));
                idxs[0x18][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x06, Ycont + 0));
                idxs[0x19][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x0A, Ycont + 2));
                idxs[0x19][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x0E, Ycont + 0));
                idxs[0x1A][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x12, Ycont + 2));
                idxs[0x1A][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x16, Ycont + 0));
                idxs[0x1B][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x1A, Ycont + 2));
                idxs[0x1B][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x1E, Ycont + 0));
                idxs[0x1C][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x03, Ycont + 2));
                idxs[0x1C][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x07, Ycont + 0));
                idxs[0x1D][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x0B, Ycont + 2));
                idxs[0x1D][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x0F, Ycont + 0));
                idxs[0x1E][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x13, Ycont + 2));
                idxs[0x1E][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x17, Ycont + 0));
                idxs[0x1F][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x1B, Ycont + 2));
                idxs[0x1F][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x1F, Ycont + 0));
            }
            else
            {
                // versão flip
                idxs[0x00][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x00, Ycont + 2));
                idxs[0x00][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x04, Ycont + 0));
                idxs[0x01][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x08, Ycont + 2));
                idxs[0x01][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x0C, Ycont + 0));
                idxs[0x02][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x10, Ycont + 2));
                idxs[0x02][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x14, Ycont + 0));
                idxs[0x03][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x18, Ycont + 2));
                idxs[0x03][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x1C, Ycont + 0));
                idxs[0x04][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x01, Ycont + 2));
                idxs[0x04][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x05, Ycont + 0));
                idxs[0x05][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x09, Ycont + 2));
                idxs[0x05][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x0D, Ycont + 0));
                idxs[0x06][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x11, Ycont + 2));
                idxs[0x06][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x15, Ycont + 0));
                idxs[0x07][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x19, Ycont + 2));
                idxs[0x07][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x1D, Ycont + 0));
                idxs[0x08][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x02, Ycont + 2));
                idxs[0x08][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x06, Ycont + 0));
                idxs[0x09][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x0A, Ycont + 2));
                idxs[0x09][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x0E, Ycont + 0));
                idxs[0x0A][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x12, Ycont + 2));
                idxs[0x0A][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x16, Ycont + 0));
                idxs[0x0B][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x1A, Ycont + 2));
                idxs[0x0B][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x1E, Ycont + 0));
                idxs[0x0C][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x03, Ycont + 2));
                idxs[0x0C][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x07, Ycont + 0));
                idxs[0x0D][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x0B, Ycont + 2));
                idxs[0x0D][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x0F, Ycont + 0));
                idxs[0x0E][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x13, Ycont + 2));
                idxs[0x0E][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x17, Ycont + 0));
                idxs[0x0F][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x1B, Ycont + 2));
                idxs[0x0F][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x1F, Ycont + 0));
                idxs[0x10][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x04, Ycont + 2));
                idxs[0x10][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x00, Ycont + 0));
                idxs[0x11][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x0C, Ycont + 2));
                idxs[0x11][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x08, Ycont + 0));
                idxs[0x12][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x14, Ycont + 2));
                idxs[0x12][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x10, Ycont + 0));
                idxs[0x13][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x1C, Ycont + 2));
                idxs[0x13][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x18, Ycont + 0));
                idxs[0x14][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x05, Ycont + 2));
                idxs[0x14][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x01, Ycont + 0));
                idxs[0x15][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x0D, Ycont + 2));
                idxs[0x15][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x09, Ycont + 0));
                idxs[0x16][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x15, Ycont + 2));
                idxs[0x16][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x11, Ycont + 0));
                idxs[0x17][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x1D, Ycont + 2));
                idxs[0x17][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x19, Ycont + 0));
                idxs[0x18][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x06, Ycont + 2));
                idxs[0x18][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x02, Ycont + 0));
                idxs[0x19][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x0E, Ycont + 2));
                idxs[0x19][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x0A, Ycont + 0));
                idxs[0x1A][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x16, Ycont + 2));
                idxs[0x1A][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x12, Ycont + 0));
                idxs[0x1B][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x1E, Ycont + 2));
                idxs[0x1B][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x1A, Ycont + 0));
                idxs[0x1C][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x07, Ycont + 2));
                idxs[0x1C][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x03, Ycont + 0));
                idxs[0x1D][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x0F, Ycont + 2));
                idxs[0x1D][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x0B, Ycont + 0));
                idxs[0x1E][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x17, Ycont + 2));
                idxs[0x1E][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x13, Ycont + 0));
                idxs[0x1F][0] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x1F, Ycont + 2));
                idxs[0x1F][1] = (byte)Array.IndexOf(colors, bitmap.GetPixel(Xcont + 0x1B, Ycont + 0));
            }


            for (int i = 0; i < 0x20; i++)
            {
                byte index1 = idxs[i][1];
                byte index2 = idxs[i][0];
                byte left = (byte)(index1 << 4);
                byte[] arr = new byte[2] { left, index2 };
                ushort us = BitConverter.ToUInt16(arr, 0);
                byte Endindex = (byte)(us >> 4);
                indexes[IN + i] = Endindex;
            }


        }


    }

}
