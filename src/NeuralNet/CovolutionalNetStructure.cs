namespace NeuralNet
{
    public class NeuralMap
    {
        int mapSize = 0;
        int mapDifference = 0;
        int poolingMapSize = 0;
        int poolingSize = 0;

        List<List<float>> activations = new List<List<float>>();
        List<List<float>> poolingActivations = new List<List<float>>();

        List<List<float>> values = new List<List<float>>();
        List<List<float>> poolingValues = new List<List<float>>();

        List<List<float>> dValues = new List<List<float>>();
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
            for(int x=mD;x<mapSize+mD;x++)
            {
                for(int y=mD;y<mapSize+mD;y++)
                {
                    for(int i=0;i<previousMaps.Count;i++)
                    {
                        for(int dX=-mD;dX<=mD;dX++)
                        {
                            for(int dY=-mD;dY<=mD;dY++)
                            {
                                neuralKernels[i].AddDWeight(mD+dX,mD+dY,previousMaps[i].poolingValues[x+dX][y+dY]*Global.DSigmoid(activations[x-mD][y-mD])*dValues[x-mD][y-mD]/(previousMaps.Count*previousMaps[0].mapSize*previousMaps[0].mapSize));
                                previousMaps[i].dPoolingValues[x+dX][y+dY] += neuralKernels[i].getWeights()[mD+dX][mD+dY]*Global.DSigmoid(activations[x-mD][y-mD])*dValues[x-mD][y-mD]/(previousMaps.Count*previousMaps[0].mapSize*previousMaps[0].mapSize);
                            }
                        }
                    }
                }
            }
        }
    }

    public class NeuralKernel
    {
        int kernelSize = 0;
        List<List<float>> weights = new List<List<float>>();
        List<List<float>> dWeights = new List<List<float>>();

        //NEURAL KERNEL SIZE HAS TO BE AN ODD NUMBER!!!
        public NeuralKernel(int inKernelSize) 
        {
            kernelSize = inKernelSize;

            for(int x=0;x<kernelSize;x++)
            {
                weights.Add(new List<float>());
                dWeights.Add(new List<float>());
                for(int y=0;y<kernelSize;y++)
                {
                    weights[x].Add(Global.RandomFloat(1,-1));
                    dWeights[x].Add(0);
                }
            }
        }

        public void Improve()
        {
            for(int x=0;x<kernelSize;x++)
            {
                for(int y=0;y<kernelSize;y++)
                {
                    weights[x][y] -= dWeights[x][y];
                    dWeights[x][y] = 0;
                }
            }
        }

        public int GetKernelSize() 
        {
            return kernelSize;
        }

        public void AddDWeight(int x, int y, float inDWeight)
        {
            dWeights[x][y] += inDWeight; 
        }

        public List<List<float>> getWeights()
        {
            return weights;
        }
    }

    public class ConvolutionalNet
    {
        int layerCount = 0;
        int[] layerSize;
        int[] convolutionalSize;
        int[] poolingSize;

        List<List<NeuralMap>> neuralMaps = new List<List<NeuralMap>>();
        List<List<NeuralKernel>> neuralKernels = new List<List<NeuralKernel>>();
        List<NeuralKernel> poolingKernels = new List<NeuralKernel>();


        public ConvolutionalNet(int inLayerCount, int[] inLayerSize, int[] inConvolutionalSize, int[] inPoolingSize)
        {
            layerCount = inLayerCount;
            layerSize = inLayerSize;
            convolutionalSize = inConvolutionalSize;
            poolingSize = inPoolingSize;

            int inputSize = 1;
            for(int i=layerCount-2;i>=0;i--)
            {
                inputSize *= inPoolingSize[i];
                inputSize += (inConvolutionalSize[i]-1);
            }

            Console.WriteLine("Net has an input size of " + inputSize);


            neuralMaps.Add(new List<NeuralMap>());
            neuralKernels.Add(new List<NeuralKernel>());
            poolingKernels.Add(new NeuralKernel(0));

            for(int l=0;l<layerSize[0];l++)
            {
                neuralMaps[0].Add(new NeuralMap(inputSize));
            }

            for(int i=1;i<layerCount;i++)
            {
                neuralMaps.Add(new List<NeuralMap>());
                neuralKernels.Add(new List<NeuralKernel>());
                for(int l=0;l<layerSize[i];l++)
                {
                    neuralKernels[i].Add(new NeuralKernel(convolutionalSize[i]));
                }
                for(int l=0;l<layerSize[i];l++)
                {
                    NeuralKernel poolingKernel = new NeuralKernel(poolingSize[i]);
                    poolingKernels.Add(poolingKernel);
                    List<NeuralKernel> neuralKernelList = new List<NeuralKernel>();
                    for(int p=0;p<layerSize[i-1];p++)
                    {
                        NeuralKernel neuralKernel = new NeuralKernel(convolutionalSize[i]);
                        neuralKernels[i].Add(neuralKernel);
                        neuralKernelList.Add(neuralKernel);
                    }
                    neuralMaps[i].Add(new NeuralMap(neuralKernelList,poolingKernel,neuralMaps[i-1]));
                }
            }
        }

        public void SetInput(byte[] inputR, byte[] inputG, byte[] inputB)
        {
            neuralMaps[0][0].SetValues(inputR);
            neuralMaps[0][1].SetValues(inputG);
            neuralMaps[0][2].SetValues(inputB);
        }

        public List<float> getOutput()
        {
            List<float> output = new List<float>();
            for(int l=0;l<layerSize[layerCount-1];l++)
            {
                output.Add(neuralMaps[layerCount-1][l].GetValue());
            }
            return output;
        }

        public void Update()
        {
            for(int i=1;i<layerCount;i++)
            {
                for(int l=0;l<layerSize[i];l++)
                {
                    neuralMaps[i][l].Update();
                }
            }

        }

        public void CalculateChanges(List<float> dValues)
        {
            for(int i=0;i<layerSize[layerCount-1];i++)
            {
                neuralMaps[layerCount-1][i].CalculateChanges(dValues[i]);
            }
            
            for(int i=layerCount-2;i>=1;i--)
            {
                for(int l=0;l<layerSize[i];l++)
                {
                    neuralMaps[i][l].CalculateChanges();
                }
            }
        }

        public void Improve()
        {
            foreach(List<NeuralKernel> layer in neuralKernels)
            {
                foreach(NeuralKernel kernel in layer)
                {
                    kernel.Improve();
                }
            }

            foreach(NeuralKernel kernel in poolingKernels)
            {
                kernel.Improve();
            }

            foreach(List<NeuralMap> maps in neuralMaps)
            {
                foreach(NeuralMap map in maps)
                {   
                    map.resetDValues();
                }
            }


        }
    }
}