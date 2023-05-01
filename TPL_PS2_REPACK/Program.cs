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
        static void Main(string[] args)
        {
            Console.WriteLine("Start");
            Console.WriteLine("TPL_PS2_REPACK Version A.1.0.0.1");

            if (args.Length >= 1 && File.Exists(args[0]) && new FileInfo(args[0]).Extension.ToUpper() == ".IDXTPL")
            {

                Console.WriteLine(args[0]);

                try
                {
                    var fileinfo = new FileInfo(args[0]);
                    string parentDirectory = fileinfo.DirectoryName;
                    if (parentDirectory[parentDirectory.Length - 1] != '\\')
                    {
                        parentDirectory += "\\";
                    }

                    string tplPath = fileinfo.FullName.Substring(0, fileinfo.FullName.Length - fileinfo.Extension.Length) + ".TPL";
                    TplRepack.Repack(fileinfo.FullName, tplPath, parentDirectory);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex);
                }


            }
            else
            {
                Console.WriteLine("no arguments or invalid file");
            }

            Console.WriteLine("End");

        }
    }
}
