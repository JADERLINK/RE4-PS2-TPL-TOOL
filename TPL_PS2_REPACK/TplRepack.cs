using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

namespace TPL_PS2_REPACK
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


    public static class TplRepack
    {

        public static void Repack(string idxPath, string tplPath, string parentDirectory)
        {
            var idxtpl = IdxPs2TplLoader.Loader(idxPath);

            Console.WriteLine("ImageFlipY: " + idxtpl.ImageFlipY);
            Console.WriteLine("ImageFolder: " + idxtpl.ImageFolder);
            Console.WriteLine("ImageFormat: " + idxtpl.ImageFormat);

            //------
            string ImageDiretory = parentDirectory + idxtpl.ImageFolder + "\\";

            if (!Directory.Exists(ImageDiretory))
            {
                Console.WriteLine("The directory does not exist: " + ImageDiretory);
                Console.WriteLine("A .TPL file was not created;");
                return;
            }
            //---------

            uint TplCount = 0; // quantidade de imagens
            bool asFile = true;

            while (asFile)
            {
                string path = ImageDiretory + TplCount.ToString("D4") + ".idxtplheader";

                if (File.Exists(path))
                {
                    TplCount++;
                }
                else
                {
                    asFile = false;
                }
            }

            Console.WriteLine("TplCount: " + TplCount);

            //---------------
 
            TplImageHeader[] headers = new TplImageHeader[TplCount];

            uint mipmapTotalCount = 0;

            //primeiro "for", obtem os dados de "headers"
            for (int i = 0; i < TplCount; i++)
            {
                string path = ImageDiretory + i.ToString("D4") + ".idxtplheader";

                var data = IdxtplheaderLoader.Loader(path);

                PrintTplImageHeader(data, "Entry: " + i.ToString("D4"));

                if (data.MipmapStatus != 0)
                {
                    PrintTplImageHeader(data.MipmapHeader1, "Mipmap1:   ");
                    mipmapTotalCount++;
                }

                if (data.MipmapStatus > 1)
                {
                    PrintTplImageHeader(data.MipmapHeader2, "Mipmap2:   ");
                    mipmapTotalCount++;
                }

                headers[i] = data;
            }

            if (mipmapTotalCount != 0)
            {
                Console.WriteLine("MipmapTotalCount: " + mipmapTotalCount);
            }

            // le o arquivos de imagens, e cria o arquivo tpl
            MakeTPL.CreateTPL(tplPath, ref headers, TplCount, mipmapTotalCount, ImageDiretory, idxtpl.ImageFlipY, idxtpl.ImageFormat);
        }

        private static void PrintTplImageHeader(TplImageHeader data, string text)
        {
            Console.WriteLine(text + "   Dimension: " + (data.Width + "x" + data.Height).PadRight(9) + "   BitDepth: " + data.BitDepth + "   Interlace: " + data.Interlace);
        }

    }


}
