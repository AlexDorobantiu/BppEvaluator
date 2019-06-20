using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace BppEvaluator
{
    class Program
    {
        static readonly List<string> knownBitmapExtensions = new List<string>() { ".tif", ".bmp", ".tiff", ".png", ".jpg", ".jpeg", ".gif", ".ico", ".emf", ".exif", ".wmf" };
        static readonly List<string> knownNetpbmExtensions = new List<string>() { ".pgm", ".ppm", ".pbm" };
        private const string netpbmCommentSign = "#";
        const int filenamePaddingSize = 40;
        const int bitsInByte = 8;

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
            bool found = iterateFiles(filename, originalFileSize, knownBitmapExtensions, computeBppUsingBitmap);
            if (!found)
            {
                // try netpbm
                found = iterateFiles(filename, originalFileSize, knownNetpbmExtensions, computeBppFromPgm);
            }

            return found;
        }

        private static bool iterateFiles(string filename, long originalFileSize, ICollection<string> extensions, Func<long, string, double> bppComputer)
        {
            foreach (string extension in extensions)
            {
                string newPath = Path.ChangeExtension(filename, extension);
                FileInfo fileInfo = new FileInfo(newPath);
                if (fileInfo.Exists)
                {
                    try
                    {
                        Console.Write(bppComputer(originalFileSize, newPath));
                        return true;
                    }
                    catch
                    {
                        // could not parse the filetype -> ignore
                    }
                }
            }

            return false;
        }

        private static double computeBppFromPgm(long originalFileSize, string newPath)
        {
            string sizeString;
            using (StreamReader streamReader = new StreamReader(newPath))
            {
                streamReader.ReadLine(); // pgm version
                do
                {
                    sizeString = streamReader.ReadLine();
                } while (sizeString.StartsWith(netpbmCommentSign));
            }
            string[] split = sizeString.Split(' ');
            int sizeX = int.Parse(split[0]);
            int sizeY = int.Parse(split[1]);

            return computeBitsPerPixel(originalFileSize, sizeX, sizeY);
        }

        private static double computeBppUsingBitmap(long originalFileSize, string path)
        {
            Bitmap bitmap = new Bitmap(path);
            int sizeX = bitmap.Width;
            int sizeY = bitmap.Height;

            return computeBitsPerPixel(originalFileSize, sizeX, sizeY);
        }

        private static double computeBitsPerPixel(long originalFileSize, int sizeX, int sizeY)
        {
            return originalFileSize * bitsInByte / (double)(sizeX * sizeY);
        }
    }
}