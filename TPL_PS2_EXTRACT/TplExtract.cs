﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

namespace TPL_PS2_EXTRACT
{
    /*
    Codigo feito por JADERLINK
    Pesquisas feitas por HardHain e JaderLink e Zatarita.
    https://www.youtube.com/@JADERLINK
    https://www.youtube.com/@HardRainModder

    https://residentevil modding.boards.net/user/10432
    https://residentevil modding.boards.net/user/29688
    https://residentevil modding.boards.net/user/26610

    Referencia:
    https://github.com/christianmateus/TPLUtil
    https://residentevil modding.boards.net/thread/15776/tpl-file-specification
    https://residentevil modding.boards.net/thread/15336/tutorial-edit-skins-textures-ps2
    engenharia reversa de imagem sobre o programa "Game Graphic Studio 7.4.0 by obocaman"
    engenharia reversa de imagem sobre o programa "MummGGTool"/"Mumm-Ra's Game Graphic Tool"
    engenharia reversa de imagem sobre o jogo com o "Texmod.exe" com o "PCSX2 1.4.0"

    Em desenvolvimento
    Para Pesquisas
    2023/12/30
    version: B.1.1.0.0
    */

    public static class TplExtract
    {
        public static void Extract(Stream stream, FileInfo info, ImageFormat imageFormat, bool flipY, bool rotateInterlace1and3) 
        {
            BinaryReader br = new BinaryReader(stream);

            string baseName = info.Name.Remove(info.Name.Length - info.Extension.Length, info.Extension.Length);

            string baseDiretory = info.DirectoryName + "\\" + baseName + "\\";

            if (!Directory.Exists(baseDiretory))
            {
                Directory.CreateDirectory(baseDiretory);
            }

            var idxtpl = new FileInfo(info.DirectoryName + "\\" + baseName + ".idxps2tpl").CreateText();
            idxtpl.WriteLine(Program.headerText());
            idxtpl.WriteLine("");
            idxtpl.WriteLine("ImageFlipY:" + flipY);
            idxtpl.WriteLine("ImageFolder:" + baseName);
            idxtpl.WriteLine("ImageFormat:" + Enum.GetName(typeof(ImageFormat), imageFormat));

            Console.WriteLine("ImageFlipY: " + flipY);
            Console.WriteLine("ImageFolder: " + baseName);
            Console.WriteLine("ImageFormat: " + Enum.GetName(typeof(ImageFormat), imageFormat));
            Console.WriteLine("RotateInterlace1and3: " + rotateInterlace1and3);

            var text = new AltTextWriter(info.FullName + ".Debug.txt2", false); // arquivo de debug desabilitado para a versão release
            text.WriteLine(Program.headerText());
            text.WriteLine(info.FullName);
            text.WriteLine("");

            byte[] magic = br.ReadBytes(4);
            text.WriteLine("magic: " + BitConverter.ToString(magic));

            uint TplCount = br.ReadUInt32();
            Console.WriteLine("TplCount: " + TplCount);
            text.WriteLine("TplCount: " + TplCount);

            uint StartOffset = br.ReadUInt32();
            text.WriteLine("StartOffset: 0x" + StartOffset.ToString("X8"));

            byte[] headerUnk1 = br.ReadBytes(4);
            text.WriteLine("headerUnk1: " + BitConverter.ToString(headerUnk1));
            text.WriteLine("");

            TplImageHeader[] tihs = new TplImageHeader[TplCount];

            br.BaseStream.Position = StartOffset;

            TplImageHeaderAssistant assistant = new TplImageHeaderAssistant();

            //headers
            uint PositionCount = 0;
            for (int i = 0; i < TplCount; i++)
            {
                text.WriteLine("ImageID: " + i);

                TplImageHeader tih = assistant.Fill(ref br, StartOffset + PositionCount);
                PositionCount += 0x30;
                assistant.SetText(ref text, tih);

                PrintTplImageHeader(tih, "Entry: " + i.ToString("D4"));

                if (tih.MipmapStatus == 0x2)
                {
                    if (tih.MipmapHeader1Offset != 0)
                    {
                        text.WriteLine("");
                        text.WriteLine("REF ImageID: " + i + ",  mipmapHeader1");
                        TplImageHeader mipmap1 = assistant.Fill(ref br, tih.MipmapHeader1Offset);
                        tih.MipmapHeader1 = mipmap1;
                        assistant.SetText(ref text, mipmap1);

                        PrintTplImageHeader(mipmap1, "Mipmap1:   ");
                    }

                    if (tih.MipmapHeader2Offset != 0)
                    {
                        text.WriteLine("");
                        text.WriteLine("REF ImageID: " + i + ",  mipmapHeader2");
                        TplImageHeader mipmap2 = assistant.Fill(ref br, tih.MipmapHeader2Offset);
                        tih.MipmapHeader2 = mipmap2;
                        assistant.SetText(ref text, mipmap2);

                        PrintTplImageHeader(mipmap2, "Mipmap2:   ");
                    }
                  
                }

                FileInfo headerInfo = new FileInfo(baseDiretory + i.ToString("D4") + ".IdxtplHeader");
                var idxHeader = headerInfo.CreateText();
                idxHeader.WriteLine(Program.headerText());
                idxHeader.WriteLine("");
                assistant.SetIdxTplMain(ref idxHeader, tih);
                idxHeader.Close();

                tihs[i] = tih;
                text.WriteLine("");
            }

          
            TplImage tplImage = new TplImage(ref br, flipY, rotateInterlace1and3);

            //images //mipmap
            for (int i = 0; i < TplCount; i++)
            {
                Bitmap bitmap = null;
                bool AsBitmap = false;

                try
                {
                    AsBitmap = tplImage.GetImage(tihs[i].Width, tihs[i].Height, tihs[i].BitDepth, tihs[i].Interlace, tihs[i].IndexesOffset, tihs[i].PaletteOffset, out bitmap);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                if (AsBitmap && bitmap != null)
                {
                    BitmapSalve(ref bitmap, imageFormat, baseDiretory + i.ToString("D4"));
                }
                

                if (tihs[i].MipmapStatus == 0x2)
                {
                    if (tihs[i].MipmapHeader1 != null)
                    {
                        Bitmap bitmap1 = null;
                        bool AsBitmap1 = false;

                        try
                        {
                            AsBitmap1 = tplImage.GetImage(tihs[i].MipmapHeader1.Width, tihs[i].MipmapHeader1.Height, tihs[i].MipmapHeader1.BitDepth, tihs[i].MipmapHeader1.Interlace, tihs[i].MipmapHeader1.IndexesOffset, tihs[i].PaletteOffset, out bitmap1);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }

                        if (AsBitmap1 && bitmap1 != null)
                        {
                            BitmapSalve(ref bitmap1, imageFormat, baseDiretory + i.ToString("D4") + "_Mipmap1");
                        }

                    }

                    if (tihs[i].MipmapHeader2 != null)
                    {
                        Bitmap bitmap2 = null;
                        bool AsBitmap2 = false;

                        try
                        {
                            AsBitmap2 = tplImage.GetImage(tihs[i].MipmapHeader2.Width, tihs[i].MipmapHeader2.Height, tihs[i].MipmapHeader2.BitDepth, tihs[i].MipmapHeader2.Interlace, tihs[i].MipmapHeader2.IndexesOffset, tihs[i].PaletteOffset, out bitmap2);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }

                        if (AsBitmap2 && bitmap2 != null)
                        {
                            BitmapSalve(ref bitmap2, imageFormat, baseDiretory + i.ToString("D4") + "_Mipmap2");
                        }

                    }


                }

             
            }


            idxtpl.Close();
            text.Close();
            br.Close();
        }

        private static void PrintTplImageHeader(TplImageHeader data, string text)
        {
            Console.WriteLine(text + "   Dimension: " + (data.Width + "x" + data.Height).PadRight(9) + "   BitDepth: " + data.BitDepth + "   Interlace: " + data.Interlace);
        }

        private static void BitmapSalve(ref Bitmap bitmap, ImageFormat imageFormat, string name) 
        {
            switch (imageFormat)
            {
                case ImageFormat.TGA:
                    TGASharpLib.TGA tga = new TGASharpLib.TGA(bitmap);
                    tga.Save(name + ".TGA");
                    break;
                case ImageFormat.PNG:
                    bitmap.Save(name + ".PNG", System.Drawing.Imaging.ImageFormat.Png);
                    break;
                case ImageFormat.BMP:
                    bitmap.Save(name + ".BMP", System.Drawing.Imaging.ImageFormat.Bmp);
                    break;
                case ImageFormat.GIF:
                    bitmap.Save(name + ".GIF", System.Drawing.Imaging.ImageFormat.Gif);
                    break;
                default:
                    break;
            }


            

        }

    }


