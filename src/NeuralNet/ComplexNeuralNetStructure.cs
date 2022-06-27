namespace NeuralNet
{
    public class ComplexNeuralNet
    {
        NeuronalNet backNet;
        ConvolutionalNet frontNet;

        public ComplexNeuralNet()
        {
            frontNet = new ConvolutionalNet(5, new int[] {1,10,20,50,100}, new int[] {3,3,3,3,1}, new int[] {2,2,2,2,1});
            backNet = new NeuronalNet(3, new int[] {50,20,10});
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