namespace NeuralNet
{
    public static class Global
    {
        public static float BATCH_SIZE = 10;
        public static Random RANDOM = new Random();

        public static float RandomFloat(float max, float min=0)
        {
            return (float)(RANDOM.NextDouble()*(max-min)+min);
        }

        public static float Pow(float value, float exponent) 
        {
            return (float)(Math.Pow((double)value,(double)exponent));
        }

        public static float Sigmoid(float value)
        {
            return (float)(1/(Math.Exp(-value)+1));
        }

        public static float DSigmoid(float value)
        {
            float sig = Sigmoid(value);
            float dSig = sig*(1-sig);
            return Math.Max(dSig,0.0001f);
        }

        public static byte[] floatToByte(float[] data)
        {
            byte[] byteArray = new byte[data.Length * 4];
            Buffer.BlockCopy(data, 0, byteArray, 0, byteArray.Length);

            return byteArray;
        }

        public static List<float> byteToFloat(byte[] data)
        {
            float[] floatArray = new float[data.Length / 4];
            Buffer.BlockCopy(data, 0, floatArray, 0, data.Length);

            List<float> floatList = new List<float>();
            for(int i=0;i<floatArray.Length;i++)
                floatList.Add(floatArray[i]);

            return floatList;
        }
    }

    public enum NeuronType
    {
        Input = 0,
        Convolutional = 1,
        Pooling = 2,
        Connected = 3
    }

    public class InputMap : NeuralMap
    {
        public InputMap(int inMapSize)
        {
            type = NeuronType.Input;

            mapSize = inMapSize;
            mapArea = mapSize*mapSize;

            activations = new List<float>();
            for(int i=0;i<mapArea;i++) activations.Add(0);
            values = new List<float>();
            for(int i=0;i<mapArea;i++) values.Add(0);
            dValues = new List<float>();
            for(int i=0;i<mapArea;i++) dValues.Add(0);
            dValuesCount = new List<int>();
            for(int i=0;i<mapArea;i++) dValuesCount.Add(0);

            //Console.WriteLine("Added Input map with size "+mapSize);
        }

        public override void Update()
        {
            for(int i=0;i<mapArea;i++) dValues[i] = 0;
            for(int i=0;i<mapArea;i++) dValuesCount[i] = 0;         
        }

        public override void CalculateChanges()
        {
        }

        public override void Improve()
        {
            for(int i=0;i<mapArea;i++) dValues[i] = 0;
        }

        public override List<float> GetValues()
        {
            return values;
        }

        public override void SetValues(List<float> inValues)
        {
            values = inValues;
        }
    }

    public class ConvolutionalMap : NeuralMap
    {
        public int filterSize;
        public List<NeuralKernel> kernels;

        public ConvolutionalMap(int inFilterSize, List<NeuralMap> inPreviousMaps)
        {
            type = NeuronType.Convolutional;

            filterSize = inFilterSize;

            previousMaps = inPreviousMaps;
            mapSize = previousMaps[0].mapSize-filterSize+1;
            mapArea = mapSize*mapSize;

            activations = new List<float>();
            for(int i=0;i<mapArea;i++) activations.Add(0);
            values = new List<float>();
            for(int i=0;i<mapArea;i++) values.Add(0);
            dValues = new List<float>();
            for(int i=0;i<mapArea;i++) dValues.Add(0);
            dValuesCount = new List<int>();
            for(int i=0;i<mapArea;i++) dValuesCount.Add(0);
            kernels = new List<NeuralKernel>();
            for(int i=0;i<previousMaps.Count;i++) kernels.Add(new NeuralKernel(filterSize));

            //Console.WriteLine("Added Convolutional map with size "+mapSize);
        }

        override public void Update() 
        {
            for(int i=0;i<mapArea;i++) activations[i] = 0;

            for(int i=0;i<previousMaps.Count;i++)
            {
                NeuralMap previousMap = previousMaps[i];
                int previousMapSize = previousMap.mapSize;
                NeuralKernel kernel = kernels[i];
                for(int y=0;y<mapSize;y++)
                {
                    for(int x=0;x<mapSize;x++)
                    {
                        int mC = x+mapSize*y;
                        for(int dY=0;dY<filterSize;dY++)
                        {
                            for(int dX=0;dX<filterSize;dX++)
                            {
                                int pC = (x+dX)+previousMapSize*(y+dY);
                                int wC = dX+filterSize*dY;

                                activations[mC] += previousMap.values[pC]*kernel.weights[wC];
                            }
                        }
                    }
                }  
            }

            for(int i=0;i<mapArea;i++) values[i] = Global.Sigmoid(activations[i]);
            for(int i=0;i<mapArea;i++) dValues[i] = 0;
            for(int i=0;i<mapArea;i++) dValuesCount[i] = 0;         
        }