    public class TplImage 
    {
        private BinaryReader br;
        private bool flipY = false;
        private bool rotateInterlace1and3 = false;

        public TplImage(ref BinaryReader br, bool flipY, bool rotateInterlace1and3)
        {
            this.br = br;
            this.flipY = flipY;
            this.rotateInterlace1and3 = rotateInterlace1and3;
        }

        public bool GetImage(int width, int height, ushort bitDepth, ushort interlace, uint indexesOffset, uint paletteOffset, out Bitmap bitmap)
        {
            
            if (bitDepth == 0x8 && (interlace == 0x0 || interlace == 0x1))
            {
                return bitDepth8interlace0and1(width, height, bitDepth, interlace, indexesOffset, paletteOffset, out bitmap);
            }
            else if (bitDepth == 0x9 && (interlace == 0x0 || interlace == 0x1))
            {
                return bitDepth9interlace0and1(width, height, bitDepth, interlace, indexesOffset, paletteOffset, out bitmap);
            }
            else if (bitDepth == 0x6 && (interlace == 0x0 || interlace == 0x1))
            {
                return bitDepth6interlace0and1(width, height, bitDepth, interlace, indexesOffset, paletteOffset, out bitmap);
            }
            else if (bitDepth == 0x8 && (interlace == 0x2 || interlace == 0x3))
            {
                return bitDepth8interlace2and3(width, height, bitDepth, interlace, indexesOffset, paletteOffset, out bitmap);
            }
            else if (bitDepth == 0x9 && (interlace == 0x2 || interlace == 0x3))
            {
                return bitDepth9interlace2and3(width, height, bitDepth, interlace, indexesOffset, paletteOffset, out bitmap);
            }
            else if (bitDepth == 0x6 && (interlace == 0x2 || interlace == 0x3))
            {
                //não suportado atualmente
            }
            else 
            {
                //outros casos não existem
            }

            bitmap = null;
            return false;
        }

        private bool bitDepth8interlace0and1(int width, int height, ushort bitDepth, ushort interlace, uint indexesOffset, uint paletteOffset, out Bitmap bitmap) 
        {
            int indexesbytesCount = (height * width) / 2;

            br.BaseStream.Position = indexesOffset;

            byte[] indexes = br.ReadBytes(indexesbytesCount);

            br.BaseStream.Position = paletteOffset;

            byte[] palette = br.ReadBytes(0x80);

            Color[] colors = new Color[16];

            int cont = 0;
            for (int ic = 0; ic < 16; ic++)
            {
                Color c = Color.FromArgb(
                    (byte)(palette[cont + 3] * 0xFF / 0x80),
                    palette[cont + 0],
                    palette[cont + 1],
                    palette[cont + 2]
                    );
                colors[ic] = c;
                cont += 4;
                if (ic == 7)
                {
                    cont = 0x40;
                }
            }


            bitmap = new Bitmap(width, height);


            int Xcont = 0;
            int Ycont = 0;
            for (int IN = 0; IN < indexes.Length; IN++)
            {
                int nibbleLow = indexes[IN] >> 4;
                int nibbleHigh = indexes[IN] & 0x0F;

                bitmap.SetPixel(Xcont + 1, Ycont, colors[nibbleLow]);
                bitmap.SetPixel(Xcont, Ycont, colors[nibbleHigh]);


                Xcont += 2;
                if (Xcont >= width)
                {
                    Xcont = 0;
                    Ycont += 1;
                }
            }

            if (rotateInterlace1and3 && interlace == 0x1)
            {
                bitmap.RotateFlip(RotateFlipType.Rotate90FlipX);
            }

            if (flipY)
            {
                bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            }
          
            return true;

        }

