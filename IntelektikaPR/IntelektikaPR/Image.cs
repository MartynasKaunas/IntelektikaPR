using System;
using System.Drawing;

namespace IntelektikaPR
{
    public class Image
    {
        public Bitmap Bitmap { get; }

        public Image(Bitmap bitmap)
        {
            Bitmap = bitmap;
        }

        /// <summary>
        /// Converts the given Image object to a 2D [width, height] byte array.
        /// Each byte consists of grayscale 0-255 value for the image.
        /// </summary>
        /// <returns>Byte array consisting of values 0-255 for each element.</returns>
        public byte[,] ToByteMatrix()
        {
            var matrix = new byte[Bitmap.Width, Bitmap.Height];

            for (int i = 0; i < Bitmap.Width; i++)
            {
                for (int j = 0; j < Bitmap.Height; j++)
                {
                    // Take the blue value, as image is monochrome, B, G, and R are identical.
                    matrix[i, j] = Bitmap.GetPixel(i, j).B;
                }
            }

            return matrix;
        }

        /// <summary>
        /// Converts the given Image object to a width-first double array.
        /// Each number consists of grayscale 0-1 value for the image.
        /// 0 = Black, 1 = White, 0.5 = Gray
        /// </summary>
        /// <returns>Double array consisting of values 0-255 for each element.</returns>
        public double[] ToDoubleArray()
        {
            var vector = new double[Bitmap.Width * Bitmap.Height];

            for (int i = 0; i < Bitmap.Width; i++)
            {
                for (int j = 0; j < Bitmap.Height; j++)
                {
                    // Take the blue value, as image is monochrome, B, G, and R are identical.
                    vector[i * Bitmap.Width + j] = (double)Bitmap.GetPixel(i, j).B / 255;
                }
            }

            return vector;
        }
    }
}