        public override void CalculateChanges()
        {
            for(int i=0;i<previousMaps.Count;i++)
            {
                NeuralMap previousMap = previousMaps[i];
                int previousMapSize = previousMap.mapSize;
                NeuralKernel kernel = kernels[i];
                for(int y=0;y<mapSize;y++)
                {
                    for(int x=0;x<mapSize;x++)
                    {
                        for(int dY=0;dY<filterSize;dY++)
                        {
                            for(int dX=0;dX<filterSize;dX++)
                            {
                                int mC = x+mapSize*y;
                                int pC = (x+dX)+previousMapSize*(y+dY);
                                int wC = dX+filterSize*dY;

                                kernel.dWeights[wC] += previousMap.values[pC]*dValues[mC];
                                kernel.dWeightsCount[wC]++;
                                if(previousMap.type!=NeuronType.Pooling)
                                    previousMap.dValues[pC] += kernel.weights[wC]*Global.DSigmoid(previousMap.activations[pC])*dValues[mC];
                                else
                                    previousMap.dValues[pC] += kernel.weights[wC]*dValues[mC];
                                previousMap.dValuesCount[pC]++;
                            }
                        }
                    }
                }  
            }
        }

        public override void Improve()
        {
            foreach(NeuralKernel kernel in kernels)
                kernel.Improve();
            for(int i=0;i<mapArea;i++) dValues[i] = 0;
            for(int i=0;i<mapArea;i++) dValuesCount[i] = 0;         
        }
        
        public override List<float> GetValues()
        {
            return values;
        }

        public override void SetValues(List<float> inValues)
        {
            values = inValues;
        }
    }

    public class PoolingMap : NeuralMap
    {
        public int poolingSize;

        public PoolingMap(int inPoolingSize, NeuralMap inPreviousMap)
        {
            type = NeuronType.Pooling;

            poolingSize = inPoolingSize;
            previousMaps = new List<NeuralMap>() {inPreviousMap};
            mapSize = previousMaps[0].mapSize/poolingSize;
            mapArea = mapSize*mapSize;

            activations = new List<float>();
            for(int i=0;i<mapArea;i++) activations.Add(0);
            values = new List<float>();
            for(int i=0;i<mapArea;i++) values.Add(0);
            dValues = new List<float>();
            for(int i=0;i<mapArea;i++) dValues.Add(0);
            dValuesCount = new List<int>();
            for(int i=0;i<mapArea;i++) dValuesCount.Add(0);

            //Console.WriteLine("Added Pooling map with size "+mapSize);
        }

        override public void Update()
        {
            for(int i=0;i<previousMaps.Count;i++)
            {
                NeuralMap previousMap = previousMaps[i];
                int previousMapSize = previousMap.mapSize;
                for(int y=0;y<mapSize;y++)
                {
                    for(int x=0;x<mapSize;x++)
                    {
                        float maximum = previousMap.values[(x*poolingSize)+previousMapSize*(y*poolingSize)];
                        for(int dY=0;dY<poolingSize;dY++)
                        {
                            for(int dX=0;dX<poolingSize;dX++)
                            {
                                float value = previousMap.values[(x*poolingSize+dX)+previousMapSize*(y*poolingSize+dY)];
                                maximum = value > maximum ? value : maximum;
                            }
                        }
                        activations[x+mapSize*y] = maximum;
                        values[x+mapSize*y] = maximum;
                    }
                }
            }
            for(int i=0;i<mapArea;i++) dValues[i] = 0;
            for(int i=0;i<mapArea;i++) dValuesCount[i] = 0;         
        }

