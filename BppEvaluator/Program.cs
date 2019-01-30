using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace BppEvaluator
{
    class Program
    {
        static List<string> knownExtensions = new List<string>() { ".tif", ".bmp", ".tiff", ".png", ".jpg", ".jpeg", ".gif", ".ico", ".emf", ".exif", ".wmf" };
        const int filenamePaddingSize = 40;

        static void Main(string[] args)
        {
            Array.Sort(args);
            foreach (string filename in args)
            {
                FileInfo originalFileInfo = new FileInfo(filename);
                if (!originalFileInfo.Exists)
                {
                    continue;
                }
                long originalFileSize = originalFileInfo.Length;
                Console.Write(originalFileInfo.Name.PadRight(filenamePaddingSize));

                bool found = false;
                foreach (string extension in knownExtensions)
                {
                    string newPath = Path.ChangeExtension(filename, extension);
                    FileInfo fileInfo = new FileInfo(newPath);
                    if (fileInfo.Exists)
                    {
                        try
                        {
                            Bitmap bitmap = new Bitmap(newPath);
                            int sizeX = bitmap.Width;
                            int sizeY = bitmap.Height;

                            int bpp = 8; // default 8
                            switch (bitmap.PixelFormat)
                            {
                                case PixelFormat.Format32bppArgb:
                                    bpp = 32;
                                    break;
                                case PixelFormat.Format24bppRgb:
                                    bpp = 24;
                                    break;
                                case PixelFormat.Format8bppIndexed:
                                    bpp = 8;
                                    break;
                                case PixelFormat.Format4bppIndexed:
                                    bpp = 4;
                                    break;
                                case PixelFormat.Format1bppIndexed:
                                    bpp = 1;
                                    break;
                            }

                            Console.Write(originalFileSize * bpp / (double)(sizeX * sizeY));
                            found = true;
                            break;
                        }
                        catch
                        {
                            // could not open bitmap file -> ignore
                        }
                    }
                }
                if (!found)
                {
                    // try .pgm
                    string newPath = Path.ChangeExtension(filename, ".pgm");
                    FileInfo fileInfo = new FileInfo(newPath);
                    if (fileInfo.Exists)
                    {
                        try
                        {
                            string sizeString;
                            using (StreamReader streamReader = new StreamReader(newPath))
                            {
                                streamReader.ReadLine(); // pgm version
                                sizeString = streamReader.ReadLine();
                            }
                            string[] split = sizeString.Split(' ');
                            int sizeX = int.Parse(split[0]);
                            int sizeY = int.Parse(split[1]);
                            // bpp for pgm is 8
                            Console.Write(originalFileSize * 8 / (double)(sizeX * sizeY));
                        }
                        catch
                        {
                            // could not parse the PGM -> ignore
                        }
                    }
                }
                Console.WriteLine();
            }
            Console.ReadLine();
        }
    }
}