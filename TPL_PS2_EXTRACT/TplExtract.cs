using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

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
    2024/05/05
    version: B.1.1.1
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
            idxtpl.Close();

            Console.WriteLine("ImageFlipY: " + flipY);
            Console.WriteLine("ImageFolder: " + baseName);
            Console.WriteLine("ImageFormat: " + Enum.GetName(typeof(ImageFormat), imageFormat));
            Console.WriteLine("RotateInterlace1and3: " + rotateInterlace1and3);

            var text = new AltTextWriter(info.FullName + ".Debug.txt2", false); // arquivo de debug desabilitado para a versão release
            text.WriteLine(Program.headerText());
            text.WriteLine(info.FullName);
            text.WriteLine("");

            TplExtractHeaders Teh = new TplExtractHeaders(br);
            var Header = Teh.MainReader();
            Console.WriteLine("TplCount: " + Header.TplCount);

            text.WriteLine("Magic: 0x" + Header.Magic.ToString("X8"));
            text.WriteLine("TplCount: " + Header.TplCount);
            text.WriteLine("StartOffset: 0x" + Header.StartOffset.ToString("X8"));
            text.WriteLine("HeaderUnk1: 0x" + Header.HeaderUnk1.ToString("X8"));
            text.WriteLine("");

            TplImageHeader[] tihs = Teh.Extract(Header.TplCount, Header.StartOffset);

            TplImageHeaderAssistant assistant = new TplImageHeaderAssistant();

            //headers
            for (int i = 0; i < tihs.Length; i++)
            {
                text.WriteLine("ImageID: " + i);

                TplImageHeader tih = tihs[i];
                assistant.SetText(ref text, tih);

                PrintTplImageHeader(tih, "Entry: " + i.ToString("D4"));

                if (tih.MipmapStatus == 0x2)
                {
                    if (tih.MipmapHeader1Offset != 0)
                    {
                        text.WriteLine("");
                        text.WriteLine("REF ImageID: " + i + ",  mipmapHeader1");
                        assistant.SetText(ref text, tih.MipmapHeader1);
                        PrintTplImageHeader(tih.MipmapHeader1, "Mipmap1:   ");
                    }

                    if (tih.MipmapHeader2Offset != 0)
                    {
                        text.WriteLine("");
                        text.WriteLine("REF ImageID: " + i + ",  mipmapHeader2");
                        assistant.SetText(ref text, tih.MipmapHeader2);
                        PrintTplImageHeader(tih.MipmapHeader2, "Mipmap2:   ");
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
            for (int i = 0; i < tihs.Length; i++)
            {
                Bitmap bitmap = null;
                bool AsBitmap = false;

                try
                {
                    AsBitmap = tplImage.GetImage(tihs[i].Width, tihs[i].Height, tihs[i].BitDepth, tihs[i].Interlace, tihs[i].IndexesOffset, tihs[i].PaletteOffset, out bitmap);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
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
                            Console.WriteLine(ex);
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
                            Console.WriteLine(ex);
                        }

                        if (AsBitmap2 && bitmap2 != null)
                        {
                            BitmapSalve(ref bitmap2, imageFormat, baseDiretory + i.ToString("D4") + "_Mipmap2");
                        }

                    }


                }

             
            }


            text.Close();
            br.Close();
        }

        private static void PrintTplImageHeader(TplImageHeader data, string text)
        {
            Console.WriteLine(text + "   Dimension: " + (data.Width + "x" + data.Height).PadRight(9) + "   BitDepth: " + data.BitDepth + "   Interlace: " + data.Interlace);
        }

        private static void BitmapSalve(ref Bitmap bitmap, ImageFormat imageFormat, string name)
        {
            try
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
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving image: "+ name + Environment.NewLine + ex.ToString());
            }
        }

    }


    public class TplImageHeaderAssistant 
    {

        public TplImageHeaderAssistant()
        {
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
    BMP
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
0001 // rgba + rotation
0002 // PS2 Swizzle
0003 // PS2 Swizzle + rotation



*/