        private bool bitDepth9interlace0and1(int width, int height, ushort bitDepth, ushort interlace, uint indexesOffset, uint paletteOffset, out Bitmap bitmap) 
        {
            int indexesbytesCount = (height * width);

            br.BaseStream.Position = indexesOffset;

            byte[] indexes = br.ReadBytes(indexesbytesCount);

            br.BaseStream.Position = paletteOffset;

            byte[] palette = br.ReadBytes(256 * 4);

            Color[] colors = new Color[256];

            int index = 0;
            int swap = 1;
            for (int ic = 0; ic < 256; ic++)
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

                Color c = Color.FromArgb(
                  (byte)(palette[cont + 3] * 0xFF / 0x80),
                  palette[cont + 0],
                  palette[cont + 1],
                  palette[cont + 2]
                  );

                colors[ic] = c;

                index++;

            }

            bitmap = new Bitmap(width, height);


            int Xcont = 0;
            int Ycont = 0;
            for (int IN = 0; IN < indexes.Length; IN++)
            {
                bitmap.SetPixel(Xcont, Ycont, colors[indexes[IN]]);

                Xcont += 1;
                if (Xcont >= width)
                {
                    Xcont = 0;
                    Ycont += 1;
                }
            }

            if (rotateInterlace1and3 && interlace == 0x1)
            {
                bitmap.RotateFlip(RotateFlipType.Rotate90FlipX);
            }

            if (flipY)
            {
                bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            }

            return true;
        }

        private bool bitDepth6interlace0and1(int width, int height, ushort bitDepth, ushort interlace, uint indexesOffset, uint paletteOffset, out Bitmap bitmap) 
        {
            int bytesCount = (height * width) * 4;

            br.BaseStream.Position = indexesOffset;

            byte[] ColorBytes = br.ReadBytes(bytesCount);

            bitmap = new Bitmap(width, height);

            int Xcont = 0;
            int Ycont = 0;
            int lenght = height * width;
            int cont = 0;
            for (int IN = 0; IN < lenght; IN++)
            {
                Color c = Color.FromArgb(
                   ColorBytes[cont + 3],
                   ColorBytes[cont + 0],
                   ColorBytes[cont + 1],
                   ColorBytes[cont + 2]
                   );
                cont += 4;

                bitmap.SetPixel(Xcont, Ycont, c);

                Xcont++;
                if (Xcont >= width)
                {
                    Xcont = 0;
                    Ycont += 1;
                }
            }

            if (rotateInterlace1and3 && interlace == 0x1)
            {
                bitmap.RotateFlip(RotateFlipType.Rotate90FlipX);
            }

            if (flipY)
            {
                bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            }

            return true;
        }


        private bool bitDepth8interlace2and3(int width, int height, ushort bitDepth, ushort interlace, uint indexesOffset, uint paletteOffset, out Bitmap bitmap) 
        {
            int indexesbytesCount = (height * width) / 2;

            br.BaseStream.Position = indexesOffset;

            byte[] indexes = br.ReadBytes(indexesbytesCount);

            br.BaseStream.Position = paletteOffset;

            byte[] palette = br.ReadBytes(0x80);

            Color[] colors = new Color[16];

            int cont = 0;
            for (int ic = 0; ic < 16; ic++)
            {
                Color c = Color.FromArgb(
                    (byte)(palette[cont + 3] * 0xFF / 0x80),
                    palette[cont + 0],
                    palette[cont + 1],
                    palette[cont + 2]
                    );
                colors[ic] = c;
                cont += 4;
                if (ic == 7)
                {
                    cont = 0x40;
                }
            }


            // no comentario esta a imagem na posição correta
            //bitmap = new Bitmap(width, height);
            bitmap = new Bitmap(height, width);

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

            // correção da imagem baseado em gambiarra

            if (width > 128 || height > 128)
            {
                Bitmap bitmapFix = new Bitmap(width, height);

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
                    if (setX >= width)
                    {
                        setX = 0;
                        setY += 128;
                    }

                    copyY += 128;
                    if (copyY >= width)
                    {
                        copyY = 0;
                        copyX += 128;
                    }

                }

                bitmap = bitmapFix;

            }


            if (rotateInterlace1and3 && interlace == 0x3)
            {
                bitmap.RotateFlip(RotateFlipType.Rotate90FlipX);
            }

            if (flipY)
            {
                bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

            }

            return true;
        }

        private bool bitDepth9interlace2and3(int width, int height, ushort bitDepth, ushort interlace, uint indexesOffset, uint paletteOffset, out Bitmap bitmap) 
        {
            int indexesbytesCount = (height * width);

            br.BaseStream.Position = indexesOffset;

            byte[] indexes = br.ReadBytes(indexesbytesCount);

            br.BaseStream.Position = paletteOffset;

            byte[] palette = br.ReadBytes(256 * 4);

            Color[] colors = new Color[256];

            int index = 0;
            int swap = 1;
            for (int ic = 0; ic < 256; ic++)
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

                Color c = Color.FromArgb(
                  (byte)(palette[cont + 3] * 0xFF / 0x80),
                  palette[cont + 0],
                  palette[cont + 1],
                  palette[cont + 2]
                  );

                colors[ic] = c;

                index++;

            }

            bitmap = new Bitmap(width, height);

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

            if (rotateInterlace1and3 && interlace == 0x3)
            {
                bitmap.RotateFlip(RotateFlipType.Rotate90FlipX);
            }

            if (flipY)
            {
                bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            }

