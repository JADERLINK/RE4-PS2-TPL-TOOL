using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TPL_PS2_EXTRACT
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("start");

            Console.WriteLine("TPL_PS2_EXTRACT Version A.1.0.0.0");

            if (args.Length >= 1 && File.Exists(args[0]))
            {
                ImageFormat imageFormat = ImageFormat.TGA;

                bool flipY = false;
                bool rotateInterlace1and3 = false;

                if (args.Length >= 2)
                {
                    if (args[1].ToUpperInvariant().Contains("TGA"))
                    {
                        imageFormat = ImageFormat.TGA;
                    }
                    else if(args[1].ToUpperInvariant().Contains("PNG"))
                    {
                        imageFormat = ImageFormat.PNG;
                    }
                    else if (args[1].ToUpperInvariant().Contains("GIF"))
                    {
                        imageFormat = ImageFormat.GIF;
                    }
                    else if (args[1].ToUpperInvariant().Contains("BMP"))
                    {
                        imageFormat = ImageFormat.BMP;
                    }
                }

                if (args.Length >= 3 && args[2].ToUpperInvariant().Contains("TRUE"))
                {
                    flipY = true;
                }
                if (args.Length >= 4 && args[3].ToUpperInvariant().Contains("TRUE"))
                {
                    rotateInterlace1and3 = true;
                }

                FileInfo fileInfo = new FileInfo(args[0]);
                Console.WriteLine("File: "+ fileInfo.Name);

                if (fileInfo.Extension.ToUpper() == ".TPL")
                {
                    try
                    {
                        TplExtract.Extract(fileInfo.OpenRead(), fileInfo, imageFormat, flipY, rotateInterlace1and3);
                    }
                    catch (Exception ex)
                    {
                       Console.WriteLine("Error: " + ex);
                    }

                }
                else
                {
                    Console.WriteLine("it is not an TPL file");
                }
            }
            else
            {
                Console.WriteLine("no arguments or invalid file");
            }

            Console.WriteLine("end");



        }
    }
}
