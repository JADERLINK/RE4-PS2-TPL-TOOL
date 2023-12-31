using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TPL_PS2_REPACK
{
    public static class IdxPs2TplLoader
    {
        public static IdxPs2Tpl Loader(string idxPath) 
        {
            IdxPs2Tpl idx = new IdxPs2Tpl();

            StreamReader stream = File.OpenText(idxPath);
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

            //------
 
            if (pair.ContainsKey("IMAGEFLIPY"))
            {
                try
                {
                    idx.ImageFlipY = bool.Parse(pair["IMAGEFLIPY"].Trim());
                }
                catch (Exception)
                {
                }
            }

            if (pair.ContainsKey("IMAGEFOLDER"))
            {
                try
                {
                    string value = pair["IMAGEFOLDER"].Trim();
                    value = value.Replace("/", "").Replace("\\", "")
                  .Replace(":", "").Replace("*", "").Replace("\"", "").Replace("|", "")
                  .Replace("<", "").Replace(">", "").Replace("?", "");

                    if (value.Length == 0)
                    {
                        value = "null";
                    }
                    idx.ImageFolder = value;
                }
                catch (Exception)
                {
                }
            }

            if (pair.ContainsKey("IMAGEFORMAT"))
            {
                try
                {
                    string value = pair["IMAGEFORMAT"].Trim();
                    value = value.Replace("/", "").Replace("\\", "")
                  .Replace(":", "").Replace("*", "").Replace("\"", "").Replace("|", "")
                  .Replace("<", "").Replace(">", "").Replace("?", "");

                    if (value.Length == 0)
                    {
                        value = "TGA";
                    }
                    idx.ImageFormat = value;
                }
                catch (Exception)
                {
                }
            }

            //-----

            return idx;
        }


    }

    public class IdxPs2Tpl 
    {
        public bool ImageFlipY = false;
        public string ImageFolder = "null";
        public string ImageFormat = "TGA";
    }



}