            return true;
        }



        private static void preenche9(ref Bitmap bitmap, int Xcont, int Ycont, ref Color[] colors, ref byte[] indexes, int IN, bool flipInX)
        {
            if (flipInX == false)
            {
                bitmap.SetPixel(Xcont + 0x0, Ycont + 0, colors[indexes[IN + 0x0]]);
                bitmap.SetPixel(Xcont + 0x4, Ycont + 2, colors[indexes[IN + 0x1]]);
                bitmap.SetPixel(Xcont + 0x8, Ycont + 0, colors[indexes[IN + 0x2]]);
                bitmap.SetPixel(Xcont + 0xC, Ycont + 2, colors[indexes[IN + 0x3]]);

                bitmap.SetPixel(Xcont + 0x1, Ycont + 0, colors[indexes[IN + 0x4]]);
                bitmap.SetPixel(Xcont + 0x5, Ycont + 2, colors[indexes[IN + 0x5]]);
                bitmap.SetPixel(Xcont + 0x9, Ycont + 0, colors[indexes[IN + 0x6]]);
                bitmap.SetPixel(Xcont + 0xD, Ycont + 2, colors[indexes[IN + 0x7]]);

                bitmap.SetPixel(Xcont + 0x2, Ycont + 0, colors[indexes[IN + 0x8]]);
                bitmap.SetPixel(Xcont + 0x6, Ycont + 2, colors[indexes[IN + 0x9]]);
                bitmap.SetPixel(Xcont + 0xA, Ycont + 0, colors[indexes[IN + 0xA]]);
                bitmap.SetPixel(Xcont + 0xE, Ycont + 2, colors[indexes[IN + 0xB]]);

                bitmap.SetPixel(Xcont + 0x3, Ycont + 0, colors[indexes[IN + 0xC]]);
                bitmap.SetPixel(Xcont + 0x7, Ycont + 2, colors[indexes[IN + 0xD]]);
                bitmap.SetPixel(Xcont + 0xB, Ycont + 0, colors[indexes[IN + 0xE]]);
                bitmap.SetPixel(Xcont + 0xF, Ycont + 2, colors[indexes[IN + 0xF]]);

                bitmap.SetPixel(Xcont + 0x4, Ycont + 0, colors[indexes[IN + 0x10]]);
                bitmap.SetPixel(Xcont + 0x0, Ycont + 2, colors[indexes[IN + 0x11]]);
                bitmap.SetPixel(Xcont + 0xC, Ycont + 0, colors[indexes[IN + 0x12]]);
                bitmap.SetPixel(Xcont + 0x8, Ycont + 2, colors[indexes[IN + 0x13]]);

                bitmap.SetPixel(Xcont + 0x5, Ycont + 0, colors[indexes[IN + 0x14]]);
                bitmap.SetPixel(Xcont + 0x1, Ycont + 2, colors[indexes[IN + 0x15]]);
                bitmap.SetPixel(Xcont + 0xD, Ycont + 0, colors[indexes[IN + 0x16]]);
                bitmap.SetPixel(Xcont + 0x9, Ycont + 2, colors[indexes[IN + 0x17]]);

                bitmap.SetPixel(Xcont + 0x6, Ycont + 0, colors[indexes[IN + 0x18]]);
                bitmap.SetPixel(Xcont + 0x2, Ycont + 2, colors[indexes[IN + 0x19]]);
                bitmap.SetPixel(Xcont + 0xE, Ycont + 0, colors[indexes[IN + 0x1A]]);
                bitmap.SetPixel(Xcont + 0xA, Ycont + 2, colors[indexes[IN + 0x1B]]);

                bitmap.SetPixel(Xcont + 0x7, Ycont + 0, colors[indexes[IN + 0x1C]]);
                bitmap.SetPixel(Xcont + 0x3, Ycont + 2, colors[indexes[IN + 0x1D]]);
                bitmap.SetPixel(Xcont + 0xF, Ycont + 0, colors[indexes[IN + 0x1E]]);
                bitmap.SetPixel(Xcont + 0xB, Ycont + 2, colors[indexes[IN + 0x1F]]);
            }
            else
            {
                bitmap.SetPixel(Xcont + 0x4, Ycont + 0, colors[indexes[IN + 0x0]]);
                bitmap.SetPixel(Xcont + 0x0, Ycont + 2, colors[indexes[IN + 0x1]]);
                bitmap.SetPixel(Xcont + 0xC, Ycont + 0, colors[indexes[IN + 0x2]]);
                bitmap.SetPixel(Xcont + 0x8, Ycont + 2, colors[indexes[IN + 0x3]]);

                bitmap.SetPixel(Xcont + 0x5, Ycont + 0, colors[indexes[IN + 0x4]]);
                bitmap.SetPixel(Xcont + 0x1, Ycont + 2, colors[indexes[IN + 0x5]]);
                bitmap.SetPixel(Xcont + 0xD, Ycont + 0, colors[indexes[IN + 0x6]]);
                bitmap.SetPixel(Xcont + 0x9, Ycont + 2, colors[indexes[IN + 0x7]]);

                bitmap.SetPixel(Xcont + 0x6, Ycont + 0, colors[indexes[IN + 0x8]]);
                bitmap.SetPixel(Xcont + 0x2, Ycont + 2, colors[indexes[IN + 0x9]]);
                bitmap.SetPixel(Xcont + 0xE, Ycont + 0, colors[indexes[IN + 0xA]]);
                bitmap.SetPixel(Xcont + 0xA, Ycont + 2, colors[indexes[IN + 0xB]]);

                bitmap.SetPixel(Xcont + 0x7, Ycont + 0, colors[indexes[IN + 0xC]]);
                bitmap.SetPixel(Xcont + 0x3, Ycont + 2, colors[indexes[IN + 0xD]]);
                bitmap.SetPixel(Xcont + 0xF, Ycont + 0, colors[indexes[IN + 0xE]]);
                bitmap.SetPixel(Xcont + 0xB, Ycont + 2, colors[indexes[IN + 0xF]]);

                bitmap.SetPixel(Xcont + 0x0, Ycont + 0, colors[indexes[IN + 0x10]]);
                bitmap.SetPixel(Xcont + 0x4, Ycont + 2, colors[indexes[IN + 0x11]]);
                bitmap.SetPixel(Xcont + 0x8, Ycont + 0, colors[indexes[IN + 0x12]]);
                bitmap.SetPixel(Xcont + 0xC, Ycont + 2, colors[indexes[IN + 0x13]]);

                bitmap.SetPixel(Xcont + 0x1, Ycont + 0, colors[indexes[IN + 0x14]]);
                bitmap.SetPixel(Xcont + 0x5, Ycont + 2, colors[indexes[IN + 0x15]]);
                bitmap.SetPixel(Xcont + 0x9, Ycont + 0, colors[indexes[IN + 0x16]]);
                bitmap.SetPixel(Xcont + 0xD, Ycont + 2, colors[indexes[IN + 0x17]]);

                bitmap.SetPixel(Xcont + 0x2, Ycont + 0, colors[indexes[IN + 0x18]]);
                bitmap.SetPixel(Xcont + 0x6, Ycont + 2, colors[indexes[IN + 0x19]]);
                bitmap.SetPixel(Xcont + 0xA, Ycont + 0, colors[indexes[IN + 0x1A]]);
                bitmap.SetPixel(Xcont + 0xE, Ycont + 2, colors[indexes[IN + 0x1B]]);

                bitmap.SetPixel(Xcont + 0x3, Ycont + 0, colors[indexes[IN + 0x1C]]);
                bitmap.SetPixel(Xcont + 0x7, Ycont + 2, colors[indexes[IN + 0x1D]]);
                bitmap.SetPixel(Xcont + 0xB, Ycont + 0, colors[indexes[IN + 0x1E]]);
                bitmap.SetPixel(Xcont + 0xF, Ycont + 2, colors[indexes[IN + 0x1F]]);

            }


        }


        private static void preenche8(ref Bitmap bitmap, int Xcont, int Ycont, ref Color[] colors, ref byte[] indexes, int IN, bool flipInX)
        {
            int[][] idxs = new int[0x20][]; // indice de cor;
            for (int i = 0; i < 0x20; i++)
            {
                int nibbleLow = indexes[IN + i] >> 4;
                int nibbleHigh = indexes[IN + i] & 0x0F;

                int[] two = new int[2];

                two[0] = nibbleLow;
                two[1] = nibbleHigh;

                idxs[i] = two;
            }


            if (flipInX == false)
            {
                bitmap.SetPixel(Xcont + 0x4, Ycont + 2, colors[idxs[0x0][0]]);
                bitmap.SetPixel(Xcont + 0x0, Ycont + 0, colors[idxs[0x0][1]]);
                bitmap.SetPixel(Xcont + 0xC, Ycont + 2, colors[idxs[0x1][0]]);
                bitmap.SetPixel(Xcont + 0x8, Ycont + 0, colors[idxs[0x1][1]]);
                bitmap.SetPixel(Xcont + 0x14, Ycont + 2, colors[idxs[0x2][0]]);
                bitmap.SetPixel(Xcont + 0x10, Ycont + 0, colors[idxs[0x2][1]]);
                bitmap.SetPixel(Xcont + 0x1C, Ycont + 2, colors[idxs[0x3][0]]);
                bitmap.SetPixel(Xcont + 0x18, Ycont + 0, colors[idxs[0x3][1]]);

                bitmap.SetPixel(Xcont + 0x5, Ycont + 2, colors[idxs[0x4][0]]);
                bitmap.SetPixel(Xcont + 0x1, Ycont + 0, colors[idxs[0x4][1]]);
                bitmap.SetPixel(Xcont + 0xD, Ycont + 2, colors[idxs[0x5][0]]);
                bitmap.SetPixel(Xcont + 0x9, Ycont + 0, colors[idxs[0x5][1]]);
                bitmap.SetPixel(Xcont + 0x15, Ycont + 2, colors[idxs[0x6][0]]);
                bitmap.SetPixel(Xcont + 0x11, Ycont + 0, colors[idxs[0x6][1]]);
                bitmap.SetPixel(Xcont + 0x1D, Ycont + 2, colors[idxs[0x7][0]]);
                bitmap.SetPixel(Xcont + 0x19, Ycont + 0, colors[idxs[0x7][1]]);



                bitmap.SetPixel(Xcont + 0x6, Ycont + 2, colors[idxs[0x8][0]]);
                bitmap.SetPixel(Xcont + 0x2, Ycont + 0, colors[idxs[0x8][1]]);
                bitmap.SetPixel(Xcont + 0xE, Ycont + 2, colors[idxs[0x9][0]]);
                bitmap.SetPixel(Xcont + 0xA, Ycont + 0, colors[idxs[0x9][1]]);
                bitmap.SetPixel(Xcont + 0x16, Ycont + 2, colors[idxs[0xA][0]]);
                bitmap.SetPixel(Xcont + 0x12, Ycont + 0, colors[idxs[0xA][1]]);
                bitmap.SetPixel(Xcont + 0x1E, Ycont + 2, colors[idxs[0xB][0]]);
                bitmap.SetPixel(Xcont + 0x1A, Ycont + 0, colors[idxs[0xB][1]]);

                bitmap.SetPixel(Xcont + 0x7, Ycont + 2, colors[idxs[0xC][0]]);
                bitmap.SetPixel(Xcont + 0x3, Ycont + 0, colors[idxs[0xC][1]]);
                bitmap.SetPixel(Xcont + 0xF, Ycont + 2, colors[idxs[0xD][0]]);
                bitmap.SetPixel(Xcont + 0xB, Ycont + 0, colors[idxs[0xD][1]]);
                bitmap.SetPixel(Xcont + 0x17, Ycont + 2, colors[idxs[0xE][0]]);
                bitmap.SetPixel(Xcont + 0x13, Ycont + 0, colors[idxs[0xE][1]]);
                bitmap.SetPixel(Xcont + 0x1F, Ycont + 2, colors[idxs[0xF][0]]);
                bitmap.SetPixel(Xcont + 0x1B, Ycont + 0, colors[idxs[0xF][1]]);



                bitmap.SetPixel(Xcont + 0x0, Ycont + 2, colors[idxs[0x10][0]]);
                bitmap.SetPixel(Xcont + 0x4, Ycont + 0, colors[idxs[0x10][1]]);
                bitmap.SetPixel(Xcont + 0x8, Ycont + 2, colors[idxs[0x11][0]]);
                bitmap.SetPixel(Xcont + 0xC, Ycont + 0, colors[idxs[0x11][1]]);
                bitmap.SetPixel(Xcont + 0x10, Ycont + 2, colors[idxs[0x12][0]]);
                bitmap.SetPixel(Xcont + 0x14, Ycont + 0, colors[idxs[0x12][1]]);
                bitmap.SetPixel(Xcont + 0x18, Ycont + 2, colors[idxs[0x13][0]]);
                bitmap.SetPixel(Xcont + 0x1C, Ycont + 0, colors[idxs[0x13][1]]);

                bitmap.SetPixel(Xcont + 0x1, Ycont + 2, colors[idxs[0x14][0]]);
                bitmap.SetPixel(Xcont + 0x5, Ycont + 0, colors[idxs[0x14][1]]);
                bitmap.SetPixel(Xcont + 0x9, Ycont + 2, colors[idxs[0x15][0]]);
                bitmap.SetPixel(Xcont + 0xD, Ycont + 0, colors[idxs[0x15][1]]);
                bitmap.SetPixel(Xcont + 0x11, Ycont + 2, colors[idxs[0x16][0]]);
                bitmap.SetPixel(Xcont + 0x15, Ycont + 0, colors[idxs[0x16][1]]);
                bitmap.SetPixel(Xcont + 0x19, Ycont + 2, colors[idxs[0x17][0]]);
                bitmap.SetPixel(Xcont + 0x1D, Ycont + 0, colors[idxs[0x17][1]]);



                bitmap.SetPixel(Xcont + 0x2, Ycont + 2, colors[idxs[0x18][0]]);
                bitmap.SetPixel(Xcont + 0x6, Ycont + 0, colors[idxs[0x18][1]]);
                bitmap.SetPixel(Xcont + 0xA, Ycont + 2, colors[idxs[0x19][0]]);
                bitmap.SetPixel(Xcont + 0xE, Ycont + 0, colors[idxs[0x19][1]]);
                bitmap.SetPixel(Xcont + 0x12, Ycont + 2, colors[idxs[0x1A][0]]);
                bitmap.SetPixel(Xcont + 0x16, Ycont + 0, colors[idxs[0x1A][1]]);
                bitmap.SetPixel(Xcont + 0x1A, Ycont + 2, colors[idxs[0x1B][0]]);
                bitmap.SetPixel(Xcont + 0x1E, Ycont + 0, colors[idxs[0x1B][1]]);

                bitmap.SetPixel(Xcont + 0x3, Ycont + 2, colors[idxs[0x1C][0]]);
                bitmap.SetPixel(Xcont + 0x7, Ycont + 0, colors[idxs[0x1C][1]]);
                bitmap.SetPixel(Xcont + 0xB, Ycont + 2, colors[idxs[0x1D][0]]);
                bitmap.SetPixel(Xcont + 0xF, Ycont + 0, colors[idxs[0x1D][1]]);
                bitmap.SetPixel(Xcont + 0x13, Ycont + 2, colors[idxs[0x1E][0]]);
                bitmap.SetPixel(Xcont + 0x17, Ycont + 0, colors[idxs[0x1E][1]]);
                bitmap.SetPixel(Xcont + 0x1B, Ycont + 2, colors[idxs[0x1F][0]]);
                bitmap.SetPixel(Xcont + 0x1F, Ycont + 0, colors[idxs[0x1F][1]]);



            }
            else
            {
                // versão flip

                bitmap.SetPixel(Xcont + 0x0, Ycont + 2, colors[idxs[0x0][0]]);
                bitmap.SetPixel(Xcont + 0x4, Ycont + 0, colors[idxs[0x0][1]]);
                bitmap.SetPixel(Xcont + 0x8, Ycont + 2, colors[idxs[0x1][0]]);
                bitmap.SetPixel(Xcont + 0xC, Ycont + 0, colors[idxs[0x1][1]]);
                bitmap.SetPixel(Xcont + 0x10, Ycont + 2, colors[idxs[0x2][0]]);
                bitmap.SetPixel(Xcont + 0x14, Ycont + 0, colors[idxs[0x2][1]]);
                bitmap.SetPixel(Xcont + 0x18, Ycont + 2, colors[idxs[0x3][0]]);
                bitmap.SetPixel(Xcont + 0x1C, Ycont + 0, colors[idxs[0x3][1]]);

                bitmap.SetPixel(Xcont + 0x1, Ycont + 2, colors[idxs[0x4][0]]);
                bitmap.SetPixel(Xcont + 0x5, Ycont + 0, colors[idxs[0x4][1]]);
                bitmap.SetPixel(Xcont + 0x9, Ycont + 2, colors[idxs[0x5][0]]);
                bitmap.SetPixel(Xcont + 0xD, Ycont + 0, colors[idxs[0x5][1]]);
                bitmap.SetPixel(Xcont + 0x11, Ycont + 2, colors[idxs[0x6][0]]);
                bitmap.SetPixel(Xcont + 0x15, Ycont + 0, colors[idxs[0x6][1]]);
                bitmap.SetPixel(Xcont + 0x19, Ycont + 2, colors[idxs[0x7][0]]);
                bitmap.SetPixel(Xcont + 0x1D, Ycont + 0, colors[idxs[0x7][1]]);



                bitmap.SetPixel(Xcont + 0x2, Ycont + 2, colors[idxs[0x8][0]]);
                bitmap.SetPixel(Xcont + 0x6, Ycont + 0, colors[idxs[0x8][1]]);
                bitmap.SetPixel(Xcont + 0xA, Ycont + 2, colors[idxs[0x9][0]]);
                bitmap.SetPixel(Xcont + 0xE, Ycont + 0, colors[idxs[0x9][1]]);
                bitmap.SetPixel(Xcont + 0x12, Ycont + 2, colors[idxs[0xA][0]]);
                bitmap.SetPixel(Xcont + 0x16, Ycont + 0, colors[idxs[0xA][1]]);
                bitmap.SetPixel(Xcont + 0x1A, Ycont + 2, colors[idxs[0xB][0]]);
                bitmap.SetPixel(Xcont + 0x1E, Ycont + 0, colors[idxs[0xB][1]]);

                bitmap.SetPixel(Xcont + 0x3, Ycont + 2, colors[idxs[0xC][0]]);
                bitmap.SetPixel(Xcont + 0x7, Ycont + 0, colors[idxs[0xC][1]]);
                bitmap.SetPixel(Xcont + 0xB, Ycont + 2, colors[idxs[0xD][0]]);
                bitmap.SetPixel(Xcont + 0xF, Ycont + 0, colors[idxs[0xD][1]]);
                bitmap.SetPixel(Xcont + 0x13, Ycont + 2, colors[idxs[0xE][0]]);
                bitmap.SetPixel(Xcont + 0x17, Ycont + 0, colors[idxs[0xE][1]]);
                bitmap.SetPixel(Xcont + 0x1B, Ycont + 2, colors[idxs[0xF][0]]);
                bitmap.SetPixel(Xcont + 0x1F, Ycont + 0, colors[idxs[0xF][1]]);



                bitmap.SetPixel(Xcont + 0x4, Ycont + 2, colors[idxs[0x10][0]]);
                bitmap.SetPixel(Xcont + 0x0, Ycont + 0, colors[idxs[0x10][1]]);
                bitmap.SetPixel(Xcont + 0xC, Ycont + 2, colors[idxs[0x11][0]]);
                bitmap.SetPixel(Xcont + 0x8, Ycont + 0, colors[idxs[0x11][1]]);
                bitmap.SetPixel(Xcont + 0x14, Ycont + 2, colors[idxs[0x12][0]]);
                bitmap.SetPixel(Xcont + 0x10, Ycont + 0, colors[idxs[0x12][1]]);
                bitmap.SetPixel(Xcont + 0x1C, Ycont + 2, colors[idxs[0x13][0]]);
                bitmap.SetPixel(Xcont + 0x18, Ycont + 0, colors[idxs[0x13][1]]);

                bitmap.SetPixel(Xcont + 0x5, Ycont + 2, colors[idxs[0x14][0]]);
                bitmap.SetPixel(Xcont + 0x1, Ycont + 0, colors[idxs[0x14][1]]);
                bitmap.SetPixel(Xcont + 0xD, Ycont + 2, colors[idxs[0x15][0]]);
                bitmap.SetPixel(Xcont + 0x9, Ycont + 0, colors[idxs[0x15][1]]);
                bitmap.SetPixel(Xcont + 0x15, Ycont + 2, colors[idxs[0x16][0]]);
                bitmap.SetPixel(Xcont + 0x11, Ycont + 0, colors[idxs[0x16][1]]);
                bitmap.SetPixel(Xcont + 0x1D, Ycont + 2, colors[idxs[0x17][0]]);
                bitmap.SetPixel(Xcont + 0x19, Ycont + 0, colors[idxs[0x17][1]]);



                bitmap.SetPixel(Xcont + 0x6, Ycont + 2, colors[idxs[0x18][0]]);
                bitmap.SetPixel(Xcont + 0x2, Ycont + 0, colors[idxs[0x18][1]]);
                bitmap.SetPixel(Xcont + 0xE, Ycont + 2, colors[idxs[0x19][0]]);
                bitmap.SetPixel(Xcont + 0xA, Ycont + 0, colors[idxs[0x19][1]]);
                bitmap.SetPixel(Xcont + 0x16, Ycont + 2, colors[idxs[0x1A][0]]);
                bitmap.SetPixel(Xcont + 0x12, Ycont + 0, colors[idxs[0x1A][1]]);
                bitmap.SetPixel(Xcont + 0x1E, Ycont + 2, colors[idxs[0x1B][0]]);
                bitmap.SetPixel(Xcont + 0x1A, Ycont + 0, colors[idxs[0x1B][1]]);

                bitmap.SetPixel(Xcont + 0x7, Ycont + 2, colors[idxs[0x1C][0]]);
                bitmap.SetPixel(Xcont + 0x3, Ycont + 0, colors[idxs[0x1C][1]]);
                bitmap.SetPixel(Xcont + 0xF, Ycont + 2, colors[idxs[0x1D][0]]);
                bitmap.SetPixel(Xcont + 0xB, Ycont + 0, colors[idxs[0x1D][1]]);
                bitmap.SetPixel(Xcont + 0x17, Ycont + 2, colors[idxs[0x1E][0]]);
                bitmap.SetPixel(Xcont + 0x13, Ycont + 0, colors[idxs[0x1E][1]]);
                bitmap.SetPixel(Xcont + 0x1F, Ycont + 2, colors[idxs[0x1F][0]]);
                bitmap.SetPixel(Xcont + 0x1B, Ycont + 0, colors[idxs[0x1F][1]]);

            }


        }


    }


    public class TplImageHeader 
    {
        public byte[] Line;

        public ushort Width;
        public ushort Height;
        public ushort BitDepth;
        public ushort Interlace;
        public ushort Next;
        public ushort MipmapStatus;
        public ushort Qwc;
        public ushort Ref;

        public uint MipmapHeader1Offset;
        public uint MipmapHeader2Offset;
        public ulong GsMip;

        public uint IndexesOffset;
        public uint PaletteOffset;
        public ulong GsTex;
       
        public TplImageHeader MipmapHeader1 = null;
        public TplImageHeader MipmapHeader2 = null;
    }

    public class TplImageHeaderAssistant 
    {

        public TplImageHeaderAssistant()
        {
        }

        public TplImageHeader Fill(ref BinaryReader br, uint Offset) 
        {
            TplImageHeader tih = new TplImageHeader();

            br.BaseStream.Position = Offset;

            byte[] subheader = br.ReadBytes(0x30);
            tih.Line = subheader;

            ushort width = BitConverter.ToUInt16(subheader, 0);
            tih.Width = width;

            ushort height = BitConverter.ToUInt16(subheader, 2);
            tih.Height = height;

            ushort bitDepth = BitConverter.ToUInt16(subheader, 4);
            tih.BitDepth = bitDepth;

            ushort interlace = BitConverter.ToUInt16(subheader, 6);
            tih.Interlace = interlace;

            ushort unk5 = BitConverter.ToUInt16(subheader, 8);
            tih.Next = unk5;

            //mipmapStatus
            //nesse campo somente existe dois valores atribuiveis:
            // 0x0: não tem conteudo mipmap
            // 0x2 contem o conteudo mipmap
            ushort mipmapStatus = BitConverter.ToUInt16(subheader, 0XA);
            tih.MipmapStatus = mipmapStatus;

            ushort unk7 = BitConverter.ToUInt16(subheader, 0xC);
            tih.Qwc = unk7;

            ushort unk8 = BitConverter.ToUInt16(subheader, 0xE);
            tih.Ref = unk8;

            uint mipmapHeader1Offset = BitConverter.ToUInt32(subheader, 0x10);
            tih.MipmapHeader1Offset = mipmapHeader1Offset;

            uint mipmapHeader2Offset = BitConverter.ToUInt32(subheader, 0x14);
            tih.MipmapHeader2Offset = mipmapHeader2Offset;

            ulong gsMip = BitConverter.ToUInt64(subheader, 0x18);
            tih.GsMip = gsMip;

            uint indexesOffset = BitConverter.ToUInt32(subheader, 0x20);
            tih.IndexesOffset = indexesOffset;

            uint paletteOffset = BitConverter.ToUInt32(subheader, 0x24);
            tih.PaletteOffset = paletteOffset;

            ulong gsTex = BitConverter.ToUInt64(subheader, 0x28);
            tih.GsTex = gsTex;

            return tih;
        }

        public void SetText(ref AltTextWriter text, TplImageHeader tih) 
        {
            text.WriteLine("Width: " + tih.Width);
            text.WriteLine("Height: " + tih.Height);
            text.WriteLine("BitDepth: 0x" + tih.BitDepth.ToString("X4"));
            text.WriteLine("Interlace: 0x" + tih.Interlace.ToString("X4"));
            text.WriteLine("Next: 0x" + tih.Next.ToString("X4"));
            text.WriteLine("MipmapStatus: " + tih.MipmapStatus.ToString("X4"));
            text.WriteLine("Qwc: 0x" + tih.Qwc.ToString("X4"));
            text.WriteLine("Ref: 0x" + tih.Ref.ToString("X4"));
            text.WriteLine("MipmapHeader1Offset: 0x" + tih.MipmapHeader1Offset.ToString("X8"));
            text.WriteLine("MipmapHeader2Offset: 0x" + tih.MipmapHeader2Offset.ToString("X8"));
            text.WriteLine("GsMip: 0x" + tih.GsMip.ToString("X16"));
            text.WriteLine("IndexesOffset: 0x" + tih.IndexesOffset.ToString("X8"));
            text.WriteLine("PaletteOffset: 0x" + tih.PaletteOffset.ToString("X8"));
            text.WriteLine("GsTex: 0x" + tih.GsTex.ToString("X8"));
        }


        public void SetIdxTplMain(ref StreamWriter idx, TplImageHeader tih)
        {
            idx.WriteLine("Width:" + tih.Width);
            idx.WriteLine("Height:" + tih.Height);
            idx.WriteLine("BitDepth:" + tih.BitDepth.ToString("X4"));
            idx.WriteLine("Interlace:" + tih.Interlace.ToString("X4"));
            idx.WriteLine("Next:" + tih.Next.ToString("X4"));
            idx.WriteLine("Qwc:" + tih.Qwc.ToString("X4"));
            idx.WriteLine("Ref:" + tih.Ref.ToString("X4"));
            idx.WriteLine("GsTex:" + tih.GsTex.ToString("X16"));

            if (tih.MipmapStatus != 0)
            {
                SetIdxTplMipmaps(ref idx, tih);
            }
        }

        private void SetIdxTplMipmaps(ref StreamWriter idx, TplImageHeader tih) 
        {
            idx.WriteLine("");
            idx.WriteLine("MipmapStatus:" + tih.MipmapStatus.ToString("X4"));
            idx.WriteLine("GsMip:" + tih.GsMip.ToString("X16"));

            if (tih.MipmapHeader1 != null)
            {
                idx.WriteLine("");
                PrintMipMaptext(ref idx, tih.MipmapHeader1, "Mipmap1_");
            }


            if (tih.MipmapHeader2 != null)
            {
                idx.WriteLine("");
                PrintMipMaptext(ref idx, tih.MipmapHeader2, "Mipmap2_");
            }
        }

        private void PrintMipMaptext(ref StreamWriter idx, TplImageHeader tih, string text) 
        {
            idx.WriteLine(text + "Width:" + tih.Width);
            idx.WriteLine(text + "Height:" + tih.Height);
            idx.WriteLine(text + "BitDepth:" + tih.BitDepth.ToString("X4"));
            idx.WriteLine(text + "Interlace:" + tih.Interlace.ToString("X4"));
            idx.WriteLine(text + "Next:" + tih.Next.ToString("X4"));
            idx.WriteLine(text + "Qwc:" + tih.Qwc.ToString("X4"));
            idx.WriteLine(text + "Ref:" + tih.Ref.ToString("X4"));
            idx.WriteLine(text + "GsTex:" + tih.GsTex.ToString("X16"));
            idx.WriteLine(text + "MipmapStatus:" + tih.MipmapStatus.ToString("X4"));
            idx.WriteLine(text + "GsMip:" + tih.GsMip.ToString("X16"));
        }

    }


    public enum ImageFormat 
    {
    TGA,
    PNG,
    BMP,
    GIF
    }


    //AltTextWriter
    public class AltTextWriter
    {
        private TextWriter text;

        public AltTextWriter(string Filepatch, bool Create)
        {
            if (Create)
            {
                text = new FileInfo(Filepatch).CreateText();
            }

        }

        public void WriteLine(string text)
        {
            if (this.text != null)
            {
                this.text.WriteLine(text);
            }
        }

        public void Close()
        {
            if (this.text != null)
            {
                this.text.Close();
            }
        }
    }

}


/*
anotações:
mode/bitDepth:
0008 // uses 4-bit indices to point to a palette of 16 colors.
0009 // uses 8-bit indices to point to a palette of 256 colors.
0006 // uses no palette, and is a stream of 32-bit color pixels.

interlacing:
0000 // rgba
0001 // rgba
0002 // PS2 Swizzle
0003 // PS2 Swizzle



*/
