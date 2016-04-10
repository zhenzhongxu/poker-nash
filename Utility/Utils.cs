using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace GameTreeDraft.Utility
{
    public static class Utils
    {

        public static long[] ReadFileIntoLongArray(FileInfo file)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            long contentLength = file.Length;
            long[] ret = new long[contentLength / sizeof(long)];
            long currentIndex = 0;
            const int increments = 1024 * 1024 * 10;


            long count = 0;
            using (FileStream fs = file.OpenRead())
            {
                while (currentIndex < contentLength)
                {
                    int actualIncrements;
                    if ((currentIndex + increments) < contentLength)
                    {
                        actualIncrements = increments;
                    }
                    else
                    {
                        actualIncrements = checked((int)(contentLength - currentIndex));
                    }

                    byte[] byteArray = new byte[actualIncrements];
                    int num = fs.Read(byteArray, 0, actualIncrements);
                    Buffer.BlockCopy(byteArray, 0, ret, checked((int) currentIndex),
                        actualIncrements);

                    currentIndex += actualIncrements;
                    count+=actualIncrements;
                }
            }

            return ret;
        }

        public static int[] ReadFileIntoIntArray(FileInfo file)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            long contentLength = file.Length;
            int[] ret = new int[contentLength / sizeof(int)];
            long currentIndex = 0;
            const int increments = 1024 * 1024 * 10;


            long count = 0;
            using (FileStream fs = file.OpenRead())
            {
                while (currentIndex < contentLength)
                {
                    int actualIncrements;
                    if ((currentIndex + increments) < contentLength)
                    {
                        actualIncrements = increments;
                    }
                    else
                    {
                        actualIncrements = checked((int)(contentLength - currentIndex));
                    }

                    byte[] byteArray = new byte[actualIncrements];
                    int num = fs.Read(byteArray, 0, actualIncrements);
                    Buffer.BlockCopy(byteArray, 0, ret, checked((int)currentIndex),
                        actualIncrements);

                    currentIndex += actualIncrements;
                    count += actualIncrements;
                }
            }

            return ret;
        }

        public static byte[] ReadFileIntoByteArray(FileInfo file)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            byte[] ret = new byte[file.Length];

            using (FileStream fs = file.OpenRead())
            {
                fs.Read(ret, 0, ret.Length);
            }

            return ret;
        }
    }
}
