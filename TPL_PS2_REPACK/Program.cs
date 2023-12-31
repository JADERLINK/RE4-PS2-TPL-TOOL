using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TPL_PS2_REPACK
{
    class Program
    {
        public static string Version = "B.1.1.0.0 (2023-12-30)";

        public static string headerText()
        {
            return "# RE4_PS2_TPL_REPACK" + Environment.NewLine +
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
            }
            else if (args.Length >= 1 && File.Exists(args[0]))
            {
                FileInfo fileInfo = new FileInfo(args[0]);
                Console.WriteLine("File: " + fileInfo.Name);

                if (fileInfo.Extension.ToUpper() == ".IDXPS2TPL")
                {
                    try
                    {
                        string parentDirectory = fileInfo.DirectoryName + "\\";
                        string tplPath = fileInfo.FullName.Substring(0, fileInfo.FullName.Length - fileInfo.Extension.Length) + ".TPL";
                        TplRepack.Repack(fileInfo.FullName, tplPath, parentDirectory);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex);
                   }
                }
                else
                {
                    Console.WriteLine("it is not an IDXPS2TPL file");
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