        public override void CalculateChanges()
        {
            for(int i=0;i<previousMaps.Count;i++)
            {
                NeuralMap previousMap = previousMaps[i];
                int previousMapSize = previousMap.mapSize;
                for(int y=0;y<mapSize;y++)
                {
                    for(int x=0;x<mapSize;x++)
                    {
                        float maximum = previousMap.values[(x*poolingSize)+previousMapSize*(y*poolingSize)];
                        int maximumIndex = (x*poolingSize)+previousMapSize*(y*poolingSize);
                        for(int dY=0;dY<poolingSize;dY++)
                        {
                            for(int dX=0;dX<poolingSize;dX++)
                            {
                                float value = previousMap.values[(x*poolingSize+dX)+previousMapSize*(y*poolingSize+dY)];
                                maximumIndex = value > maximum ? ((x*poolingSize+dX)+previousMapSize*(y*poolingSize+dY)) : maximumIndex;
                                maximum = value > maximum ? value : maximum;
                            }
                        }
                        previousMap.dValues[maximumIndex] += dValues[x+mapSize*y]*Global.DSigmoid(previousMap.activations[maximumIndex]);
                        previousMap.dValuesCount[maximumIndex]++;
                    }
                }
            }
        }

        public override void Improve()
        {
            for(int i=0;i<mapArea;i++) dValues[i] = 0;
            for(int i=0;i<mapArea;i++) dValuesCount[i] = 0;
       }

        public override List<float> GetValues()
        {
            return values;
        }

        public override void SetValues(List<float> inValues)
        {
            values = inValues;
        }
    }

    public class ConnectedMap : NeuralMap
    {
        public List<float> bias;
        public List<float> dBias;
        public List<int> dBiasCount;
        public List<float> weights;
        public List<float> dWeights;
        public List<int> dWeightsCount;

        public ConnectedMap(int inMapArea, List<NeuralMap> inPreviousMaps)
        {
            type = NeuronType.Connected;

            previousMaps = inPreviousMaps;
            mapSize = inMapArea;
            mapArea = inMapArea;

            activations = new List<float>();
            for(int i=0;i<mapArea;i++) activations.Add(0);
            values = new List<float>();
            for(int i=0;i<mapArea;i++) values.Add(0);
            dValues = new List<float>();
            for(int i=0;i<mapArea;i++) dValues.Add(0);
            dValuesCount = new List<int>();
            for(int i=0;i<mapArea;i++) dValuesCount.Add(0);
            bias = new List<float>();
            for(int i=0;i<mapArea;i++) bias.Add(Global.RandomFloat(1f,-1f));
            dBias = new List<float>();
            for(int i=0;i<mapArea;i++) dBias.Add(0);
            dBiasCount = new List<int>();
            for(int i=0;i<mapArea;i++) dBiasCount.Add(0);
            weights = new List<float>();
            for(int i=0;i<mapArea*previousMaps.Count*previousMaps[0].mapArea;i++) weights.Add(Global.RandomFloat(1f,-1f));
            dWeights = new List<float>();
            for(int i=0;i<mapArea*previousMaps.Count*previousMaps[0].mapArea;i++) dWeights.Add(0);
            dWeightsCount = new List<int>();
            for(int i=0;i<mapArea*previousMaps.Count*previousMaps[0].mapArea;i++) dWeightsCount.Add(0);

            //Console.WriteLine("Added Connected map with size "+mapArea);
        }

        override public void Update()
        {
            for(int i=0;i<mapArea;i++) activations[i] = bias[i];

            for(int i=0;i<previousMaps.Count;i++)
            {
                NeuralMap previousMap = previousMaps[i];
                int previousMapArea = previousMap.mapArea;
                for(int j=0;j<mapArea;j++)
                {
                    for(int k=0;k<previousMapArea;k++)
                    {
                        activations[j] += previousMap.values[k]*weights[i+previousMaps.Count*j+previousMaps.Count*mapArea*k];
                    }
                }
            }
            for(int i=0;i<mapArea;i++) values[i] = Global.Sigmoid(activations[i]);
            for(int i=0;i<mapArea;i++) dValues[i] = 0;
            for(int i=0;i<mapArea;i++) dValuesCount[i] = 0;
        }

