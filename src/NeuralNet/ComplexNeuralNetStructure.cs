namespace NeuralNet
{
    public class ComplexNeuralNet
    {
        NeuronalNet backNet;
        ConvolutionalNet frontNet;

        public ComplexNeuralNet()
        {
            frontNet = new ConvolutionalNet(5, new int[] {3,3,3,6,10}, new int[] {3,3,3,3,3}, new int[] {2,2,2,2,2});
            backNet = new NeuronalNet(2, new int[] {10,5});
        }

        public void Update(byte[] inputR, byte[] inputG, byte[] inputB)
        {
            frontNet.SetInput(inputR, inputG, inputB);
            frontNet.Update();

            backNet.SetInput(frontNet.getOutput());
            backNet.Update();
        }

        public float CalculateChanges(List<float> realValues)
        {
            float difference = backNet.CalculateChanges(realValues);
            frontNet.CalculateChanges(backNet.GetDValues());
            return difference;
        }

        public void Improve()
        {
            frontNet.Improve();
            backNet.Improve();
        }

        public List<float> getOutput()
        {
            return backNet.GetOutput();
        }
    }
}