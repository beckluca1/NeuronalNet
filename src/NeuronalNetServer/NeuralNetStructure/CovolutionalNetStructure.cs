namespace NeuronalNetServer
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

        List<NeuralMap> previousMaps = new List<NeuralMap>();
        List<NeuralKernel> neuralKernels = new List<NeuralKernel>();
        NeuralKernel poolingKernel = new NeuralKernel(1);

        public NeuralMap(int inMapSize) 
        {
            mapSize = inMapSize;
            poolingMapSize = inMapSize;

            for(int x=0;x<=mapSize;x++)
            {
                activations.Add(new List<float>());
                for(int y=0;y<=mapSize;y++)
                {
                    activations[x].Add(0);
                    values[x].Add(0);
                }
            }

            for(int x=0;x<=poolingMapSize;x++)
            {
                poolingActivations.Add(new List<float>());
                for(int y=0;y<=poolingMapSize;y++)
                {
                    poolingActivations[x].Add(0);
                    poolingValues[x].Add(0);
                }
            } 
        }

        public NeuralMap(List<NeuralKernel> inNeuralKernels, NeuralKernel inPoolingKernel, List<NeuralMap> inPreviousMaps) 
        {
            neuralKernels = inNeuralKernels;
            poolingKernel = inPoolingKernel;
            previousMaps = inPreviousMaps;
            mapDifference =  (neuralKernels[0].GetKernelSize()-1)/2;
            mapSize = previousMaps[0].mapSize - 2*mapDifference;
            poolingSize = inPoolingKernel.GetKernelSize();
            poolingMapSize = mapSize/poolingSize;

            for(int x=0;x<=mapSize;x++)
            {
                activations.Add(new List<float>());
                for(int y=0;y<=mapSize;y++)
                {
                    activations[x].Add(0);
                    values[x].Add(0);
                }
            }

            for(int x=0;x<=poolingMapSize;x++)
            {
                poolingActivations.Add(new List<float>());
                for(int y=0;y<=poolingMapSize;y++)
                {
                    poolingActivations[x].Add(0);
                    poolingValues[x].Add(0);
                }
            }           
        }

        public void Update() 
        {
            int mD = mapDifference;
            for(int x=mD;x<=mapSize+mD;x++)
            {
                for(int y=mD;y<=mapSize+mD;y++)
                {
                    activations[x-mD][y-mD] = 0;
                    for(int i=0;i<=previousMaps.Count;i++)
                    {
                        for(int dX=-mD;dX<=mD;dX++)
                        {
                            for(int dY=-mD;dY<=mD;dY++)
                            {
                                activations[x-mD][y-mD] += previousMaps[i].activations[x+dX][y+dY]*neuralKernels[i].getWeights()[dX][dY];
                            }
                        }
                    }
                    values[x-mD][y-mD] = Global.Sigmoid(activations[x-mD][y-mD]);
                }
            }

            for(int x=0;x<=poolingMapSize;x++)
            {
                for(int y=0;y<=poolingMapSize;y++)
                {
                    poolingActivations[x][y] = 0;
                    for(int dX=0;dX<=poolingKernel.GetKernelSize();dX++)
                    {
                        for(int dY=0;dY<=poolingKernel.GetKernelSize();dY++)
                        {
                            poolingActivations[x][y] += activations[x*poolingKernel.GetKernelSize()+dX][y*poolingKernel.GetKernelSize()+dY]*poolingKernel.getWeights()[dX][dY];
                        }
                    }
                    poolingValues[x][y] = Global.Sigmoid(poolingActivations[x][y]);
                }
            }

        }

        public float getValue()
        {
            return poolingValues[0][0];
        }
    }

    public class NeuralKernel
    {
        int kernelSize = 0;
        List<List<float>> weights = new List<List<float>>();

        //NEURAL KERNEL SIZE HAS TO BE AN ODD NUMBER!!!
        public NeuralKernel(int inKernelSize) 
        {
            kernelSize = inKernelSize;

            for(int x=0;x<=kernelSize;x++)
            {
                weights.Add(new List<float>());
                for(int y=0;y<=kernelSize;y++)
                {
                    weights[x].Add(Global.RandomFloat(-1,1));
                }
            }
        }

        public int GetKernelSize() 
        {
            return kernelSize;
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
            for(int i=layerCount-1;i>0;i--)
            {
                inputSize *= inPoolingSize[i];
                inputSize += (inConvolutionalSize[i]-1)/2;
            }

            for(int i=0;i<=layerCount;i++)
            {
                neuralMaps.Add(new List<NeuralMap>());
                neuralKernels.Add(new List<NeuralKernel>());

                for(int l=0;l<layerSize[i];l++)
                {
                    if(i!=0)
                    {
                        neuralMaps[i].Add(new NeuralMap(neuralKernels[i-1],poolingKernels[i-1],neuralMaps[i-1]));
                    }
                    else
                    {
                        neuralMaps[i].Add(new NeuralMap(inputSize));
                    }


                    neuralKernels[i].Add(new NeuralKernel(convolutionalSize[i]));
                }
                poolingKernels[i] = new NeuralKernel(poolingSize[i]);
            }
        }

        public void Update()
        {
            for(int i=0;i<=layerCount;i++)
            {
                for(int l=0;l<layerSize[i];l++)
                {
                    neuralMaps[i][l].Update();
                }
            }

        }
    }
}