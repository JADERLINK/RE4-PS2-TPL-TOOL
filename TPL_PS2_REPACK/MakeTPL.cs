using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

namespace TPL_PS2_REPACK
{
    public static class MakeTPL
    {
        public static void CreateTPL(string tplPath, ref TplImageHeader[] headers, uint TplCount, uint mipmapTotalCount, string ImageDiretory, bool ImageFlipY, string ImageFormat) 
        {
            Console.WriteLine("If there are errors, they will be informed below:");

            BitmapManager bitmapManager = new BitmapManager(ImageFlipY, ImageDiretory);

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

                    bitmapManager.GetBitmapContent(i.ToString("D4") + "." + ImageFormat, ref headers[i], out mainAllColors, out mainBitmap);

                    if (headers[i].MipmapHeader1 != null)
                    {
                        bitmapManager.GetBitmapContent(i.ToString("D4") + "_Mipmap1." + ImageFormat, ref headers[i].MipmapHeader1, out mipmap1AllColors, out mipmap1Bitmap);
                    }

                    if (headers[i].MipmapHeader2 != null)
                    {
                        bitmapManager.GetBitmapContent(i.ToString("D4") + "_Mipmap2." + ImageFormat, ref headers[i].MipmapHeader2, out mipmap2AllColors, out mipmap2Bitmap);
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
                    TplIndexes.GetImageBytesArry(headers[i].BitDepth, headers[i].Interlace, ref mainBitmap, ref FinalPalette, out IndexArry);
                }
                catch (Exception ex)
                {
                    IndexArry = new byte[0];
                    Console.WriteLine($"Entry: {i.ToString("D4")}, Error creating image content in TPL: [IndexArry] " + Environment.NewLine + ex.Message);
                }


                if (headers[i].MipmapHeader1 != null)
                {
                    try
                    {
                        TplIndexes.GetImageBytesArry(headers[i].MipmapHeader1.BitDepth, headers[i].MipmapHeader1.Interlace, ref mipmap1Bitmap, ref FinalPalette, out mipmap1IndexArry);
                    }
                    catch (Exception ex)
                    {
                        mipmap1IndexArry = new byte[0];
                        Console.WriteLine($"Mipmap1: {i.ToString("D4")}, Error creating image content in TPL: [mipmap1IndexArry] " + Environment.NewLine + ex.Message);
                    }

                }

                if (headers[i].MipmapHeader2 != null)
                {
                    try
                    {
                        TplIndexes.GetImageBytesArry(headers[i].MipmapHeader2.BitDepth, headers[i].MipmapHeader2.Interlace, ref mipmap2Bitmap, ref FinalPalette, out mipmap2IndexArry);
                    }
                    catch (Exception ex)
                    {
                        mipmap2IndexArry = new byte[0];
                        Console.WriteLine($"Mipmap2: {i.ToString("D4")}, Error creating image content in TPL: [mipmap2IndexArry] " + Environment.NewLine + ex.Message);
                    }

                }
                #endregion

                //----
                #region terceira parte, define os offsets
                headers[i].IndexesOffset = offsetIndexCount;

                offsetIndexCount += (uint)IndexArry.Length;

                if (PaletteArry != null && PaletteArry.Length != 0)
                {
                    headers[i].PaletteOffset = offsetIndexCount;
                    offsetIndexCount += (uint)PaletteArry.Length;
                }


                if (headers[i].MipmapHeader1 != null)
                {
                    headers[i].MipmapHeader1Offset = offsetMipmapHeaderCount;
                    offsetMipmapHeaderCount += 0x30;

                    headers[i].MipmapHeader1.IndexesOffset = offsetIndexCount;

                    offsetIndexCount += (uint)mipmap1IndexArry.Length;
                }

                if (headers[i].MipmapHeader2 != null)
                {
                    headers[i].MipmapHeader2Offset = offsetMipmapHeaderCount;
                    offsetMipmapHeaderCount += 0x30;

                    headers[i].MipmapHeader2.IndexesOffset = offsetIndexCount;

                    offsetIndexCount += (uint)mipmap2IndexArry.Length;
                }
                #endregion

                //---
                #region quarta parte, responsavel de colocar o conteudo no TPL
                byte[] line = LineHeaderMaker(ref headers[i]);

                stream.Position = offsetHeadersCount;
                stream.Write(line, 0, line.Length);

                stream.Position = headers[i].IndexesOffset;
                stream.Write(IndexArry, 0, IndexArry.Length);

                if (PaletteArry != null && PaletteArry.Length != 0)
                {
                    stream.Position = headers[i].PaletteOffset;
                    stream.Write(PaletteArry, 0, PaletteArry.Length);
                }

                if (headers[i].MipmapHeader1 != null)
                {
                    byte[] mipmapHeader1line = LineHeaderMaker(ref headers[i].MipmapHeader1);

                    stream.Position = headers[i].MipmapHeader1Offset;
                    stream.Write(mipmapHeader1line, 0, mipmapHeader1line.Length);

                    stream.Position = headers[i].MipmapHeader1.IndexesOffset;
                    stream.Write(mipmap1IndexArry, 0, mipmap1IndexArry.Length);
                }

                if (headers[i].MipmapHeader2 != null)
                {
                    byte[] mipmapHeader2line = LineHeaderMaker(ref headers[i].MipmapHeader2);

                    stream.Position = headers[i].MipmapHeader2Offset;
                    stream.Write(mipmapHeader2line, 0, mipmapHeader2line.Length);

                    stream.Position = headers[i].MipmapHeader2.IndexesOffset;
                    stream.Write(mipmap2IndexArry, 0, mipmap2IndexArry.Length);
                }

                offsetHeadersCount += 0x30;
                #endregion

            }

