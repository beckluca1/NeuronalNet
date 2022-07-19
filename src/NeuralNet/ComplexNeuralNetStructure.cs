namespace NeuralNet
{
    public class ComplexNeuralNet
    {

        public float cost = 0;
        public int correct = 0;

        ConvolutionalNet frontNet;

        public ComplexNeuralNet()
        {
            frontNet = new ConvolutionalNet();
        }

        public void Update(byte[] inputR, byte[] inputG, byte[] inputB)
        {
            frontNet.SetInput(inputR, inputG, inputB);
            frontNet.Update();
        }

        public bool Correct(int number)
        {
            return frontNet.Correct(number);
        }

        public void CalculateChanges(List<float> realValues, int number)
        {
            frontNet.CalculateCost(realValues);
            frontNet.Correct(number);
            frontNet.CalculateChanges();
        }

        public void Improve()
        {
            frontNet.Improve();
            cost = frontNet.cost;
            frontNet.cost = 0;
            correct = frontNet.correct;
            frontNet.correct = 0;
        }

        public List<float> getOutput()
        {
            return frontNet.GetOutput();
        }
    }
}