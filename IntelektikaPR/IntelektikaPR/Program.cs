using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;
using System.Threading;

namespace IntelektikaPR
{
    class Program
    {
        private static readonly string TrainigData = Path.Combine(Environment.CurrentDirectory, @"Data\Data");
        private static readonly string TestData = Path.Combine(Environment.CurrentDirectory, @"Data\Test");

        //metodo 1 simbolių "heatmap" matricos
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

        //į kokį dydį performuojami paveiksliukai
        private const int ConvWidth = 16;
        private const int ConvHeigth = 16;

        static readonly Color ColorBlack = Color.Black;
        static readonly Color ColorWhite = Color.White;
        private const double BwThreshold = 0.5;
        private static readonly List<Vertex> Vertices = new List<Vertex>();

        static void Main(string[] args)
        {
            new Thread(NeuralNetworkTests).Start();

            List<double> result = XValidation(5, TrainigData);
            Console.WriteLine("Kryžminės patikros rezultatai:");
            int i = 0;
            foreach (double value in result)
            {
                Console.WriteLine((i + 1) + " kryžminės patikros bendras tikslumas: " + $"{value:F1}" + " %");
                i++;
            }

            Console.ReadLine();
        }

        /// <summary>
        /// Performs the Neural Netowrk tests.
        /// </summary>
        public static void NeuralNetworkTests()
        {
            // Read the trainig data.
            Console.WriteLine("Reading input data.");
            var inputData = ReadAllFolders(TrainigData)
                .OrderBy(o => Guid.NewGuid()) // Scramble the list
                .ToArray(); // Execute the request

            // Prepare data for the neural network.
            var outputVector = inputData.Select(d => d.Key)
                .Select(iValue => // Create a key array, with index of the key having value 1, rest = 0.
                {
                    var a = new double[10];
                    a[iValue] = 1;
                    return a;
                }).ToArray();
            var inputVector = inputData.Select(d => d.Value.ToDoubleArray())
                .ToArray(); // Create a value array, with having appropriate indexes.

            // Create a neural network
            var network = new NeuralNetwork(inputVector, outputVector);

            // Execute the learning process
            network.Teach();

            // Read the test data.
            Console.WriteLine("Reading output data.");
            var outputData = ReadAllFolders(TestData);

            // Print out the result.
            var correctCount = outputData.GroupBy(k => k.Key).Select(g => new
            {
                Index = g.Key,
                TotalElements = g.Count(),
                SuccessCount = g.Count(testData => testData.Key == network.Compute(testData.Value))
            }).ToArray();
            foreach (var el in correctCount)
            {
                Console.WriteLine($"Neural network accuracy for number {el.Index}: {(double)el.SuccessCount / el.TotalElements:P}%");
            }

            var overallSuccess = correctCount.Sum(e => e.SuccessCount);
            Console.WriteLine($"Overall neural network accuracy: [{overallSuccess} / {outputData.Length}] - {(double)overallSuccess / outputData.Length:P}");
        }

        /// <summary>
        /// Reads all of the contents of a given folder and creates an array of Key - Image pairings
        /// </summary>
        /// <returns>An array of Key-Image pairings. Key is int - image's true number</returns>
        public static KeyValuePair<int, Image>[] ReadAllFolders(string folderPath)
        {
            var index = 0;
            var inputData = Enumerable.Empty<KeyValuePair<int, Image>>();
            foreach (var directory in Directory.EnumerateDirectories(folderPath))
            {
                inputData = inputData.Concat(ReadFolder(directory) // Read the contents and concatinate the image data.
                        .Select(f => new KeyValuePair<int, Image>(index, f))) // Attach an index to a given image, i.e. 2 = file in directory no.2
                    .ToArray(); // Execute the request immedietly.
                index++;
                Console.WriteLine($"{index * 10}%");
            }

            return (KeyValuePair<int, Image>[])inputData;
        }

