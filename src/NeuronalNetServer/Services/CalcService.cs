using NeuralNet;

namespace NeuronalNetServer.Services
{
    public class Calculator
    {
        ComplexNeuralNet net = new ComplexNeuralNet();

        Dictionary<int[],List<float>> trainngData = new Dictionary<int[], List<float>>();

        public void Calculate(int number,int index)
        {
            List<float> trainingValues = new List<float>();
            for(int i=0;i<=10;i++)
            {
                trainingValues.Add(i==number ? 1 : 0);
            }
            net.Update(trainngData[new int[] {number,index}]);
            net.CalculateChanges(trainingValues);
            net.Improve();
        }
    }
}
