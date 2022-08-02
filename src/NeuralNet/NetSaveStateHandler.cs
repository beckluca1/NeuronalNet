namespace NeuralNet
{
    public static class NetSaveStateHandler
    {
        public static byte[] saveFromNet(ConvolutionalNet net)
        {
            int layerCount = 0;
            List<int> layerSizes = new List<int>();
            List<int> mapSizes = new List<int>();
            List<int> previousMapCount = new List<int>();
            List<int> info = new List<int>();
            List<NeuronType> neuronTypes = new List<NeuronType>();

            List<float> DataList = new List<float>();

            DataList = new List<float>();
            layerCount = net.neuralMaps.Count;
            DataList.Add(layerCount);
            for(int i=0;i<layerCount;i++)
            {
                layerSizes.Add(net.neuralMaps[i].Count);
                mapSizes.Add(net.neuralMaps[i][0].mapSize);
                previousMapCount.Add(net.neuralMaps[i][0].previousMaps.Count);
                neuronTypes.Add(net.neuralMaps[i][0].type);
                if(neuronTypes[i]==NeuronType.Input)
                    info.Add(((InputMap)net.neuralMaps[i][0]).mapSize);
                else if(neuronTypes[i]==NeuronType.Convolutional)
                    info.Add(((ConvolutionalMap)net.neuralMaps[i][0]).filterSize);
                else if(neuronTypes[i]==NeuronType.Pooling)
                    info.Add(((PoolingMap)net.neuralMaps[i][0]).poolingSize);
                else
                    info.Add(((ConnectedMap)net.neuralMaps[i][0]).previousMaps[0].mapArea);
   
                DataList.Add(layerSizes[i]);
                DataList.Add(mapSizes[i]);
                DataList.Add(previousMapCount[i]);
                DataList.Add((float)neuronTypes[i]);
                DataList.Add(info[i]);
            }

            for(int i=0;i<layerCount;i++)
            {
                if(neuronTypes[i]==NeuronType.Convolutional)
                {
                    for(int j=0;j<layerSizes[i];j++)
                    {
                        for(int k=0;k<previousMapCount[i];k++)
                        {
                            for(int l=0;l<info[i]*info[i];l++)
                            {
                                DataList.Add(((ConvolutionalMap)net.neuralMaps[i][j]).kernels[k].weights[l]);
                            }
                        }
                    }
                }
                else if(neuronTypes[i]==NeuronType.Connected)
                {
                    for(int j=0;j<layerSizes[i];j++)
                    {
                        for(int k=0;k<mapSizes[i];k++)
                        {
                            DataList.Add(((ConnectedMap)net.neuralMaps[i][j]).bias[k]);
                        }
                        for(int k=0;k<mapSizes[i]*previousMapCount[i]*info[i];k++)
                        {
                            DataList.Add(((ConnectedMap)net.neuralMaps[i][j]).weights[k]);
                        }
                    }
                }
            }

            byte[] dataListArray = Global.floatToByte(DataList.ToArray());

            return dataListArray;
        }

       public static ConvolutionalNet readFromSaveStateCNN(byte[] inDataByteList)
        {
            int layerCount = 0;
            List<int> layerSizes = new List<int>();
            List<int> mapSizes = new List<int>();
            List<int> previousMapCount = new List<int>();
            List<int> info = new List<int>();
            List<NeuronType> neuronTypes = new List<NeuronType>();

            List<float> DataList = Global.byteToFloat(inDataByteList);
            layerCount = (int)DataList[0];
            DataList.RemoveRange(0,1);
            for(int i=0;i<layerCount;i++)
            {
                layerSizes.Add((int)DataList[0]);
                mapSizes.Add((int)DataList[1]);
                previousMapCount.Add((int)DataList[2]);
                neuronTypes.Add((NeuronType)DataList[3]);
                if(neuronTypes[i]==NeuronType.Input)
                    info.Add((int)DataList[4]);
                else if(neuronTypes[i]==NeuronType.Convolutional)
                    info.Add((int)DataList[4]);
                else if(neuronTypes[i]==NeuronType.Pooling)
                    info.Add((int)DataList[4]);
                else
                    info.Add((int)DataList[4]);
   
                DataList.RemoveRange(0,5);
            }
            ConvolutionalNet net = new ConvolutionalNet(layerCount, layerSizes.ToArray(), mapSizes.ToArray(), neuronTypes.ToArray(), info.ToArray());
            
            for(int i=0;i<layerCount;i++)
            {
                if(neuronTypes[i]==NeuronType.Convolutional)
                {
                    for(int j=0;j<layerSizes[i];j++)
                    {
                        for(int k=0;k<previousMapCount[i];k++)
                        {
                            for(int l=0;l<info[i]*info[i];l++)
                            {
                                ((ConvolutionalMap)net.neuralMaps[i][j]).kernels[k].weights[l] = DataList[0];
                                DataList.RemoveRange(0,1);
                            }
                        }
                    }
                }
                else if(neuronTypes[i]==NeuronType.Connected)
                {
                    for(int j=0;j<layerSizes[i];j++)
                    {
                        for(int k=0;k<mapSizes[i];k++)
                        {
                            ((ConnectedMap)net.neuralMaps[i][j]).bias[k] = DataList[0];
                            DataList.RemoveRange(0,1);
                        }
                        for(int k=0;k<mapSizes[i]*previousMapCount[i]*info[i];k++)
                        {
                            ((ConnectedMap)net.neuralMaps[i][j]).weights[k] = DataList[0];
                            DataList.RemoveRange(0,1);
                        }
                    }
                }
            }            
            return net;
        }

        public static ProposalNeuralNet readFromSaveStateRPN(byte[] inDataByteList)
        {
            int layerCount = 0;
            List<int> layerSizes = new List<int>();
            List<int> mapSizes = new List<int>();
            List<int> previousMapCount = new List<int>();
            List<int> info = new List<int>();
            List<NeuronType> neuronTypes = new List<NeuronType>();

            List<float> DataList = Global.byteToFloat(inDataByteList);
            layerCount = (int)DataList[0];
            DataList.RemoveRange(0,1);
            for(int i=0;i<layerCount;i++)
            {
                layerSizes.Add((int)DataList[0]);
                mapSizes.Add((int)DataList[1]);
                previousMapCount.Add((int)DataList[2]);
                neuronTypes.Add((NeuronType)DataList[3]);
                if(neuronTypes[i]==NeuronType.Input)
                    info.Add((int)DataList[4]);
                else if(neuronTypes[i]==NeuronType.Convolutional)
                    info.Add((int)DataList[4]);
                else if(neuronTypes[i]==NeuronType.Pooling)
                    info.Add((int)DataList[4]);
                else
                    info.Add((int)DataList[4]);
   
                DataList.RemoveRange(0,5);
            }
            ProposalNeuralNet net = new ProposalNeuralNet(layerCount, layerSizes.ToArray(), mapSizes.ToArray(), neuronTypes.ToArray(), info.ToArray());
            
            for(int i=0;i<layerCount;i++)
            {
                if(neuronTypes[i]==NeuronType.Convolutional)
                {
                    for(int j=0;j<layerSizes[i];j++)
                    {
                        for(int k=0;k<previousMapCount[i];k++)
                        {
                            for(int l=0;l<info[i]*info[i];l++)
                            {
                                ((ConvolutionalMap)net.neuralMaps[i][j]).kernels[k].weights[l] = DataList[0];
                                DataList.RemoveRange(0,1);
                            }
                        }
                    }
                }
                else if(neuronTypes[i]==NeuronType.Connected)
                {
                    for(int j=0;j<layerSizes[i];j++)
                    {
                        for(int k=0;k<mapSizes[i];k++)
                        {
                            ((ConnectedMap)net.neuralMaps[i][j]).bias[k] = DataList[0];
                            DataList.RemoveRange(0,1);
                        }
                        for(int k=0;k<mapSizes[i]*previousMapCount[i]*info[i];k++)
                        {
                            ((ConnectedMap)net.neuralMaps[i][j]).weights[k] = DataList[0];
                            DataList.RemoveRange(0,1);
                        }
                    }
                }
            }            
            return net;
        }
    } 
}