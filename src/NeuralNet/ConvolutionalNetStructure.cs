namespace NeuralNet
{
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

            Console.WriteLine("Added Input map with size "+mapSize);
        }

        public void SetInput(byte[] inValues)
        {
            for(int i=0;i<mapArea;i++)
            {
                activations[i] = ((float)inValues[i])/255.0f;
                values[i] = ((float)inValues[i])/255.0f;
            }
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
        int filterSize;
        List<NeuralKernel> kernels;

        List<float> partialActivations;

        public ConvolutionalMap(int inFilterSize, List<NeuralMap> inPreviousMaps)
        {
            type = NeuronType.Convolutional;

            filterSize = inFilterSize;
            previousMaps = inPreviousMaps;
            mapSize = previousMaps[0].mapSize-filterSize+1;
            mapArea = mapSize*mapSize;

            partialActivations = new List<float>();
            for(int i=0;i<mapArea*previousMaps.Count;i++) partialActivations.Add(0);

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

            Console.WriteLine("Added Convolutional map with size "+mapSize);
        }

        override public void Update() 
        {
            for(int i=0;i<mapArea;i++) activations[i] = 0;
            for(int i=0;i<mapArea*previousMaps.Count;i++) partialActivations[i] = 0;

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
                                partialActivations[i+previousMaps.Count*x+previousMaps.Count*mapSize*y] += previousMap.values[(x+dX)+previousMapSize*(y+dY)]*kernel.weights[dX+filterSize*dY];
                                activations[x+mapSize*y] += previousMap.values[(x+dX)+previousMapSize*(y+dY)]*kernel.weights[dX+filterSize*dY];
                            }
                        }
                    }
                }  
            }

            for(int i=0;i<mapArea;i++) values[i] = Global.ReLu(activations[i]);
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
                                kernel.dWeights[dX+filterSize*dY] += previousMap.values[(x+dX)+previousMapSize*(y+dY)]*dValues[x+mapSize*y];
                                kernel.dWeightsCount[dX+filterSize*dY]++;
                                if(previousMap.type!=NeuronType.Pooling)
                                    previousMap.dValues[(x+dX)+previousMapSize*(y+dY)] += kernel.weights[dX+filterSize*dY]*Global.DReLu(previousMap.activations[(x+dX)+previousMapSize*(y+dY)])*dValues[x+mapSize*y];
                                else
                                    previousMap.dValues[(x+dX)+previousMapSize*(y+dY)] += kernel.weights[dX+filterSize*dY]*dValues[x+mapSize*y];
                                previousMap.dValuesCount[(x+dX)+previousMapSize*(y+dY)]++;
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
        int poolingSize;

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

            Console.WriteLine("Added Pooling map with size "+mapSize);
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
                        previousMap.dValues[maximumIndex] += dValues[x+mapSize*y]*Global.DReLu(previousMap.activations[maximumIndex]);
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
            for(int i=0;i<mapArea;i++) bias.Add(Global.RandomFloat(5,-5));
            dBias = new List<float>();
            for(int i=0;i<mapArea;i++) dBias.Add(0);
            dBiasCount = new List<int>();
            for(int i=0;i<mapArea;i++) dBiasCount.Add(0);
            weights = new List<float>();
            for(int i=0;i<mapArea*previousMaps.Count*previousMaps[0].mapArea;i++) weights.Add(Global.RandomFloat(1,-1));
            dWeights = new List<float>();
            for(int i=0;i<mapArea*previousMaps.Count*previousMaps[0].mapArea;i++) dWeights.Add(0);
            dWeightsCount = new List<int>();
            for(int i=0;i<mapArea*previousMaps.Count*previousMaps[0].mapArea;i++) dWeightsCount.Add(0);

            Console.WriteLine("Added Connected map with size "+mapArea);
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
            for(int i=0;i<mapArea;i++) values[i] = Global.ReLu(activations[i]);
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
                            previousMap.dValues[k] += weights[i+previousMaps.Count*j+previousMaps.Count*mapArea*k]*Global.DReLu(previousMap.activations[k])*dValues[j];
                        else
                            previousMap.dValues[k] += weights[i+previousMaps.Count*j+previousMaps.Count*mapArea*k]*dValues[j];
                        previousMap.dValuesCount[k]++;            
                    }
                }
            }        
        }

        public override void Improve()
        {
            for(int i=0;i<mapArea;i++) bias[i] -= NeuralMap.learnRate*dBias[i]/batchSize;
            for(int i=0;i<mapArea;i++) dBias[i] = 0;
            for(int i=0;i<mapArea;i++) dBiasCount[i] = 0;
            for(int i=0;i<mapArea*previousMaps.Count*previousMaps[0].mapArea;i++) weights[i] -= NeuralMap.learnRate*dWeights[i]/batchSize;
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

        public static int batchSize = 10;

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
            for(int i=0;i<kernelArea;i++) weights[i] -= NeuralMap.learnRate*dWeights[i]/NeuralMap.batchSize;
            for(int i=0;i<kernelArea;i++) dWeights[i] = 0;
            for(int i=0;i<kernelArea;i++) dWeightsCount[i] = 0;
        }
    }

    public class ConvolutionalNet
    {
        NeuronType type;

        List<List<NeuralMap>> neuralMaps = new List<List<NeuralMap>>(8);

        public float cost = 0;
        public int correct = 0;

        public ConvolutionalNet()
        {
            for(int i=0;i<9;i++)
            {
                neuralMaps.Add(new List<NeuralMap>());
            }

            int[] mapCount = {3,10,15,20};

            for(int i=0;i<mapCount[0];i++)
                neuralMaps[0].Add(new InputMap(46));

            for(int i=0;i<mapCount[1];i++)
                neuralMaps[1].Add(new ConvolutionalMap(3,neuralMaps[0]));
            for(int i=0;i<mapCount[1];i++)
                neuralMaps[2].Add(new PoolingMap(2,neuralMaps[1][i]));

            for(int i=0;i<mapCount[2];i++)
                neuralMaps[3].Add(new ConvolutionalMap(3,neuralMaps[2]));
            for(int i=0;i<mapCount[2];i++)
                neuralMaps[4].Add(new PoolingMap(2,neuralMaps[3][i]));

            for(int i=0;i<mapCount[3];i++)
                neuralMaps[5].Add(new ConvolutionalMap(3,neuralMaps[4]));
            for(int i=0;i<mapCount[3];i++)
                neuralMaps[6].Add(new PoolingMap(2,neuralMaps[5][i]));    

            neuralMaps[7].Add(new ConnectedMap(25,neuralMaps[6]));   
            neuralMaps[8].Add(new ConnectedMap(5,neuralMaps[7]));   

        }

        public void SetInput(byte[] inputR, byte[] inputG, byte[] inputB)
        {
            int inputSize = 46*46;

            List<float> dataR = new List<float>();
            List<float> dataG = new List<float>();
            List<float> dataB = new List<float>();

            for(int i=0;i<inputSize;i++)
            {
                dataR.Add((((float)inputR[i])/255.0f));
                dataG.Add((((float)inputG[i])/255.0f));
                dataB.Add((((float)inputB[i])/255.0f));
                //Console.WriteLine(((float)inputR[i])/255.0f+", "+((float)inputG[i])/255.0f+", "+((float)inputB[i])/255.0f);
            }

            neuralMaps[0][0].SetValues(dataR);
            neuralMaps[0][1].SetValues(dataG);
            neuralMaps[0][2].SetValues(dataB);
        }

        public List<float> GetOutput()
        {
            return neuralMaps[8][0].GetValues();
        }

        public void CalculateCost(List<float> realValues)
        {
            List<float> values = GetOutput();
            for(int i=0;i<values.Count;i++)
            {
                neuralMaps[8][0].dValues[i] = 2*(neuralMaps[8][0].values[i]-realValues[i])*Global.DReLu(neuralMaps[8][0].activations[i]);
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