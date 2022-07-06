using NeuralNet;
using NeuronalNetServer.Helpers;

namespace NeuronalNetServer.Services
{
    public class NeuralCalculator
    {
        #region Fields

        private ComplexNeuralNet _net;

        #endregion

        #region Constructor

        public NeuralCalculator()
        {
            _net = new ComplexNeuralNet();
        }

        #endregion

        #region Methods

        public void Calculate(int number, List<float> dataRed, List<float> dataGreen, List<float> dataBlue)
        {
            List<float> trainingValues = new List<float>();
            for (int i = 0; i <= 10; i++)
            {
                trainingValues.Add(i == number ? 1 : 0);
            }
            _net.Update(dataRed,dataGreen,dataBlue);
            _net.CalculateChanges(trainingValues);
            _net.Improve();
        }

        #endregion
    }
}
