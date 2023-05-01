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
    30-04-2023
    version: alfa.1.0.0.1
    */


    public static class TplRepack
    {

        public static void Repack(string idxPath, string tplPath, string parentDirectory)
        {
            StreamReader idx = File.OpenText(idxPath);

            Dictionary<string, string> pair = new Dictionary<string, string>();

            List<string> lines = new List<string>();

            string endLine = "";
            while (endLine != null)
            {
                endLine = idx.ReadLine();
                lines.Add(endLine);
            }

            idx.Close();

            foreach (var item in lines)
            {
                if (item != null)
                {
                    var split = item.Split(new char[] { ':' });
                    if (split.Length >= 2)
                    {
                        string key = split[0].ToUpper().Trim();
                        if (!pair.ContainsKey(key))
                        {
                            pair.Add(key, split[1]);
                        }
                    }
                }
            }

            lines.Clear();

            // ------

            int TplCount = 0;
            bool ImagesFlipY = false;
            bool ImagesRotateInterlace1and3 = false;

            if (pair.ContainsKey("TPLCOUNT"))
            {
                try
                {
                    TplCount = ushort.Parse(Utils.ReturnValidDecValue(pair["TPLCOUNT"].Trim()), System.Globalization.CultureInfo.InvariantCulture);
                    if (TplCount > 255)
                    {
                        TplCount = 255;
                    }
                }
                catch (Exception)
                {
                }
            }

            if (pair.ContainsKey("IMAGESFLIPY"))
            {
                try
                {
                    ImagesFlipY = bool.Parse(pair["IMAGESFLIPY"].Trim());
                }
                catch (Exception)
                {
                }
            }

            if (pair.ContainsKey("IMAGESROTATEINTERLACE1AND3"))
            {
                try
                {
                    ImagesRotateInterlace1and3 = bool.Parse(pair["IMAGESROTATEINTERLACE1AND3"].Trim());
                }
                catch (Exception)
                {
                }
            }

            //-----

            Console.WriteLine("TplCount: " + TplCount);
            Console.WriteLine("ImagesFlipY: " + ImagesFlipY);
            Console.WriteLine("ImagesRotateInterlace1and3: " + ImagesRotateInterlace1and3);

            TplImageHeader[] headers = new TplImageHeader[TplCount];

            int mipmapTotalCount = 0;

            //primeiro "for", obtem os dados de "headers"
            for (int i = 0; i < TplCount; i++)
            {
                var data = GetHeaderInfo(ref pair, i, "");

                if (data.mipmapStatus != 0)
                {
                    var mipmap1 = GetHeaderInfo(ref pair, i, "_mipmap1");
                    data.mipmapHeader1 = mipmap1;
                    mipmapTotalCount++;
                }

                if (data.mipmapStatus > 1)
                {
                    var mipmap2 = GetHeaderInfo(ref pair, i, "_mipmap2");
                    data.mipmapHeader2 = mipmap2;
                    mipmapTotalCount++;
                }

                headers[i] = data;
            }

            if (mipmapTotalCount != 0)
            {
                Console.WriteLine("MipmapTotalCount: " + mipmapTotalCount);
            }

            BitmapManager bitmapManager = new BitmapManager(ImagesFlipY, ImagesRotateInterlace1and3, parentDirectory);


            #region variaveis da quarta parte, respostavel pela criação do arquivo TPL
            var stream = new FileInfo(tplPath).Open(FileMode.Create);

            byte[] tplMainHeader = new byte[0x10];
            tplMainHeader[0x1] = 0x10;
            tplMainHeader[0x4] = (byte)TplCount;
            tplMainHeader[0x8] = 0x10;
            stream.Write(tplMainHeader, 0, tplMainHeader.Length);

            uint offsetHeadersCount = 0x10;
            #endregion


            #region variaveis da terceira parte, define os offsets
            //ajustas offsets
            uint startOffsetIndex = (uint)(0x10 + ((TplCount + mipmapTotalCount) * 0x30));
            uint startOffsetMipmapHeader = (uint)(0x10 + (TplCount * 0x30));

            uint offsetIndexCount = startOffsetIndex;
            uint offsetMipmapHeaderCount = startOffsetMipmapHeader;
            #endregion

            // principal
            for (int i = 0; i < TplCount; i++)
            {
                //dados que vão no arquio TPL
                byte[] PaletteArry = null;
                byte[] IndexArry = null;
                byte[] mipmap1IndexArry = null;
                byte[] mipmap2IndexArry = null;

                //dados da primeira e segunda parte
                SimpleBitmap mainBitmap = null;
                SimpleBitmap mipmap1Bitmap = null;
                SimpleBitmap mipmap2Bitmap = null;
                Color[] FinalPalette = null;

                //----
                #region primeira parte, obtem bitmap, e "palette"
                {
                    Dictionary<Color, int> mainAllColors = null;
                    Dictionary<Color, int> mipmap1AllColors = null;
                    Dictionary<Color, int> mipmap2AllColors = null;

                    bitmapManager.GetBitmapContent(ref headers[i], out mainAllColors, out mainBitmap);

                    if (headers[i].mipmapHeader1 != null)
                    {
                        bitmapManager.GetBitmapContent(ref headers[i].mipmapHeader1, out mipmap1AllColors, out mipmap1Bitmap);
                    }

                    if (headers[i].mipmapHeader2 != null)
                    {
                        bitmapManager.GetBitmapContent(ref headers[i].mipmapHeader2, out mipmap2AllColors, out mipmap2Bitmap);
                    }

                    TplPalette.getPalette(i, ref headers[i], ref mainAllColors, ref mipmap1AllColors, ref mipmap2AllColors, out FinalPalette);


                    if (FinalPalette.Length == 16)
                    {
                        PaletteArry = TplPalette.CreatePaletteArryBitDepth0x8(FinalPalette);
                    }
                    else if (FinalPalette.Length == 256)
                    {
                        PaletteArry = TplPalette.CreatePaletteArryBitDepth0x9(FinalPalette);
                    }


                }
                #endregion

                //----
                #region segunda parte, cria os dados do "IndexArry"
                try
                {
                    TplIndexes.GetImageBytesArry(headers[i].bitDepth, headers[i].interlace, ref mainBitmap, ref FinalPalette, out IndexArry);
                }
                catch (Exception ex)
                {
                    IndexArry = new byte[0];
                    Console.WriteLine($"[{i}] IndexArry, error: "  + ex);
                }


                if (headers[i].mipmapHeader1 != null)
                {
                    try
                    {
                        TplIndexes.GetImageBytesArry(headers[i].mipmapHeader1.bitDepth, headers[i].mipmapHeader1.interlace, ref mipmap1Bitmap, ref FinalPalette, out mipmap1IndexArry);
                    }
                    catch (Exception ex)
                    {
                        mipmap1IndexArry = new byte[0];
                        Console.WriteLine($"[{i}] mipmap1IndexArry, error: " + ex);
                    }

                }

                if (headers[i].mipmapHeader2 != null)
                {
                    try
                    {
                        TplIndexes.GetImageBytesArry(headers[i].mipmapHeader2.bitDepth, headers[i].mipmapHeader2.interlace, ref mipmap2Bitmap, ref FinalPalette, out mipmap2IndexArry);
                    }
                    catch (Exception ex)
                    {
                        mipmap2IndexArry = new byte[0];
                        Console.WriteLine($"[{i}] mipmap2IndexArry, error: " + ex);
                    }

                }
                #endregion

                //----
                #region terceira parte, define os offsets
                headers[i].indexesOffset = offsetIndexCount;

                offsetIndexCount += (uint)IndexArry.Length;

                if (PaletteArry != null && PaletteArry.Length != 0)
                {
                    headers[i].paletteOffset = offsetIndexCount;
                    offsetIndexCount += (uint)PaletteArry.Length;
                }
                

                if (headers[i].mipmapHeader1 != null)
                {
                    headers[i].mipmapHeader1Offset = offsetMipmapHeaderCount;
                    offsetMipmapHeaderCount += 0x30;

                    headers[i].mipmapHeader1.indexesOffset = offsetIndexCount;

                    offsetIndexCount += (uint)mipmap1IndexArry.Length;
                }

                if (headers[i].mipmapHeader2 != null)
                {
                    headers[i].mipmapHeader2Offset = offsetMipmapHeaderCount;
                    offsetMipmapHeaderCount += 0x30;

                    headers[i].mipmapHeader2.indexesOffset = offsetIndexCount;

                    offsetIndexCount += (uint)mipmap2IndexArry.Length;
                }
                #endregion

                //---
                #region quarta parte, responsavel de colocar o conteudo no TPL
                byte[] line = LineHeaderMaker(ref headers[i]);

                stream.Position = offsetHeadersCount;
                stream.Write(line, 0, line.Length);

                stream.Position = headers[i].indexesOffset;
                stream.Write(IndexArry, 0, IndexArry.Length);

                if (PaletteArry != null && PaletteArry.Length != 0)
                {
                    stream.Position = headers[i].paletteOffset;
                    stream.Write(PaletteArry, 0, PaletteArry.Length);
                }

                if (headers[i].mipmapHeader1 != null)
                {
                    byte[] mipmapHeader1line = LineHeaderMaker(ref headers[i].mipmapHeader1);

                    stream.Position = headers[i].mipmapHeader1Offset;
                    stream.Write(mipmapHeader1line, 0, mipmapHeader1line.Length);

                    stream.Position = headers[i].mipmapHeader1.indexesOffset;
                    stream.Write(mipmap1IndexArry, 0, mipmap1IndexArry.Length);
                }

                if (headers[i].mipmapHeader2 != null)
                {
                    byte[] mipmapHeader2line = LineHeaderMaker(ref headers[i].mipmapHeader2);

                    stream.Position = headers[i].mipmapHeader2Offset;
                    stream.Write(mipmapHeader2line, 0, mipmapHeader2line.Length);

                    stream.Position = headers[i].mipmapHeader2.indexesOffset;
                    stream.Write(mipmap2IndexArry, 0, mipmap2IndexArry.Length);
                }

                offsetHeadersCount += 0x30;
                #endregion

            }

            stream.Close();

        }

        private static byte[] LineHeaderMaker(ref TplImageHeader header) 
        {
            byte[] arr = new byte[0x30];
            byte[] width = BitConverter.GetBytes(header.width);
            arr[0] = width[0];
            arr[1] = width[1];
            byte[] height = BitConverter.GetBytes(header.height);
            arr[2] = height[0];
            arr[3] = height[1];

            byte[] bitDepth = BitConverter.GetBytes(header.bitDepth);
            arr[4] = bitDepth[0];
            arr[5] = bitDepth[1];

            byte[] interlace = BitConverter.GetBytes(header.interlace);
            arr[6] = interlace[0];
            arr[7] = interlace[1];

            byte[] unk5 = BitConverter.GetBytes(header.unk5);
            arr[8] = unk5[0];
            arr[9] = unk5[1];

            byte[] mipmapStatus = BitConverter.GetBytes(header.mipmapStatus);
            arr[0xA] = mipmapStatus[0];
            arr[0xB] = mipmapStatus[1];

            byte[] unk7 = BitConverter.GetBytes(header.unk7);
            arr[0xC] = unk7[0];
            arr[0xD] = unk7[1];

            byte[] unk8 = BitConverter.GetBytes(header.unk8);
            arr[0xE] = unk8[0];
            arr[0xF] = unk8[1];

            //-----

            byte[] mipmapHeader1Offset = BitConverter.GetBytes(header.mipmapHeader1Offset);
            arr[0x10] = mipmapHeader1Offset[0];
            arr[0x11] = mipmapHeader1Offset[1];
            arr[0x12] = mipmapHeader1Offset[2];
            arr[0x13] = mipmapHeader1Offset[3];

            byte[] mipmapHeader2Offset = BitConverter.GetBytes(header.mipmapHeader2Offset);
            arr[0x14] = mipmapHeader2Offset[0];
            arr[0x15] = mipmapHeader2Offset[1];
            arr[0x16] = mipmapHeader2Offset[2];
            arr[0x17] = mipmapHeader2Offset[3];

            byte[] unkx18 = BitConverter.GetBytes(header.unkx18);
            arr[0x18] = unkx18[0];
            arr[0x19] = unkx18[1];
            arr[0x1A] = unkx18[2];
            arr[0x1B] = unkx18[3];

            byte[] unkx1C = BitConverter.GetBytes(header.unkx1C);
            arr[0x1C] = unkx1C[0];
            arr[0x1D] = unkx1C[1];
            arr[0x1E] = unkx1C[2];
            arr[0x1F] = unkx1C[3];

            byte[] indexesOffset = BitConverter.GetBytes(header.indexesOffset);
            arr[0x20] = indexesOffset[0];
            arr[0x21] = indexesOffset[1];
            arr[0x22] = indexesOffset[2];
            arr[0x23] = indexesOffset[3];

            byte[] paletteOffset = BitConverter.GetBytes(header.paletteOffset);
            arr[0x24] = paletteOffset[0];
            arr[0x25] = paletteOffset[1];
            arr[0x26] = paletteOffset[2];
            arr[0x27] = paletteOffset[3];

            byte[] unkx28 = BitConverter.GetBytes(header.unkx28);
            arr[0x28] = unkx28[0];
            arr[0x29] = unkx28[1];
            arr[0x2A] = unkx28[2];
            arr[0x2B] = unkx28[3];

            byte[] unkx2C = BitConverter.GetBytes(header.unkx2C);
            arr[0x2C] = unkx2C[0];
            arr[0x2D] = unkx2C[1];
            arr[0x2E] = unkx2C[2];
            arr[0x2F] = unkx2C[3];

            return arr;
        }

        private static TplImageHeader GetHeaderInfo (ref Dictionary<string, string> pair, int ID, string Subtext = "")
        {

            ushort _bitDepth = 0;
            ushort _interlace = 0;
            ushort _unk5 = 0;
            ushort _unk7 = 0;
            ushort _unk8 = 0;

            uint _unkx28 = 0;
            uint _unkx2C = 0;

            string _texturePath = null;

            //mipmap
            ushort _mipmapStatus = 0;
            uint _unkx18 = 0;
            uint _unkx1C = 0;

            //keys
            string id = ID.ToString("D3");
            string key_bitDepth = (id + Subtext + "_bitDepth").ToUpperInvariant();
            string key_interlace = (id + Subtext + "_interlace").ToUpperInvariant();
            string key_unk5 = (id + Subtext + "_unk5").ToUpperInvariant();
            string key_unk7 = (id + Subtext + "_unk7").ToUpperInvariant();
            string key_unk8 = (id + Subtext + "_unk8").ToUpperInvariant();
            string key_unkx28 = (id + Subtext + "_unkx28").ToUpperInvariant();
            string key_unkx2C = (id + Subtext + "_unkx2C").ToUpperInvariant();
            string key_texturePath = (id + Subtext + "_texturePath").ToUpperInvariant();
            string key_mipmapStatus = (id + Subtext + "_mipmapStatus").ToUpperInvariant();
            string key_unkx18 = (id + Subtext + "_unkx18").ToUpperInvariant();
            string key_unkx1C = (id + Subtext + "_unkx1C").ToUpperInvariant();


            if (pair.ContainsKey(key_bitDepth))
            {
                try
                {
                    _bitDepth = ushort.Parse(Utils.ReturnValidHexValue(pair[key_bitDepth].Trim()),System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                }
            }

            if (pair.ContainsKey(key_interlace))
            {
                try
                {
                    _interlace = ushort.Parse(Utils.ReturnValidHexValue(pair[key_interlace].Trim()), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                }
            }

            if (pair.ContainsKey(key_unk5))
            {
                try
                {
                    _unk5 = ushort.Parse(Utils.ReturnValidHexValue(pair[key_unk5].Trim()), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                }
            }

            if (pair.ContainsKey(key_unk7))
            {
                try
                {
                    _unk7 = ushort.Parse(Utils.ReturnValidHexValue(pair[key_unk7].Trim()), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                }
            }

            if (pair.ContainsKey(key_unk8))
            {
                try
                {
                    _unk8 = ushort.Parse(Utils.ReturnValidHexValue(pair[key_unk8].Trim()), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                }
            }

            if (pair.ContainsKey(key_mipmapStatus))
            {
                try
                {
                    _mipmapStatus = ushort.Parse(Utils.ReturnValidHexValue(pair[key_mipmapStatus].Trim()), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                }
            }

            if (pair.ContainsKey(key_unkx28))
            {
                try
                {
                    _unkx28 = uint.Parse(Utils.ReturnValidHexValue(pair[key_unkx28].Trim()), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                }
            }

            if (pair.ContainsKey(key_unkx2C))
            {
                try
                {
                    _unkx2C = uint.Parse(Utils.ReturnValidHexValue(pair[key_unkx2C].Trim()), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                }
            }

            if (pair.ContainsKey(key_unkx18))
            {
                try
                {
                    _unkx18 = uint.Parse(Utils.ReturnValidHexValue(pair[key_unkx18].Trim()), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                }
            }

            if (pair.ContainsKey(key_unkx1C))
            {
                try
                {
                    _unkx1C = uint.Parse(Utils.ReturnValidHexValue(pair[key_unkx1C].Trim()), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                }
            }

            if (pair.ContainsKey(key_texturePath))
            {
                try
                {
                    _texturePath = pair[key_texturePath].Trim().Trim(new char[] {'\\'});
                }
                catch (Exception)
                {
                }
            }

            TplImageHeader header = new TplImageHeader();
            header.bitDepth = _bitDepth;
            header.interlace = _interlace;
            header.unk5 = _unk5;
            header.unk7 = _unk7;
            header.unk8 = _unk8;
            header.unkx28 = _unkx28;
            header.unkx2C = _unkx2C;
            header.texturePath = _texturePath;
            header.mipmapStatus = _mipmapStatus;
            header.unkx18 = _unkx18;
            header.unkx1C = _unkx1C;
            return header;
        }


    }


}
