using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace BppEvaluator
{
    class Program
    {
        static readonly List<string> knownBitmapExtensions = new List<string>() { ".tif", ".bmp", ".tiff", ".png", ".jpg", ".jpeg", ".gif", ".ico", ".emf", ".exif", ".wmf" };
        private const string pgmExtension = ".pgm";
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

                bool found = computeBppForFilename(filename, originalFileSize);
                if (!found)
                {
                    // try removing extension and see if it's a concatenated extension
                    string newFilename = Path.GetFileNameWithoutExtension(filename);
                    string newExtension = Path.GetExtension(newFilename);
                    if (newExtension != null && !newExtension.Equals(string.Empty))
                    {
                        computeBppForFilename(newFilename, originalFileSize);
                    }
                }
                Console.WriteLine();
            }
            Console.ReadLine();
        }

        private static bool computeBppForFilename(string filename, long originalFileSize)
        {
            bool found = false;
            foreach (string extension in knownBitmapExtensions)
            {
                string newPath = Path.ChangeExtension(filename, extension);
                FileInfo fileInfo = new FileInfo(newPath);
                if (fileInfo.Exists)
                {
                    try
                    {
                        Console.Write(computeBppUsingBitmap(originalFileSize, newPath));
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
                string newPath = Path.ChangeExtension(filename, pgmExtension);
                FileInfo fileInfo = new FileInfo(newPath);
                if (fileInfo.Exists)
                {
                    try
                    {
                        Console.Write(computeBppFromPgm(originalFileSize, newPath));
                        found = true;
                    }
                    catch
                    {
                        // could not parse the PGM -> ignore
                    }
                }
            }

            return found;
        }

        private static double computeBppFromPgm(long originalFileSize, string newPath)
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
            return originalFileSize * 8 / (double)(sizeX * sizeY);
        }

        private static double computeBppUsingBitmap(long originalFileSize, string path)
        {
            Bitmap bitmap = new Bitmap(path);
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

            return originalFileSize * bpp / (double)(sizeX * sizeY);
        }
    }
}