        override public void CalculateChanges()
        {            
            for(int i=0;i<previousMaps.Count;i++)
            {
                NeuralMap previousMap = previousMaps[i];
                int previousMapArea = previousMap.mapArea;
                for(int j=0;j<mapArea;j++)
                {
                    dBias[j] += dValues[j];
                    dBiasCount[j]++;
                    for(int k=0;k<previousMapArea;k++)
                    {
                        dWeights[i+previousMaps.Count*j+previousMaps.Count*mapArea*k] += previousMap.values[k]*dValues[j];
                        dWeightsCount[i+previousMaps.Count*j+previousMaps.Count*mapArea*k]++;
                        if(previousMap.type!=NeuronType.Pooling)
                            previousMap.dValues[k] += weights[i+previousMaps.Count*j+previousMaps.Count*mapArea*k]*Global.DSigmoid(previousMap.activations[k])*dValues[j];
                        else
                            previousMap.dValues[k] += weights[i+previousMaps.Count*j+previousMaps.Count*mapArea*k]*dValues[j];
                        previousMap.dValuesCount[k]++;            
                    }
                }
            }        
        }

        public override void Improve()
        {
            for(int i=0;i<mapArea;i++) bias[i] -= NeuralMap.learnRate*dBias[i]/Global.BATCH_SIZE;
            for(int i=0;i<mapArea;i++) dBias[i] = 0;
            for(int i=0;i<mapArea;i++) dBiasCount[i] = 0;
            for(int i=0;i<mapArea*previousMaps.Count*previousMaps[0].mapArea;i++) weights[i] -= NeuralMap.learnRate*dWeights[i]/Global.BATCH_SIZE;
            for(int i=0;i<mapArea*previousMaps.Count*previousMaps[0].mapArea;i++) dWeights[i] = 0;
            for(int i=0;i<mapArea*previousMaps.Count*previousMaps[0].mapArea;i++) dWeightsCount[i] = 0;
            for(int i=0;i<mapArea;i++) dValues[i] = 0;
            for(int i=0;i<mapArea;i++) dValuesCount[i] = 0;

        }

        public override List<float> GetValues()
        {
            return values;
        }

        public override void SetValues(List<float> inValues)
        {
            values = inValues;
        }
    }

    abstract public class NeuralMap
    {
        public NeuronType type;

        public bool keepSize = false;

        public int mapSize = 0;
        public int mapArea = 0;

        public static float learnRate = 1f;

        public List<NeuralMap> previousMaps = new List<NeuralMap>();

        public List<float> activations = new List<float>();

        public List<float> values = new List<float>();
        public List<float> dValues = new List<float>();
        public List<int> dValuesCount = new List<int>();

        public abstract void SetValues(List<float> inValues);

        public abstract List<float> GetValues();

        public abstract void Update();

        public abstract void CalculateChanges();

        public abstract void Improve();
    }

    public class NeuralKernel
    {
        public int kernelSize;
        public int kernelArea;
        public List<float> weights;
        public List<float> dWeights;
        public List<int> dWeightsCount;

        public NeuralKernel(int inKernelSize) 
        {
            kernelSize = inKernelSize;
            kernelArea = kernelSize*kernelSize;

            weights = new List<float>();
            for(int i=0;i<kernelArea;i++) weights.Add(Global.RandomFloat(1f,-1f));
            dWeights = new List<float>();
            for(int i=0;i<kernelArea;i++) dWeights.Add(0);
            dWeightsCount = new List<int>();
            for(int i=0;i<kernelArea;i++) dWeightsCount.Add(0);
        }

        public void Improve()
        {
            for(int i=0;i<kernelArea;i++) weights[i] -= NeuralMap.learnRate*dWeights[i]/Global.BATCH_SIZE;
            for(int i=0;i<kernelArea;i++) dWeights[i] = 0;
            for(int i=0;i<kernelArea;i++) dWeightsCount[i] = 0;
        }
    }

    public class ConvolutionalNet
    {
        public List<List<NeuralMap>> neuralMaps;

        public float cost = 0;
        public int correct = 0;

        public ConvolutionalNet()
        {
            neuralMaps = new List<List<NeuralMap>>();

            int layerCount = 9;
            int[] layerSizes = {3,10,10,15,15,20,20,1,1};
            int[] mapSizes = {48,46,23,20,10,8,4,25,5};
            NeuronType[] neuronTypes = {NeuronType.Input,NeuronType.Convolutional,NeuronType.Pooling,NeuronType.Convolutional,NeuronType.Pooling,NeuronType.Convolutional,NeuronType.Pooling,NeuronType.Connected,NeuronType.Connected};
            int[] filterSizes = {1,3,2,4,2,3,2,1,1};

            for(int i=0;i<layerCount;i++)
            {
                NeuronType type = neuronTypes[i];
                neuralMaps.Add(new List<NeuralMap>());
                for(int j=0;j<layerSizes[i];j++)
                {
                    if(type==NeuronType.Input)
                        neuralMaps[i].Add(new InputMap(mapSizes[i]));
                    else if(type==NeuronType.Convolutional)
                        neuralMaps[i].Add(new ConvolutionalMap(filterSizes[i],neuralMaps[i-1]));
                    else if(type==NeuronType.Pooling)
                        neuralMaps[i].Add(new PoolingMap(filterSizes[i],neuralMaps[i-1][j]));
                    else if(type==NeuronType.Connected)
                        neuralMaps[i].Add(new ConnectedMap(mapSizes[i],neuralMaps[i-1]));
                }

            }
        }

