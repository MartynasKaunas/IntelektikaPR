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

        static string trainpath = Path.Combine(Environment.CurrentDirectory, @"Data\Train\digit_0");
        static string trainpath2 = Path.Combine(Environment.CurrentDirectory, @"Data\Train\digit_1");
        static string testpath = Path.Combine(Environment.CurrentDirectory, @"Data\Test\digit_0");
        static string testpath2 = Path.Combine(Environment.CurrentDirectory, @"Data\Test\digit_1");


        //Simbolių matricos į kurias dedami apmokymo metu gauti svoriai
        static double[,] Digit0;
        static double[,] Digit1;

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

            //PIRMO SIMBOLIO APMOKYMAS
            filename = @"4558.png";
            path = Path.Combine(trainpath, filename);

            Digit0 = processImage(path);

            Console.WriteLine();
            Console.WriteLine("DIGIT 0 PRADINĖ MATRICA");
            PrintMatrix(Digit0);


            foreach (string fpath in Directory.GetFiles(trainpath, "*.png"))
            {
                double[,] img = processImage(fpath);
                Digit0 = Training(img, Digit0);
            }

            Console.WriteLine();
            Console.WriteLine("DIGIT 0 APMOKYTA MATRICA");
            PrintMatrix(Digit0);

            //ANTRO SIMBOLIO APMOKYMAS
            filename = @"4732.png";
            path = Path.Combine(trainpath2, filename);

            Digit1 = processImage(path);

            Console.WriteLine();
            Console.WriteLine("DIGIT 1 PRADINĖ MATRICA");
            PrintMatrix(Digit1);

            foreach (string fpath in Directory.GetFiles(trainpath2, "*.png"))
            {
                double[,] img = processImage(fpath);
                Digit1 = Training(img, Digit1);
            }

            Console.WriteLine();
            Console.WriteLine("DIGIT 1 APMOKYTA MATRICA");
            PrintMatrix(Digit1);
         
            //TESTAVIMAS
            int teisingai = 0;
            foreach (string fpath in Directory.GetFiles(testpath, "*.png"))
            {
                double[,] img = processImage(fpath);
                string rez = Testing(img, Digit0, Digit1);
                if (rez == "pirmas") teisingai++;        
            }

            Console.WriteLine("tikslumas = " + teisingai + " / 300");
            Console.ReadLine();

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
        public static string Testing(double[,] testmatrix, double[,] SymbolMatrix1, double[,] SymbolMatrix2)
        {
            int[] scores = { 0, 0 };
            string[] results = { "pirmas", "antras" };

            for (int i = 0; i < testmatrix.GetLength(0); i++)
            {
                for (int j = 0; j < testmatrix.GetLength(1); j++)
                {
                    if ((SymbolMatrix1[i, j] >= 850) && (testmatrix[i, j] == 1)) {     //>=800 tai random parinkta svorio slenksčio reikšmė Paėmiau (failų skaičių apmokymo folderį)/2
                        scores[0] += 1;                      
                    }
                    else if ((SymbolMatrix2[i, j] >= 850) && (testmatrix[i, j] == 1)){
                        scores[1] += 1;
                    }
                }
            }
            if (scores[0] > scores[1]) return results[0];
            else return results[1];
        }
    }
}
