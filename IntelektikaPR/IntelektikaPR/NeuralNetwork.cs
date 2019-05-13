using System;
using Accord.Math;
using Accord.Neuro;
using Accord.Neuro.Learning;

namespace IntelektikaPR
{
    class NeuralNetwork
    {
        private readonly double[][] _inputData;
        private readonly double[][] _outputData;

        private const int EpochsMax = 2000;
        private const int OverfittedErrorTreshold = 5;

        private ActivationNetwork Network { get; }

        public NeuralNetwork(double[][] inputData, double[][] outputData)
        {
            _inputData = inputData;
            _outputData = outputData;
            Network = new ActivationNetwork(new SigmoidFunction(),
                inputData[0].Length,    // Input neuron count
                outputData[0].Length);  // Output neuron count
            Network.Randomize();
        }

        public void Teach()
        {
            // Teacher method.
            // Perceptron was experimentally found to give best results.
            var teacher = new PerceptronLearning(Network);

            // Value to track network's effectivness.
            // If value increases over time, network is overfitted - terminate learning process.
            var previousError = double.MaxValue;

            // Value tracks the amount of times error value increased.
            // Once it reaches a treshold, stop trainig - network is overfitted.
            var errorIncreaseCount = 0;

            // Epochs tracks the amount of cycles neural network took.
            // Used to prevent infinite training.
            var epochs = 0;

            Console.WriteLine("Teaching network.");
            // Train the neural network.
            // Training lasts until EITHER network starts to get overfitted with data,
            // or trainig has reached maximum epoch count.
            while (errorIncreaseCount < OverfittedErrorTreshold && epochs < EpochsMax)
            {
                var error = teacher.RunEpoch(_inputData, _outputData);
                if (error > previousError)
                    errorIncreaseCount++;
                else
                    errorIncreaseCount = 0;
                previousError = error;

                epochs += 1;
                if (epochs % 20 == 0)
                    Console.WriteLine($"{(double)epochs / EpochsMax * 100}%");
            }
        }

        public int Compute(Image img)
        {
            return Compute(img.ToDoubleArray());
        }

        private int Compute(byte[,] img)
        {
            var inputVector = new double[img.Length];
            var index = 0;
            foreach (var pixel in img)
            {
                inputVector[index++] = (double)pixel / 255;
            }

            return Compute(inputVector);
        }

        private int Compute(double[] inputVector)
        {
            var output = Network.Compute(inputVector);
            output.Max(out var index);
            return index;
        }
    }
}