        /// <summary>
        /// Reads folder data for images
        /// </summary>
        /// <param name="folderPath">Path to search for images</param>
        /// <returns>List of Images in the given folder.</returns>
        public static IEnumerable<Image> ReadFolder(string folderPath)
        {
            var rect = new Rectangle(2, 2, 28, 28);
#if DEBUG
            var index = 0;
#endif
            foreach (var filePath in Directory.EnumerateFiles(folderPath))
            {
#if DEBUG
                if (index++ >= 200) continue;
#endif
                var bitmap = new Bitmap(filePath);
                yield return new Image(bitmap.Clone(rect, bitmap.PixelFormat));
            }
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        //METODAS SU MOKYTOJU 1 BEGIN
        //------------------------------------------------------------------------------------------------------------------------------------------------

        // Vykdoma kryžminė patikra
        public static List<double> XValidation(int dataSetCount, string data)
        {
            List<double> result = new List<double>();
            int start = 0; // testuojamų duomenų pradžios indeksas
            int step = (int)(2000.0 / dataSetCount); // testuojamų duomenų indekso žingsnis tarp iteracijų
            int end = (int)(2000.0 / dataSetCount); // testuojamų duomenų pabaigos indeksas
            List<List<string>> listList = new List<List<string>>();
            // Surašomi failų pavadinimai į sąrašą. Sąrašas saugo kiekvieno skaitmens paveikslėlių sąrašą (kelius (path) iki paveikslėlių).
            for (int i = 0; i < 10; i++)
            {
                List<string> list = new List<string>();
                foreach (string fpath in Directory.GetFiles(Path.Combine(data, "digit_" + i), "*.png"))
                {
                    list.Add(fpath);
                }
                listList.Add(list);
            }

            //Pradedamas apmokymas
            for (int i = 0; i < dataSetCount; i++)
            {
                Console.WriteLine("Vykdoma " + (i + 1) + " kryžminės patikros iteracija.");
                List<List<string>> temp = new List<List<string>>();
                for (int j = 0; j < 10; j++)
                {
                    List<string> t = listList[j].ToList();
                    temp.Add(t);
                }
                if (i < dataSetCount - 1)
                {
                    //Pašalinami testuojami duomenys iš apmokymo sąrašo, kai i < kryž. patikros iteracijų sk. - 1
                    foreach (List<string> list in temp)
                    {
                        list.RemoveRange(start, step);
                    }
                }
                else
                {
                    //Pašalinami testuojami duomenys iš apmokymo sąrašo, kai i == kryž. patikros iteracijų sk. - 1
                    foreach (List<string> list in temp)
                    {
                        list.RemoveRange(start, list.Count - start);
                    }
                }
                // Pradedamas apmokymas
                Digit0 = Digit1 = Digit2 = Digit3 = Digit4 = Digit5 = Digit6 = Digit7 = Digit8 = Digit9 = null;
                Digit0 = TrainSymbolMatrix(Digit0, temp[0]);
                Digit1 = TrainSymbolMatrix(Digit1, temp[1]);
                Digit2 = TrainSymbolMatrix(Digit2, temp[2]);
                Digit3 = TrainSymbolMatrix(Digit3, temp[3]);
                Digit4 = TrainSymbolMatrix(Digit4, temp[4]);
                Digit5 = TrainSymbolMatrix(Digit5, temp[5]);
                Digit6 = TrainSymbolMatrix(Digit6, temp[6]);
                Digit7 = TrainSymbolMatrix(Digit7, temp[7]);
                Digit8 = TrainSymbolMatrix(Digit8, temp[8]);
                Digit9 = TrainSymbolMatrix(Digit9, temp[9]);

                int corr = 0; // teisingų spėjimų kiekis
                int count = 0; // bendras spėjimų kiekis

                double[] foldresults = new double[10];

                // Vykdoma kryžminė patikra. Ciklas keliauja per sąrašo skaičius (0-9)
                for (int j = 0; j < 10; j++)
                {
                    corr = 0; // teisingų spėjimų kiekis
                    count = 0; // bendras spėjimų kiekis
                               // Ciklas keliauja per j - tojo skaitmens paveikslėlių sąrašą
                    int step2 = step;
                    if (i >= dataSetCount - 1) step2 = listList[j].ToList().Count - start;
                    foreach (string fpath in listList[j].GetRange(start, step2))
                    {
                        count++;

                        double[,] img = processImage(fpath);

                        //Atlieka skaitmens testavimą
                        string rez = Testing(img, Digit0, Digit1, Digit2, Digit3, Digit4, Digit5, Digit6, Digit7, Digit8, Digit9);
                        if (rez == "" + j) corr++;
                    }
                    Console.WriteLine("Skaitmens " + j + " atpažinimo tikslumas: " + $"{(double)corr / count * 100:F1} %");
                    foldresults[j] = (double)corr / count * 100;
                    Console.WriteLine(corr + " " + count);
                }
                double sum = 0;
                for (int z = 0; z < foldresults.Length; z++)
                {
                    sum += foldresults[z];
                }
                double acc = sum / foldresults.Length;
                result.Add(acc);
                Console.WriteLine("Tikslumas: " + string.Format("{0:F1} %", acc));
                Console.WriteLine("------------------------------------------------------------------------");
                Console.WriteLine();
                // Paslenkama testuojamų duomenų pradžios bei pabaigos indeksas
                start = start + step;
                end = end + step;
            }
            return result;
        }


        //
        public static double[,] TrainSymbolMatrix(double[,] symbolmatrix, List<string> trainpath)
        {
            symbolmatrix = processImage(trainpath[0]);

            foreach (string fpath in trainpath)
            {
                double[,] img = processImage(fpath);
                symbolmatrix = Training(img, symbolmatrix);
            }

            return symbolmatrix;
        }

        //Apmokymas. Gaunam svorinę matricą. "Kaip atrodo simbolis".Kuo daugiau kartų tame pikselyje buvo balta - tuo didesnis skaičius
        public static double[,] Training(double[,] trainmatrix, double[,] finalMatrix)
        {
            double[,] newFinalmatrix = new double[ConvWidth, ConvHeigth];

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
            int riba = 300; //kiek baltos spalvos pasikartojimų heatmap'e skaitosi kaip "atitikimas" 

            for (int i = 0; i < testmatrix.GetLength(0); i++)
            {
                for (int j = 0; j < testmatrix.GetLength(1); j++)
                {
                    if ((SymbolMatrix0[i, j] >= riba) && (testmatrix[i, j] == 1))
                    {
                        scores[0] += 1;
                    }
                    if ((SymbolMatrix1[i, j] >= riba) && (testmatrix[i, j] == 1))
                    {
                        scores[1] += 1;
                    }
                    if ((SymbolMatrix2[i, j] >= riba) && (testmatrix[i, j] == 1))
                    {
                        scores[2] += 1;
                    }
                    if ((SymbolMatrix3[i, j] >= riba) && (testmatrix[i, j] == 1))
                    {
                        scores[3] += 1;
                    }
                    if ((SymbolMatrix4[i, j] >= riba) && (testmatrix[i, j] == 1))
                    {
                        scores[4] += 1;
                    }
                    if ((SymbolMatrix5[i, j] >= riba) && (testmatrix[i, j] == 1))
                    {
                        scores[5] += 1;
                    }
                    if ((SymbolMatrix6[i, j] >= riba) && (testmatrix[i, j] == 1))
                    {
                        scores[6] += 1;
                    }
                    if ((SymbolMatrix7[i, j] >= riba) && (testmatrix[i, j] == 1))
                    {
                        scores[7] += 1;
                    }
                    if ((SymbolMatrix8[i, j] >= riba) && (testmatrix[i, j] == 1))
                    {
                        scores[8] += 1;
                    }
                    if ((SymbolMatrix9[i, j] >= riba) && (testmatrix[i, j] == 1))
                    {
                        scores[9] += 1;
                    }
                }
            }
            int maxValue = scores.Max();
            int maxIndex = scores.ToList().IndexOf(maxValue);

            return results[maxIndex];
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        //METODAS SU MOKYTOJU 1 END
        //------------------------------------------------------------------------------------------------------------------------------------------------

        //------------------------------------------------------------------------------------------------------------------------------------------------
        //DUOMENŲ TVARKYMAS BEGIN
        //------------------------------------------------------------------------------------------------------------------------------------------------


        // Paima paveikslėlį iš failo, sumažina, pilkus pixelius padaro juodais/baltais, konvertuoaj į 0/1 matricą, yra galimybė apkarpyti
        public static double[,] processImage(string fpath)
        {
            Bitmap og = new Bitmap(fpath);                                 //Paima iš failo
            Bitmap conv = new Bitmap(ConvWidth, ConvHeigth);               //Sumažina
            using (Graphics gr = Graphics.FromImage(conv))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gr.DrawImage(og, new Rectangle(0, 0, ConvWidth, ConvHeigth));
            }
            conv = ImageToBlackWhite(conv, BwThreshold);                   //Pilkus pixelius -> juodus/baltus
            conv.RotateFlip(RotateFlipType.Rotate270FlipY);
            double[,] img = ImageToMatrix(conv);                            //konvertuoja į 0/1 matricą
                                                                            //img = CutMatrix(img);                                         //apkarpo -2px nuo kiekvieno krašto
            return img;
        }

        //Paverčia visus pilkus pixelius juodais arba baltais pagal slenksčio reikšmę
        public static Bitmap ImageToBlackWhite(Bitmap imgSrc, double threshold)
        {
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
                        Vertices.Add(new Vertex(col, row));
                        imgOut.SetPixel(col, row, ColorBlack);
                    }
                    else
                    {
                        imgOut.SetPixel(col, row, ColorWhite);
                    }
                }
            }
            return imgOut;
        }

        //Konvertuoja juodą/baltą paveiksliuką į 0/1 matricą
        public static double[,] ImageToMatrix(Bitmap img)
        {
            int height = img.Height;
            int width = img.Width;

            double[,] Matrix = new double[ConvWidth, ConvHeigth];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (img.GetPixel(i, j).Equals(ColorBlack))
                    {
                        Matrix[i, j] = 0;
                    }
                    else if (img.GetPixel(i, j).Equals(ColorWhite))
                    {
                        Matrix[i, j] = 1;
                    }
                }
            }
            return Matrix;
        }

        //Nukerpa tuščius 2px nuo kiekvieno krašto
        public static double[,] CutMatrix(double[,] matrix)
        {
            double[,] newmatrix = new double[ConvWidth - 2, ConvHeigth - 2];

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
        public static void PrintMatrix(double[,] matrix)
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    Console.Write($"{matrix[i, j]}");
                }
                Console.Write(Environment.NewLine + Environment.NewLine);
            }
        }

        public class Vertex
        {
            public Vertex(int i, int j)
            {
                X = i;
                Y = j;
            }
            public int X { get; set; }
            public int Y { get; set; }

            public override string ToString()
            {
                return $"({X}/{Y})";
            }
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        //DUOMENŲ TVARKYMAS END
        //------------------------------------------------------------------------------------------------------------------------------------------------

    }
}