            stream.Close();

            Console.WriteLine("TPL file has been created, if there are errors above, correct them for the file to work correctly.");
        }

        private static byte[] LineHeaderMaker(ref TplImageHeader header)
        {
            byte[] arr = new byte[0x30];
            byte[] _width = BitConverter.GetBytes(header.Width);
            arr[0] = _width[0];
            arr[1] = _width[1];
            byte[] _height = BitConverter.GetBytes(header.Height);
            arr[2] = _height[0];
            arr[3] = _height[1];

            byte[] _bitDepth = BitConverter.GetBytes(header.BitDepth);
            arr[4] = _bitDepth[0];
            arr[5] = _bitDepth[1];

            byte[] _interlace = BitConverter.GetBytes(header.Interlace);
            arr[6] = _interlace[0];
            arr[7] = _interlace[1];

            byte[] _next = BitConverter.GetBytes(header.Next);
            arr[8] = _next[0];
            arr[9] = _next[1];

            byte[] _mipmapStatus = BitConverter.GetBytes(header.MipmapStatus);
            arr[0xA] = _mipmapStatus[0];
            arr[0xB] = _mipmapStatus[1];

            byte[] _qwc = BitConverter.GetBytes(header.Qwc);
            arr[0xC] = _qwc[0];
            arr[0xD] = _qwc[1];

            byte[] _ref = BitConverter.GetBytes(header.Ref);
            arr[0xE] = _ref[0];
            arr[0xF] = _ref[1];

            //-----

            byte[] _mipmapHeader1Offset = BitConverter.GetBytes(header.MipmapHeader1Offset);
            arr[0x10] = _mipmapHeader1Offset[0];
            arr[0x11] = _mipmapHeader1Offset[1];
            arr[0x12] = _mipmapHeader1Offset[2];
            arr[0x13] = _mipmapHeader1Offset[3];

            byte[] _mipmapHeader2Offset = BitConverter.GetBytes(header.MipmapHeader2Offset);
            arr[0x14] = _mipmapHeader2Offset[0];
            arr[0x15] = _mipmapHeader2Offset[1];
            arr[0x16] = _mipmapHeader2Offset[2];
            arr[0x17] = _mipmapHeader2Offset[3];

            byte[] _gsMip = BitConverter.GetBytes(header.GsMip);
            arr[0x18] = _gsMip[0];
            arr[0x19] = _gsMip[1];
            arr[0x1A] = _gsMip[2];
            arr[0x1B] = _gsMip[3];
            arr[0x1C] = _gsMip[4];
            arr[0x1D] = _gsMip[5];
            arr[0x1E] = _gsMip[6];
            arr[0x1F] = _gsMip[7];

            byte[] _indexesOffset = BitConverter.GetBytes(header.IndexesOffset);
            arr[0x20] = _indexesOffset[0];
            arr[0x21] = _indexesOffset[1];
            arr[0x22] = _indexesOffset[2];
            arr[0x23] = _indexesOffset[3];

            byte[] _paletteOffset = BitConverter.GetBytes(header.PaletteOffset);
            arr[0x24] = _paletteOffset[0];
            arr[0x25] = _paletteOffset[1];
            arr[0x26] = _paletteOffset[2];
            arr[0x27] = _paletteOffset[3];

            byte[] _gsTex = BitConverter.GetBytes(header.GsTex);
            arr[0x28] = _gsTex[0];
            arr[0x29] = _gsTex[1];
            arr[0x2A] = _gsTex[2];
            arr[0x2B] = _gsTex[3];
            arr[0x2C] = _gsTex[4];
            arr[0x2D] = _gsTex[5];
            arr[0x2E] = _gsTex[6];
            arr[0x2F] = _gsTex[7];

            return arr;
        }

    }
}
