namespace NeuralNet
{
    public class ComplexNeuralNet
    {
        NeuronalNet backNet;
        ConvolutionalNet frontNet;

        public ComplexNeuralNet()
        {
            backNet = new NeuronalNet(3, new int[] {50,20,10});
            frontNet = new ConvolutionalNet(3, new int[] {10,20,50}, new int[] {3,3,3}, new int[] {2,2,2});
        }

        public void Update(List<float> input)
        {
            frontNet.SetInput(input);
            frontNet.Update();
            backNet.SetInput(frontNet.getOutput());
            backNet.Update();
        }

        public void CalculateChanges(List<float> realValues)
        {
            backNet.CalculateChanges(realValues);
            frontNet.CalculateChanges(backNet.GetDValues());
        }

        public void Improve()
        {
            frontNet.Improve();
            backNet.Improve();
        }
    }
}