using NeuralNet;
using NeuronalNetServer.Helpers;
using NeuronalNetServer.Proto;

namespace NeuronalNetServer.Services
{
    public class NeuralCalculator
    {
        #region Fields

        private ComplexNeuralNet _net;
        private readonly DatabaseService _dbService;

        #endregion

        #region Constructor

        public NeuralCalculator()
        {
            _net = new ComplexNeuralNet();
        }

        #endregion

        #region Methods

        public TrafficSign LoadImage(int number)
        {
            int maxIndex = 12;
            int randomIndex = new Random().Next()%maxIndex;

            TrafficSign sign = new TrafficSign();

            //Load image

            return sign;
        }

        public void Calculate(int number, List<float> dataRed, List<float> dataGreen, List<float> dataBlue)
        {
            TrafficSign loadedSign = LoadImage(number);

            //Convert sign to image data

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