        public ConvolutionalNet(int layerCount, int[] layerSizes, int[] mapSizes, NeuronType[] neuronTypes, int[] filterSizes)
        {
            neuralMaps = new List<List<NeuralMap>>();

            for(int i=0;i<layerCount;i++)
            {
                NeuronType type = neuronTypes[i];
                neuralMaps.Add(new List<NeuralMap>());
                for(int j=0;j<layerSizes[i];j++)
                {
                    if(type==NeuronType.Input)
                        neuralMaps[i].Add(new InputMap(mapSizes[i]));
                    else if(type==NeuronType.Convolutional)
                        neuralMaps[i].Add(new ConvolutionalMap(filterSizes[i],neuralMaps[i-1]));
                    else if(type==NeuronType.Pooling)
                        neuralMaps[i].Add(new PoolingMap(filterSizes[i],neuralMaps[i-1][j]));
                    else if(type==NeuronType.Connected)
                        neuralMaps[i].Add(new ConnectedMap(mapSizes[i],neuralMaps[i-1]));
                }

            }
        }

        public void SetInput(byte[] inputR, byte[] inputG, byte[] inputB)
        {
            //Console.WriteLine("Hello, World");

            int inputSize = 48*48;

            List<float> dataR = new List<float>();
            List<float> dataG = new List<float>();
            List<float> dataB = new List<float>();

            for(int i=0;i<inputSize;i++)
            {
                dataR.Add((((float)inputR[i])/255.0f));
                dataG.Add((((float)inputG[i])/255.0f));
                dataB.Add((((float)inputB[i])/255.0f));
            }

            neuralMaps[0][0].SetValues(dataR);
            neuralMaps[0][1].SetValues(dataG);
            neuralMaps[0][2].SetValues(dataB);
        }

        public List<float> GetOutput()
        {
            return neuralMaps[neuralMaps.Count-1][0].GetValues();
        }

        public void CalculateCost(List<float> realValues)
        {
            List<float> values = GetOutput();
            int maxIndex = neuralMaps.Count-1;
            for(int i=0;i<values.Count;i++)
            {
                neuralMaps[maxIndex][0].dValues[i] = 2*(neuralMaps[maxIndex][0].values[i]-realValues[i])*Global.DSigmoid(neuralMaps[maxIndex][0].activations[i]);
                cost += Global.Pow(values[i]-realValues[i],2);
            }
        }

        public bool Correct(int number)
        {
            int maxIndex = 0;
            List<float> values = GetOutput();
            float max = values[0];
            for(int i=0;i<values.Count;i++)
            {
                //Console.Write(values[i]+", ");
                maxIndex = (values[i]) > max ? i : maxIndex;
                max = (values[i]) > max ? (values[i]) : max;
            }
            //Console.WriteLine("-> "+maxIndex+" ("+number+")");
            correct += number==maxIndex ? 1 : 0;
            return number==maxIndex;
        }

        public void Update()
        {
            for(int i=0;i<neuralMaps.Count;i++)
            {
                for(int l=0;l<neuralMaps[i].Count;l++)
                {
                    neuralMaps[i][l].Update();
                }
            }
        }

        public void CalculateChanges()
        {
            for(int i=neuralMaps.Count-1;i>=0;i--)
            {
                for(int l=0;l<neuralMaps[i].Count;l++)
                {
                    neuralMaps[i][l].CalculateChanges();
                }
            }
        }

        public void Improve()
        {
            for(int i=0;i<neuralMaps.Count;i++)
            {
                for(int l=0;l<neuralMaps[i].Count;l++)
                {
                    neuralMaps[i][l].Improve();
                }
            }
        }
    }
}