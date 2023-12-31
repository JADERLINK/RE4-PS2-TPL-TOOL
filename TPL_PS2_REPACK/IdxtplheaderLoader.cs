using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TPL_PS2_REPACK
{
    public static class IdxtplheaderLoader
    {
        public static TplImageHeader Loader(string Filepath) 
        {
            StreamReader stream = File.OpenText(Filepath);
            Dictionary<string, string> pair = new Dictionary<string, string>();

            string line = "";
            while (line != null)
            {
                line = stream.ReadLine();

                if (line != null && line.Length != 0)
                {
                    line = line.Trim();

                    if (!line.StartsWith("#") && !line.StartsWith(":") && !line.StartsWith("/"))
                    {
                        var split = line.Split(new char[] { ':' });
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
            }

            stream.Close();


            TplImageHeader header = GetHeaderInfo(ref pair);

            if (header.MipmapStatus != 0)
            {
                header.MipmapHeader1 = GetHeaderInfo(ref pair, "Mipmap1_".ToUpperInvariant());
                header.MipmapHeader2 = GetHeaderInfo(ref pair, "Mipmap2_".ToUpperInvariant());
            }

            return header;
        }


        private static TplImageHeader GetHeaderInfo(ref Dictionary<string, string> pair, string Subtext = "")
        {
            ushort _width = 0;
            ushort _height = 0;
            ushort _bitDepth = 0;
            ushort _interlace = 0;
            ushort _next = 0;
            ushort _qwc = 0;
            ushort _ref = 0;
            ulong _GsTex = 0;

            //mipmap
            ushort _mipmapStatus = 0;
            ulong _GsMip = 0;

            //keys
            string key_width = (Subtext + "Width").ToUpperInvariant();
            string key_height = (Subtext + "Height").ToUpperInvariant();
            string key_bitDepth = (Subtext + "BitDepth").ToUpperInvariant();
            string key_interlace = (Subtext + "Interlace").ToUpperInvariant();
            string key_next = (Subtext + "Next").ToUpperInvariant();
            string key_qwc = (Subtext + "Qwc").ToUpperInvariant();
            string key_ref = (Subtext + "Ref").ToUpperInvariant();
            string key_gsTex = (Subtext + "GsTex").ToUpperInvariant();
            string key_mipmapStatus = (Subtext + "MipmapStatus").ToUpperInvariant();
            string key_gsMip = (Subtext + "GsMip").ToUpperInvariant();

            //------------
            if (pair.ContainsKey(key_width))
            {
                try
                {
                    _width = ushort.Parse(Utils.ReturnValidDecValue(pair[key_width].Trim()), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                }
            }

            if (pair.ContainsKey(key_height))
            {
                try
                {
                    _height = ushort.Parse(Utils.ReturnValidDecValue(pair[key_height].Trim()), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                }
            }


            if (pair.ContainsKey(key_bitDepth))
            {
                try
                {
                    _bitDepth = ushort.Parse(Utils.ReturnValidHexValue(pair[key_bitDepth].Trim()), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
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

            if (pair.ContainsKey(key_next))
            {
                try
                {
                    _next = ushort.Parse(Utils.ReturnValidHexValue(pair[key_next].Trim()), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                }
            }

            if (pair.ContainsKey(key_qwc))
            {
                try
                {
                    _qwc = ushort.Parse(Utils.ReturnValidHexValue(pair[key_qwc].Trim()), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                }
            }

            if (pair.ContainsKey(key_ref))
            {
                try
                {
                    _ref = ushort.Parse(Utils.ReturnValidHexValue(pair[key_ref].Trim()), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
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

            if (pair.ContainsKey(key_gsTex))
            {
                try
                {
                    _GsTex = ulong.Parse(Utils.ReturnValidHexValue(pair[key_gsTex].Trim()), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                }
            }

            if (pair.ContainsKey(key_gsMip))
            {
                try
                {
                    _GsMip = ulong.Parse(Utils.ReturnValidHexValue(pair[key_gsMip].Trim()), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                }
            }
            //--------------

            TplImageHeader header = new TplImageHeader();
            header.Width = _width;
            header.Height = _height;
            header.BitDepth = _bitDepth;
            header.Interlace = _interlace;
            header.Next = _next;
            header.Qwc = _qwc;
            header.Ref = _ref;
            header.GsTex = _GsTex;
            header.MipmapStatus = _mipmapStatus;
            header.GsMip = _GsMip;
            return header;
        }



    }
}
