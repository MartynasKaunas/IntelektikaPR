using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace IntelektikaPR
{
    class Program
    {
        static string trainpath0 = Path.Combine(Environment.CurrentDirectory, @"Data\Train\digit_0");
        static string trainpath1 = Path.Combine(Environment.CurrentDirectory, @"Data\Train\digit_1");
        static string trainpath2 = Path.Combine(Environment.CurrentDirectory, @"Data\Train\digit_2");
        static string trainpath3 = Path.Combine(Environment.CurrentDirectory, @"Data\Train\digit_3");
        static string trainpath4 = Path.Combine(Environment.CurrentDirectory, @"Data\Train\digit_4");
        static string trainpath5 = Path.Combine(Environment.CurrentDirectory, @"Data\Train\digit_5");
        static string trainpath6 = Path.Combine(Environment.CurrentDirectory, @"Data\Train\digit_6");
        static string trainpath7 = Path.Combine(Environment.CurrentDirectory, @"Data\Train\digit_7");
        static string trainpath8 = Path.Combine(Environment.CurrentDirectory, @"Data\Train\digit_8");
        static string trainpath9 = Path.Combine(Environment.CurrentDirectory, @"Data\Train\digit_9");

        static string testpath0 = Path.Combine(Environment.CurrentDirectory, @"Data\Test\digit_0");
        static string testpath1 = Path.Combine(Environment.CurrentDirectory, @"Data\Test\digit_1");
        static string testpath2 = Path.Combine(Environment.CurrentDirectory, @"Data\Test\digit_2");
        static string testpath3 = Path.Combine(Environment.CurrentDirectory, @"Data\Test\digit_3");
        static string testpath4 = Path.Combine(Environment.CurrentDirectory, @"Data\Test\digit_4");
        static string testpath5 = Path.Combine(Environment.CurrentDirectory, @"Data\Test\digit_5");
        static string testpath6 = Path.Combine(Environment.CurrentDirectory, @"Data\Test\digit_6");
        static string testpath7 = Path.Combine(Environment.CurrentDirectory, @"Data\Test\digit_7");
        static string testpath8 = Path.Combine(Environment.CurrentDirectory, @"Data\Test\digit_8");
        static string testpath9 = Path.Combine(Environment.CurrentDirectory, @"Data\Test\digit_9");

        static double[,] Digit0;
        static double[,] Digit1;
        static double[,] Digit2;
        static double[,] Digit3;
        static double[,] Digit4;
        static double[,] Digit5;
        static double[,] Digit6;
        static double[,] Digit7;
        static double[,] Digit8;
        static double[,] Digit9;

        static Color colorBlack = Color.FromArgb(255, 0, 0, 0);
        static Color colorWhite = Color.FromArgb(255, 255, 255, 255);
        static double BW_THRESHOLD = 0.5;
        static List<Vertex> vertices = new List<Vertex>();

        public class Vertex
        {
            public Vertex(int i, int j)
            {
                this.X = i;
                this.Y = j;
            }
            public int X { get; set; }
            public int Y { get; set; }
            public string ToString()
            {
                return string.Format("({0}/{1})", this.X, this.Y);
            }
        }

        static void Main(string[] args)
        {
            // TEST
            string filename = @"test.png";
            string path = Path.Combine(Environment.CurrentDirectory, @"Data\", filename);

            Bitmap originalImage = new Bitmap(path);
            Bitmap convertedImage = ImageToBlackWhite(originalImage, BW_THRESHOLD);  // palieka tik juodus ir baltus pixelius
            convertedImage.Save(@"Data\BWtest.png");

            convertedImage.RotateFlip(RotateFlipType.Rotate270FlipY);

            double[,] imgMatrix = ImageToMatrix(convertedImage);   //paverčia paveikliuką į matricą, kur pixeliai balta - 1 juoda - 0       
            imgMatrix = CutMatrix(imgMatrix);

            Console.WriteLine("RANDOM SIMBOLIO PAVERTIMO I MATRICA IR APKARPYMO BANDYMAS");
            PrintMatrix(imgMatrix);
            //TEST

            //APMOKYMAS
            filename = @"4558.png";
            Digit0 = TrainSymbolMatrix(Digit0, trainpath0, filename);

            filename = @"4732.png";
            Digit1 = TrainSymbolMatrix(Digit1, trainpath1, filename);

            filename = @"4885.png";
            Digit2 = TrainSymbolMatrix(Digit2, trainpath2, filename);

            filename = @"5109.png";
            Digit3 = TrainSymbolMatrix(Digit3, trainpath3, filename);

            filename = @"5265.png";
            Digit4 = TrainSymbolMatrix(Digit4, trainpath4, filename);

            filename = @"5411.png";
            Digit5 = TrainSymbolMatrix(Digit5, trainpath5, filename);

            filename = @"5549.png";
            Digit6 = TrainSymbolMatrix(Digit6, trainpath6, filename);

            filename = @"5691.png";
            Digit7 = TrainSymbolMatrix(Digit7, trainpath7, filename);

            filename = @"5820.png";
            Digit8 = TrainSymbolMatrix(Digit8, trainpath8, filename);

            filename = @"7259.png";
            Digit9 = TrainSymbolMatrix(Digit9, trainpath9, filename);

            //TESTAVIMAS
            int teisingai = 0;
            foreach (string fpath in Directory.GetFiles(testpath0, "*.png"))
            {
                Bitmap og = new Bitmap(fpath);
                Bitmap conv = ImageToBlackWhite(og, BW_THRESHOLD);
                conv.RotateFlip(RotateFlipType.Rotate270FlipY);
                double[,] img = ImageToMatrix(conv);
                img = CutMatrix(img);

                string rez = Testing(img, Digit0, Digit1, Digit2, Digit3, Digit4, Digit5, Digit6, Digit7, Digit8, Digit9);
                if (rez == "0") teisingai++;        
            }

            Console.WriteLine("Digit 0 atpažinimas:");
            Console.WriteLine("tikslumas = " + teisingai + " / 300");

            //TESTAVIMAS2
            teisingai = 0;
            foreach (string fpath in Directory.GetFiles(testpath5, "*.png"))
            {
                Bitmap og = new Bitmap(fpath);
                Bitmap conv = ImageToBlackWhite(og, BW_THRESHOLD);
                conv.RotateFlip(RotateFlipType.Rotate270FlipY);
                double[,] img = ImageToMatrix(conv);
                img = CutMatrix(img);

                string rez = Testing(img, Digit0, Digit1, Digit2, Digit3, Digit4, Digit5, Digit6, Digit7, Digit8, Digit9);
                if (rez == "5") teisingai++;
            }

            Console.WriteLine("Digit 5 atpažinimas:");
            Console.WriteLine("tikslumas = " + teisingai + " / 300");

            //TESTAVIMAS3
            teisingai = 0;
            foreach (string fpath in Directory.GetFiles(testpath9, "*.png"))
            {
                Bitmap og = new Bitmap(fpath);
                Bitmap conv = ImageToBlackWhite(og, BW_THRESHOLD);
                conv.RotateFlip(RotateFlipType.Rotate270FlipY);
                double[,] img = ImageToMatrix(conv);
                img = CutMatrix(img);

                string rez = Testing(img, Digit0, Digit1, Digit2, Digit3, Digit4, Digit5, Digit6, Digit7, Digit8, Digit9);
                if (rez == "9") teisingai++;
            }

            Console.WriteLine("Digit 9 atpažinimas:");
            Console.WriteLine("tikslumas = " + teisingai + " / 300");
            Console.ReadLine();

        }

        public static double[,] TrainSymbolMatrix(double[,] symbolmatrix, string trainpath, string filename)
        {
            string path = Path.Combine(trainpath, filename);
            Bitmap originalImage = new Bitmap(path);
            Bitmap convertedImage = ImageToBlackWhite(originalImage, BW_THRESHOLD);
            convertedImage.RotateFlip(RotateFlipType.Rotate270FlipY);
            double[,] imgMatrix = ImageToMatrix(convertedImage);
            imgMatrix = CutMatrix(imgMatrix);
            symbolmatrix = imgMatrix;

            Console.WriteLine();
            PrintMatrix(symbolmatrix);

            foreach (string fpath in Directory.GetFiles(trainpath, "*.png"))
            {
                Bitmap og = new Bitmap(fpath);
                Bitmap conv = ImageToBlackWhite(og, BW_THRESHOLD);
                conv.RotateFlip(RotateFlipType.Rotate270FlipY);
                double[,] img = ImageToMatrix(conv);
                img = CutMatrix(img);

                symbolmatrix = Training(img, symbolmatrix);
            }

            return symbolmatrix;
        }

        public static double[,] processImage(string fpath)
        {
            Bitmap og = new Bitmap(fpath);
            Bitmap conv = ImageToBlackWhite(og, BW_THRESHOLD);
            conv.RotateFlip(RotateFlipType.Rotate270FlipY);
            double[,] img = ImageToMatrix(conv);
            img = CutMatrix(img);
            return img;
        }

        //Paverčia visus pilkus pixelius juodais arba baltais pagal slenksčio reikšmę
        public static Bitmap ImageToBlackWhite(Bitmap imgSrc, double threshold){
            int width = imgSrc.Width;
            int height = imgSrc.Height;
            Color pixel;
            Bitmap imgOut = new Bitmap(imgSrc);
            for (int row = 0; row < height - 1; row++)
            {
                for (int col = 0; col < width - 1; col++)
                {
                    pixel = imgSrc.GetPixel(col, row);
                    if (pixel.GetBrightness() < threshold)
                    {
                        vertices.Add(new Vertex(col, row));
                        imgOut.SetPixel(col, row, colorBlack);
                    }
                    else
                    {
                        imgOut.SetPixel(col, row, colorWhite);
                    }
                }
            }
            return imgOut;
        }

        //Konvertuoja juodą/baltą paveiksliuką į 0/1 matricą
        public static double[,] ImageToMatrix(Bitmap img){
            int height = img.Height;
            int width = img.Width;

            double[,] Matrix = new double[32, 32];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (img.GetPixel(i, j).Equals(colorBlack))
                    {
                        Matrix[i, j] = 0;
                    }
                    else if (img.GetPixel(i, j).Equals(colorWhite))
                    {
                        Matrix[i, j] = 1;
                    }
                }
            }
            return Matrix;
        }

        //Nukerpa tuščius 2px nuo kiekvieno krašto
        public static double[,] CutMatrix(double[,] matrix){
            double[,] newmatrix = new double[28, 28];

            for (int i = 2; i < matrix.GetLength(0) - 2; i++)
            {
                for (int j = 2; j < matrix.GetLength(1) - 2; j++)
                {
                    newmatrix[i - 2, j - 2] = matrix[i, j];
                }              
            }

            return newmatrix;
        }

        //spausdina 0/1 matricą
        public static void PrintMatrix(double[,] matrix){
            for (int i = 0; i < matrix.GetLength(0); i++){
                for (int j = 0; j < matrix.GetLength(1); j++){
                    Console.Write(string.Format("{0}", matrix[i, j]));
                }
                Console.Write(Environment.NewLine + Environment.NewLine);
            }
        }

        //Apmokymas. Gaunam svorinę matricą. "Kaip atrodo simbolis".Kuo daugiau kartų tame pikselyje buvo balta - tuo didesnis skaičius
        public static double[,] Training(double[,] trainmatrix, double[,] finalMatrix)
        {
            double[,] newFinalmatrix = new double[28, 28];

            for (int i = 0; i < trainmatrix.GetLength(0); i++)
            {
                for (int j = 0; j < trainmatrix.GetLength(1); j++)
                {
                    newFinalmatrix[i, j] = Math.Round((trainmatrix[i, j] + finalMatrix[i, j]));
                }
            }
            return newFinalmatrix;       
        }

        //Testavimas, tikrinama, kurią apmokymo metu gautą svorių matricą geriausiai atitinka testuojamas paveiksliukas
        public static string Testing(double[,] testmatrix, double[,] SymbolMatrix0, double[,] SymbolMatrix1, double[,] SymbolMatrix2, double[,] SymbolMatrix3, double[,] SymbolMatrix4,
            double[,] SymbolMatrix5, double[,] SymbolMatrix6, double[,] SymbolMatrix7, double[,] SymbolMatrix8, double[,] SymbolMatrix9)
        {
            int[] scores = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            string[] results = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

            for (int i = 0; i < testmatrix.GetLength(0); i++)
            {
                for (int j = 0; j < testmatrix.GetLength(1); j++)
                {
                    if ((SymbolMatrix0[i, j] >= 500) && (testmatrix[i, j] == 1)){     //>=800 tai random parinkta svorio slenksčio reikšmė Paėmiau (failų skaičių apmokymo folderį)/2
                        scores[0] += 1;                      
                    }
                    if ((SymbolMatrix1[i, j] >= 500) && (testmatrix[i, j] == 1)){
                        scores[1] += 1;
                    }
                    if ((SymbolMatrix2[i, j] >= 500) && (testmatrix[i, j] == 1))
                    {
                        scores[2] += 1;
                    }
                    if ((SymbolMatrix3[i, j] >= 500) && (testmatrix[i, j] == 1))
                    {
                        scores[3] += 1;
                    }
                    if ((SymbolMatrix4[i, j] >= 500) && (testmatrix[i, j] == 1))
                    {
                        scores[4] += 1;
                    }
                    if ((SymbolMatrix5[i, j] >= 500) && (testmatrix[i, j] == 1))
                    {
                        scores[5] += 1;
                    }
                    if ((SymbolMatrix6[i, j] >= 500) && (testmatrix[i, j] == 1))
                    {
                        scores[6] += 1;
                    }
                    if ((SymbolMatrix7[i, j] >= 500) && (testmatrix[i, j] == 1))
                    {
                        scores[7] += 1;
                    }
                    if ((SymbolMatrix8[i, j] >= 500) && (testmatrix[i, j] == 1))
                    {
                        scores[8] += 1;
                    }
                    if ((SymbolMatrix9[i, j] >= 500) && (testmatrix[i, j] == 1))
                    {
                        scores[9] += 1;
                    }
                }
            }
            int maxValue = scores.Max();
            int maxIndex = scores.ToList().IndexOf(maxValue);

            return results[maxIndex];
        }
    }
}
