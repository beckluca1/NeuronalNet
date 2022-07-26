namespace NeuralNet
{
    public class Rectangle
    {
        float x = 0;
        float y = 0;
        float width = 0;
        float height = 0;

        float minX = 0;
        float minY = 0;
        float maxX = 0;
        float maxY = 0;

        float area = 0;

        public Rectangle(float inX, float inY, float inWidth, float inHeight)
        {
            x = inX;
            y = inY;
            width = inWidth;
            height = inHeight;

            minX = x - width/2;
            minY = y - height/2;
            maxX = x + width/2;
            maxY = y + height/2;

            area = width*height;
        }

        public static float Intersection(Rectangle rect1, Rectangle rect2)
        {
            float intersectionX = Math.Max(rect1.minX,rect2.minX) - Math.Min(rect1.maxX,rect2.maxX);
            float intersectionY = Math.Max(rect1.minY,rect2.minY) - Math.Min(rect1.maxY,rect2.maxY);

            if(intersectionX<=0||intersectionY<=0)
                return (intersectionX)*(intersectionY);
            
            return intersectionX*intersectionY;
        }

        public static float Union(Rectangle rect1, Rectangle rect2)
        {
            return rect1.area + rect2.area - Intersection(rect1,rect2);
        }

        public static float IOU(Rectangle rect1, Rectangle rect2)
        {
            float intersection = Intersection(rect1,rect2);
            return intersection/(rect1.area + rect2.area - intersection);
        }

        public static float GIOU(Rectangle rect1, Rectangle rect2)
        {
            float intersection = Intersection(rect1,rect2);
            float union = rect1.area + rect2.area - intersection;
            float boundingX = (Math.Max(rect1.maxX,rect2.maxX)-Math.Min(rect1.minX,rect2.minX));
            float boundingY = (Math.Max(rect1.maxY,rect2.maxY)-Math.Min(rect1.minY,rect2.minY));
            float bouningArea = boundingX*boundingY;
            return intersection/union - (bouningArea-union)/bouningArea;
        }

        public static List<Rectangle> SortForDistance(List<Rectangle> inNetRectangles, List<Rectangle> inRealRectangles)
        {
            List<Rectangle> netRectangles = new List<Rectangle>();
            List<Rectangle> realRectangles = inRealRectangles;

            List<Rectangle> newOrderRectangles= new List<Rectangle>();

            List<int> indexes = new List<int>();
            for(int i=0;i<inNetRectangles.Count;i++)
            {
                indexes.Add(0);
                netRectangles.Add(inNetRectangles[i]);

            }

            for(int i=0;i<netRectangles.Count;i++)
            {
                float minDistance = GIOU(netRectangles[0],realRectangles[0]);
                int[] index = new int[]{0,0};
                for(int j=0;j<netRectangles.Count;j++)
                {
                    for(int k=0;k<realRectangles.Count;k++)
                    {
                        float distance = GIOU(netRectangles[j],realRectangles[k]);
                        index = distance < minDistance ? new int[]{j,k} : index;
                        minDistance = distance < minDistance ? distance : minDistance;
                    }
                }
                indexes[index[0]] = index[1];
                netRectangles.RemoveRange(index[0],1);
                newOrderRectangles.Add(inNetRectangles[index[0]]);
                realRectangles.RemoveRange(index[1],1);
            }
            return newOrderRectangles;
        }
    }

    public class ProposalNeuralNet
    {
        public List<List<NeuralMap>> neuralMaps = new List<List<NeuralMap>>(8);

        public float cost = 0;
        public int correct = 0;

        public ProposalNeuralNet()
        {
            int layerCount = 9;
            int[] layerSizes = {3,10,10,15,15,20,20,1};
            int[] mapSizes = {48,44,22,20,10,8,4,48*48*9};
            NeuronType[] neuronTypes = {NeuronType.Input,NeuronType.Convolutional,NeuronType.Pooling,NeuronType.Convolutional,NeuronType.Pooling,NeuronType.Convolutional,NeuronType.Pooling,NeuronType.Connected};
            int[] filterSizes = {1,3,2,3,2,3,2,1,1};

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

        public ProposalNeuralNet(int layerCount, int[] layerSizes, int[] mapSizes, NeuronType[] neuronTypes, int[] filterSizes)
        {
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