namespace NeuralNet
{
    public class ComplexNeuralNet
    {
        int avgPointer = 0;
        int avgStartPointer = 0;
        List<float> avgList = new List<float>();

        ConvolutionalNet frontNet;

        public ComplexNeuralNet()
        {
            frontNet = new ConvolutionalNet();
            for(int i=0;i<100;i++)
                avgList.Add(0);
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

        public float CalculateChanges(List<float> realValues)
        {
            float difference = frontNet.CalculateCost(realValues);
            frontNet.CalculateChanges();
            avgPointer = (avgPointer+1)%100;
            avgStartPointer = avgStartPointer==100 ? 100 : avgStartPointer+1;
            avgList[avgPointer] = difference;
            float sum = 0;
            for(int i=0;i<avgStartPointer;i++)
                sum += avgList[i];
            return sum/avgStartPointer;
        }

        public void Improve()
        {
            frontNet.Improve();
        }

        public List<float> getOutput()
        {
            return frontNet.GetOutput();
        }
    }
}