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
        public static string Version = "B.1.1.1 (2024-05-05)";

        public static string headerText()
        {
            return "# github.com/JADERLINK/RE4-PS2-TPL-TOOL" + Environment.NewLine +
                   "# youtube.com/@JADERLINK" + Environment.NewLine +
                   "# RE4_PS2_TPL_EXTRACT" + Environment.NewLine +
                   "# by: JADERLINK" + Environment.NewLine +
                   "# Thanks to \"HardHain\" and \"zatarita\"" + Environment.NewLine +
                  $"# Version {Version}";
        }

        static void Main(string[] args)
        {
            Console.WriteLine(headerText());

            if (args.Length == 0)
            {
                Console.WriteLine("For more information read:");
                Console.WriteLine("https://github.com/JADERLINK/RE4-PS2-TPL-TOOL");
                Console.WriteLine("Press any key to close the console.");
                Console.ReadKey();
            }
            else if (args.Length >= 1 && File.Exists(args[0]))
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
                Console.WriteLine("The file does not exist");
            }

            Console.WriteLine("End");

        }
    }
}
