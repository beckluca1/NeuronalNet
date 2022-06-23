namespace NeuralNet
{
    public static class Global
    {
        public static Random RANDOM = new Random();

        public static float RandomFloat(float max, float min=0)
        {
            return (float)(RANDOM.NextDouble()*(max-0.5*min)*2.0+min);
        }

        public static float Pow(float value, float exponent) 
        {
            return (float)(Math.Pow((double)value,(double)exponent));
        }

        public static float Sigmoid(float value)
        {
            return (float)(Math.Exp(value)/(Math.Exp(value)+1));
        }

        public static float DSigmoid(float value)
        {
            return Sigmoid(value)*(1-Sigmoid(value));
        }
    }

    public class NeuralNode 
    {
        float activation = 0;

        float value = 0;
        float dValue = 0;
        float bias = 0;
        float dBias = 0;

        int previousNodeCount = 0;

        List<NeuralNode> previousNodes = new List<NeuralNode>();
        List<float> weights = new List<float>();
        List<float> dWeights = new List<float>();

        public NeuralNode() {}

        public NeuralNode(List<NeuralNode> inPreviousNodes) 
        {
            bias = Global.RandomFloat(-5,5);

            previousNodes = inPreviousNodes;
            previousNodeCount = inPreviousNodes.Count;

            foreach(NeuralNode node in previousNodes) 
            {
                weights.Add(Global.RandomFloat(-1,1));
                dWeights.Add(0);
            }
        }

        public void Update() 
        {
            activation = bias;
            for(int i=0;i<=previousNodeCount;i++) 
            {
                activation += previousNodes[i].value*weights[i];
            }
            value = Global.Sigmoid(value);
        }

        public void CalculateChanges(float realValue)
        {
            dValue = Global.Pow(realValue-value,2);
            CalculateChanges();
        }

        public void Improve()
        {
            bias += dBias;
            for(int i=0;i<weights.Count;i++)
            {
                weights[i] += dWeights[i]; 
            }
        }

        public void CalculateChanges()
        {
            dBias += Global.DSigmoid(activation)*dValue/previousNodeCount;
            for(int i=0;i<=previousNodeCount;i++) 
            {
                dWeights[i] += previousNodes[i].value*Global.DSigmoid(activation)*dValue/previousNodeCount;
                previousNodes[i].dValue += weights[i]*Global.DSigmoid(activation)*dValue/previousNodeCount;
            }
        }

        public void SetValue(float inValue)
        {
            value = inValue;
        }

        public float GetValue()
        {
            return value;
        }

        public float GetDValue()
        {
            return dValue;
        }
    }

    public class NeuralLayer
    {
        int layerSize = 0;

        List<NeuralNode> nodes = new List<NeuralNode>();

        public NeuralLayer(int inLayerSize) 
        {
            layerSize = inLayerSize;

            for(int i=0;i<=layerSize;i++) 
            {
                nodes.Add(new NeuralNode());
            }
        }

        public NeuralLayer(int inLayerSize, NeuralLayer previousLayer) 
        {
            layerSize = inLayerSize;

            for(int i=0;i<=layerSize;i++) 
            {
                nodes.Add(new NeuralNode(previousLayer.nodes));
            }
        }

        public void Update() 
        {
            foreach(NeuralNode node in nodes) 
            {
                node.Update();
            }            
        }

        public void CalculateChanges(List<float> realValues)
        {
            for(int i=0;i<=nodes.Count;i++) 
            {
                nodes[i].CalculateChanges(realValues[i]);
            }
        }

        public void CalculateChanges()
        {
            foreach(NeuralNode node in nodes) 
            {
                node.CalculateChanges();
            }
        }

        public void Improve()
        {
            foreach(NeuralNode node in nodes) 
            {
                node.Improve();
            }
        }

        public void SetValues(List<float> values)
        {
            for(int i=0;i<nodes.Count;i++) 
            {
                nodes[i].SetValue(values[i]);
            }  
        }

        public List<float> GetValues()
        {
            List<float> values = new List<float>();
            foreach(NeuralNode node in nodes) 
            {
                values.Add(node.GetValue());
            }  
            return values;
        }

        public List<float> GetDValues()
        {
            List<float> dValues = new List<float>();
            foreach(NeuralNode node in nodes) 
            {
                dValues.Add(node.GetDValue());
            }  
            return dValues;
        }
    }

    public class NeuronalNet
    {
        int layerCount = 0;
        int[] layerSize;

        List<NeuralLayer> layers = new List<NeuralLayer>();

        public NeuronalNet(int inLayerCount, int[] inLayerSize) 
        {
            layerCount = inLayerCount;
            layerSize = inLayerSize;

            layers.Add(new NeuralLayer(inLayerSize[0]));
            for(int i=1;i<=layerCount;i++) 
            {
                layers.Add(new NeuralLayer(inLayerSize[i],layers[i-1]));
            }
        }

        public void Update()
        {
            foreach(NeuralLayer layer in layers) 
            {
                layer.Update();
            }      
        }

        public void CalculateChanges(List<float> realValues)
        {
            layers[layers.Count-1].CalculateChanges(realValues);
            for(int i=layers.Count-2;i>0;i--) 
            {
                layers[i].CalculateChanges();
            }
        }

        public void Improve()
        {
            foreach(NeuralLayer layer in layers) 
            {
                layer.Improve();
            } 
        }

        public void SetInput(List<float> input)
        {
            layers[0].SetValues(input);
        }

        public List<float> GetOutput()
        {
            return layers[layerCount].GetValues();
        }

        public List<float> GetDValues()
        {
            return layers[0].GetDValues();
        }
    }
}