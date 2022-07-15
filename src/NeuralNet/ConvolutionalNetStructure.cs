namespace NeuralNet
{
    public class InputMap : NeuralMap
    {
        public InputMap(int inMapSize)
        {
            mapSize = inMapSize;
            mapArea = mapSize*mapSize;

            values = new List<float>();
            for(int i=0;i<mapArea;i++) values.Add(0);
            dValues = new List<float>();
            for(int i=0;i<mapArea;i++) dValues.Add(0);

            Console.WriteLine("Added Input map with size "+mapSize);
        }

        public void SetInput(byte[] inValues)
        {
            for(int y=0;y<mapSize;y++)
            {
                for(int x=0;x<mapSize;x++)
                {
                    values[x+y*mapSize] = ((float)inValues[x+y*mapSize])/255.0f;
                }
            }
        }

        public override void Update()
        {
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
        int filterSize = 0;
        List<NeuralKernel> kernels = new List<NeuralKernel>();
        List<float> activations = new List<float>();

        public ConvolutionalMap(int inFilterSize, NeuralMap inPreviousMap)
        {
            filterSize = inFilterSize;
            previousMaps = new List<NeuralMap>(){inPreviousMap};
            mapSize = previousMaps[0].mapSize-filterSize+1;
            mapArea = mapSize*mapSize;

            activations = new List<float>();
            for(int i=0;i<mapArea;i++) activations.Add(0);
            values = new List<float>();
            for(int i=0;i<mapArea;i++) values.Add(0);
            dValues = new List<float>();
            for(int i=0;i<mapArea;i++) dValues.Add(0);
            for(int i=0;i<previousMaps.Count;i++)
            {
                kernels.Add(new NeuralKernel(filterSize));
            }

            Console.WriteLine("Added Convolutional map with size "+mapSize);
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
                        for(int dY=0;dY<filterSize;dY++)
                        {
                            for(int dX=0;dX<filterSize;dX++)
                            {
                                activations[x+mapSize*y] += previousMap.values[(x+dX)+previousMapSize*(y+dY)]*kernel.weights[dX+filterSize*dY];
                            }
                        }
                    }
                }  
            }

            for(int i=0;i<mapArea;i++) values[i] = Global.ReLu(activations[i])-0.5f;
        }

        public override void CalculateChanges()
        {
            for(int i=0;i<previousMaps.Count;i++)
            {
                NeuralMap previousMap = previousMaps[i];
                int previousMapSize = previousMap.mapSize;
                int previousMapArea = previousMap.mapArea;
                NeuralKernel kernel = kernels[i];
                for(int y=0;y<mapSize;y++)
                {
                    for(int x=0;x<mapSize;x++)
                    {
                        for(int dY=0;dY<filterSize;dY++)
                        {
                            for(int dX=0;dX<filterSize;dX++)
                            {
                                kernel.dWeights[dX+filterSize*dY] += previousMap.values[(x+dX)+previousMapSize*(y+dY)]*Global.DReLu(activations[x+mapSize*y])*dValues[x+mapSize*y];
                                previousMap.dValues[(x+dX)+previousMapSize*(y+dY)] += kernel.weights[dX+filterSize*dY]*Global.DReLu(activations[x+mapSize*y])*dValues[x+mapSize*y];
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
            poolingSize = inPoolingSize;
            previousMaps = new List<NeuralMap>() {inPreviousMap};
            mapSize = previousMaps[0].mapSize/poolingSize;
            mapArea = mapSize*mapSize;

            values = new List<float>();
            for(int i=0;i<mapArea;i++) values.Add(0);
            dValues = new List<float>();
            for(int i=0;i<mapArea;i++) dValues.Add(0);

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
                        float maximum = -10;
                        for(int dY=0;dY<poolingSize;dY++)
                        {
                            for(int dX=0;dX<poolingSize;dX++)
                            {
                                float value = previousMap.values[(x*poolingSize+dX)+previousMapSize*(y*poolingSize+dY)];
                                maximum = value > maximum ? value : maximum;
                            }
                        }
                        values[x+mapSize*y] = maximum;
                    }
                }
            }            
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
                        float maximum = -10;
                        int maximumIndex = 0;
                        for(int dY=0;dY<poolingSize;dY++)
                        {
                            for(int dX=0;dX<poolingSize;dX++)
                            {
                                float value = previousMap.values[(x*poolingSize+dX)+previousMapSize*(y*poolingSize+dY)];
                                maximumIndex = value > maximum ? ((x*poolingSize+dX)+previousMapSize*(y*poolingSize+dY)) : maximumIndex;
                                maximum = value > maximum ? value : maximum;
                            }
                        }
                        previousMap.dValues[maximumIndex] += dValues[x+mapSize*y];
                    }
                }
            }
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

    public class ConnectedMap : NeuralMap
    {
        public List<float> activations;
        public List<float> bias;
        public List<float> dBias;
        public List<float> weights;
        public List<float> dWeights;

        public ConnectedMap(int inMapArea, List<NeuralMap> inPreviousMaps)
        {
            previousMaps = inPreviousMaps;
            mapArea = inMapArea;

            activations = new List<float>();
            for(int i=0;i<mapArea;i++) activations.Add(0);
            values = new List<float>();
            for(int i=0;i<mapArea;i++) values.Add(0);
            dValues = new List<float>();
            for(int i=0;i<mapArea;i++) dValues.Add(0);
            bias = new List<float>();
            for(int i=0;i<mapArea;i++) bias.Add(Global.RandomFloat(1,-1));
            dBias = new List<float>();
            for(int i=0;i<mapArea;i++) dBias.Add(0);
            weights = new List<float>();
            for(int i=0;i<mapArea*previousMaps[0].mapArea;i++) weights.Add(Global.RandomFloat(1,-1));
            dWeights = new List<float>();
            for(int i=0;i<mapArea*previousMaps[0].mapArea;i++) dWeights.Add(0);

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
                        activations[j] += previousMap.values[k]*weights[j+mapArea*k];
                    }
                    
                }
            }
            for(int i=0;i<mapArea;i++) values[i] = Global.ReLu(activations[i]) -0.5f;
        }

        override public void CalculateChanges()
        {
            for(int i=0;i<previousMaps.Count;i++)
            {
                NeuralMap previousMap = previousMaps[i];
                int previousMapArea = previousMap.mapArea;
                for(int j=0;j<mapArea;j++)
                {
                    dBias[j] += Global.DReLu(activations[j])*dValues[j];
                    for(int k=0;k<previousMapArea;k++)
                    {
                        dWeights[j+mapArea*k] += previousMap.values[k]*Global.DReLu(activations[j])*dValues[j];
                        previousMap.dValues[k] += weights[j+mapArea*k]*Global.DReLu(activations[j])*dValues[j];            
                    }
                }
            }        
        }

        public override void Improve()
        {
            for(int i=0;i<mapArea;i++) bias[i] -= NeuralMap.learnRate*dBias[i];
            for(int i=0;i<mapArea;i++) dBias[i] = 0;
            for(int i=0;i<mapArea*previousMaps[0].mapArea;i++) weights[i] -= NeuralMap.learnRate*dWeights[i];
            for(int i=0;i<mapArea*previousMaps[0].mapArea;i++) dWeights[i] = 0;
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

    abstract public class NeuralMap
    {
        public int mapSize = 0;
        public int mapArea = 0;

        public static float learnRate = 0.005f;

        public List<NeuralMap> previousMaps = new List<NeuralMap>();

        public List<float> values = new List<float>();
        public List<float> dValues = new List<float>();

        public abstract void SetValues(List<float> inValues);

        public abstract List<float> GetValues();

        public abstract void Update();

        public abstract void CalculateChanges();

        public abstract void Improve();

        /*int mapDifference = 0;
        int poolingMapSize = 0;
        int poolingSize = 0;

        
        List<List<float>> poolingActivations = new List<List<float>>();

        List<List<float>> values = new List<List<float>>();
        List<List<float>> poolingValues = new List<List<float>>();

        List<List<float>> dPoolingValues = new List<List<float>>();

        List<NeuralMap> previousMaps = new List<NeuralMap>();
        List<NeuralKernel> neuralKernels = new List<NeuralKernel>();
        NeuralKernel poolingKernel = new NeuralKernel(1);

        public NeuralMap(int inMapSize) 
        {
            mapSize = inMapSize;
            poolingMapSize = inMapSize;

            for(int x=0;x<mapSize;x++)
            {
                activations.Add(new List<float>());
                values.Add(new List<float>());
                dValues.Add(new List<float>());

                for(int y=0;y<mapSize;y++)
                {
                    activations[x].Add(0);
                    values[x].Add(0);
                    dValues[x].Add(0);
                }
            }

            for(int x=0;x<poolingMapSize;x++)
            {
                poolingActivations.Add(new List<float>());
                poolingValues.Add(new List<float>());
                dPoolingValues.Add(new List<float>());

                for(int y=0;y<poolingMapSize;y++)
                {
                    poolingActivations[x].Add(0);
                    poolingValues[x].Add(0);
                    dPoolingValues[x].Add(0);
                }
            } 
        }

        public NeuralMap(List<NeuralKernel> inNeuralKernels, NeuralKernel inPoolingKernel, List<NeuralMap> inPreviousMaps) 
        {
            neuralKernels = inNeuralKernels;
            poolingKernel = inPoolingKernel;
            previousMaps = inPreviousMaps;
            mapDifference =  (neuralKernels[0].GetKernelSize()-1)/2;
            mapSize = previousMaps[0].poolingMapSize - 2*mapDifference;
            poolingSize = poolingKernel.GetKernelSize();
            poolingMapSize = mapSize/poolingSize;

            for(int x=0;x<mapSize;x++)
            {
                activations.Add(new List<float>());
                values.Add(new List<float>());
                dValues.Add(new List<float>());

                for(int y=0;y<mapSize;y++)
                {
                    activations[x].Add(0);
                    values[x].Add(0);
                    dValues[x].Add(0);
                }
            }

            for(int x=0;x<poolingMapSize;x++)
            {
                poolingActivations.Add(new List<float>());
                poolingValues.Add(new List<float>());
                dPoolingValues.Add(new List<float>());

                for(int y=0;y<poolingMapSize;y++)
                {
                    poolingActivations[x].Add(0);
                    poolingValues[x].Add(0);
                    dPoolingValues[x].Add(0);
                }
            }           
        }

        public void Update() 
        {
            int mD = mapDifference;
                                
            for(int x=0;x<mapSize;x++)
            {
                for(int y=0;y<mapSize;y++)
                {
                    activations[x][y] = 0;
                    for(int i=0;i<previousMaps.Count;i++)
                    {
                        for(int dX=-mD;dX<=mD;dX++)
                        {
                            for(int dY=-mD;dY<=mD;dY++)
                            {
                                activations[x][y] += previousMaps[i].poolingValues[mD+x+dX][mD+y+dY]*neuralKernels[i].getWeights()[mD+dX][mD+dY];
                            }
                        }

                    }
                    values[x][y] = Global.Sigmoid(activations[x][y]);
                }
            }

            for(int x=0;x<poolingMapSize;x++)
            {
                for(int y=0;y<poolingMapSize;y++)
                {
                    poolingActivations[x][y] = 0;
                    for(int dX=0;dX<poolingSize;dX++)
                    {
                        for(int dY=0;dY<poolingSize;dY++)
                        {
                            poolingActivations[x][y] += values[x*poolingSize+dX][y*poolingSize+dY]*poolingKernel.getWeights()[dX][dY];
                        }
                    }
                    poolingValues[x][y] = Global.Sigmoid(poolingActivations[x][y]);
                }
            }

        }

        public void SetValues(byte[] inValues)
        {
            for(int x=0;x<mapSize;x++)
            {
                for(int y=0;y<mapSize;y++)
                {
                    poolingValues[x][y] = ((float)inValues[y*mapSize+x])/255.0f;
                }
            }
        }

        public void resetDValues() 
        {
            for(int x=0;x<mapSize;x++)
            {
                for(int y=0;y<mapSize;y++)
                {
                    dValues[x][y] = 0;
                }
            }
            for(int x=0;x<poolingMapSize;x++)
            {
                for(int y=0;y<poolingMapSize;y++)
                {
                    dPoolingValues[x][y] = 0;
                }
            }
        }

        public float GetValue()
        {
            return poolingValues[0][0];
        }

        public void CalculateChanges(float inDValue)
        {
            dValues[0][0] = inDValue;
            CalculateChanges();
        }

        public void CalculateChanges()
        {
            for(int x=0;x<poolingMapSize;x++)
            {
                for(int y=0;y<poolingMapSize;y++)
                {
                    for(int dX=0;dX<poolingSize;dX++)
                    {
                        for(int dY=0;dY<poolingSize;dY++)
                        {
                            poolingKernel.AddDWeight(dX,dY,values[x*poolingSize+dX][y*poolingSize+dY]*Global.DSigmoid(poolingActivations[x][y])*dPoolingValues[x][y]/(mapSize*mapSize));
                            dValues[x*poolingSize+dX][y*poolingSize+dY] += poolingKernel.getWeights()[dX][dY]*Global.DSigmoid(poolingActivations[x][y])*dPoolingValues[x][y]/(mapSize*mapSize);                        
                        }
                    }
                }
            }

            int mD = mapDifference;
            for(int x=0;x<mapSize;x++)
            {
                for(int y=0;y<mapSize;y++)
                {
                    for(int i=0;i<previousMaps.Count;i++)
                    {
                        for(int dX=-mD;dX<=mD;dX++)
                        {
                            for(int dY=-mD;dY<=mD;dY++)
                            {
                                neuralKernels[i].AddDWeight(mD+dX,mD+dY,previousMaps[i].poolingValues[mD+x+dX][mD+y+dY]*Global.DSigmoid(activations[x][y])*dValues[x][y]/(previousMaps.Count*previousMaps[0].mapSize*previousMaps[0].mapSize));
                                previousMaps[i].dPoolingValues[mD+x+dX][mD+y+dY] += neuralKernels[i].getWeights()[mD+dX][mD+dY]*Global.DSigmoid(activations[x][y])*dValues[x][y]/(previousMaps.Count*previousMaps[0].mapSize*previousMaps[0].mapSize);
                            }
                        }
                    }
                }
            }
        }*/
    }

    public class NeuralKernel
    {
        public int kernelSize = 0;
        public int kernelArea = 0;
        public List<float> weights = new List<float>();
        public List<float> dWeights = new List<float>();

        public NeuralKernel(int inKernelSize) 
        {
            kernelSize = inKernelSize;
            kernelArea = kernelSize*kernelSize;

            weights = new List<float>();
            for(int i=0;i<kernelArea;i++) weights.Add(Global.RandomFloat(1f,-1f));
            dWeights = new List<float>();
            for(int i=0;i<kernelArea;i++) dWeights.Add(0);
        }

        public void Improve()
        {
            for(int i=0;i<kernelArea;i++) weights[i] -= NeuralMap.learnRate*dWeights[i];
            for(int i=0;i<kernelArea;i++) dWeights[i] = 0;
        }
    }

    public class ConvolutionalNet
    {

        List<List<NeuralMap>> neuralMaps = new List<List<NeuralMap>>(8);


        public ConvolutionalNet()
        {
            for(int i=0;i<9;i++)
            {
                neuralMaps.Add(new List<NeuralMap>());
            }

            int[] mapCount = {3,100,150,200};

            for(int i=0;i<mapCount[0];i++)
                neuralMaps[0].Add(new InputMap(46));

            for(int i=0;i<mapCount[1];i++)
                neuralMaps[1].Add(new ConvolutionalMap(3,neuralMaps[0][i%mapCount[0]]));
            for(int i=0;i<mapCount[1];i++)
                neuralMaps[2].Add(new PoolingMap(2,neuralMaps[1][i]));

            for(int i=0;i<mapCount[2];i++)
                neuralMaps[3].Add(new ConvolutionalMap(3,neuralMaps[2][i%mapCount[1]]));
            for(int i=0;i<mapCount[2];i++)
                neuralMaps[4].Add(new PoolingMap(2,neuralMaps[3][i]));

            for(int i=0;i<mapCount[3];i++)
                neuralMaps[5].Add(new ConvolutionalMap(3,neuralMaps[4][i%mapCount[2]]));
            for(int i=0;i<mapCount[3];i++)
                neuralMaps[6].Add(new PoolingMap(2,neuralMaps[5][i]));    

            neuralMaps[7].Add(new ConnectedMap(250,neuralMaps[6]));   
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
                dataR.Add((((float)inputR[i])/255.0f-0.5f));
                dataG.Add((((float)inputG[i])/255.0f-0.5f));
                dataB.Add((((float)inputB[i])/255.0f-0.5f));
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

        public float CalculateCost(List<float> realValues)
        {
            float cost = 0;
            List<float> values = GetOutput();
            for(int i=0;i<values.Count;i++)
            {
                neuralMaps[8][0].dValues[i] = 2*(values[i]+0.5f-realValues[i]);
                cost += Global.Pow(values[i]+0.5f-realValues[i],2);
            }
            return cost;
        }

        public bool Correct(int number)
        {
            int maxIndex = 0;
            float max = 0;
            List<float> values = GetOutput();
            for(int i=0;i<values.Count;i++)
            {
                maxIndex = (values[i]+0.5f) > max ? i : maxIndex;
                max = (values[i]+0.5f) > max ? (values[i]+0.5f) : max;
            }
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
            //NeuralMap.learnRate *= 0.9f;
            // Console.WriteLine(NeuralMap.learnRate);

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