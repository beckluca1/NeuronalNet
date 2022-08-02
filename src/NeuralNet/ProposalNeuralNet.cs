namespace NeuralNet
{
    public class Rectangle
    {
        public float x = 0;
        public float y = 0;
        public float width = 0;
        public float height = 0;

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

        public static List<Rectangle> GetAllRectangles()
        {
            List<Rectangle> allRectangles = new List<Rectangle>();

            int imageSize = 48;
            int[] rectSizes = new int[]{6,10,16,22,28,36};
            float[] ratios = new float[]{1.2f,1,0.8f};

            for(int x=0;x<imageSize;x+=8)
            {
                for(int y=0;y<imageSize;y+=8)
                {
                    for(int sizeIndex=0;sizeIndex<rectSizes.Length;sizeIndex++)
                    {
                        if(x-rectSizes[sizeIndex]/2<0||x+rectSizes[sizeIndex]/2>=48||y-rectSizes[sizeIndex]/2<0||y+rectSizes[sizeIndex]/2>=48)
                            continue;

                        allRectangles.Add(new Rectangle(((float)x)/48f,((float)y)/48f,((float)rectSizes[sizeIndex])/48f,((float)rectSizes[sizeIndex])/48f));
                    }
                }
            }
            Console.WriteLine(allRectangles.Count);

            return allRectangles;
        }   

        public float Intersection(Rectangle rect)
        {
            float intersectionX = Math.Min(maxX,rect.maxX) - Math.Max(minX,rect.minX);
            float intersectionY = Math.Min(maxY,rect.maxY) - Math.Max(minY,rect.minY);

            if(intersectionX<=0||intersectionY<=0)
                return 0;
            
            return intersectionX*intersectionY;
        }

        public float Bounding(Rectangle rect)
        {
            float boundingX = (Math.Max(maxX,rect.maxX) - Math.Min(minX,rect.minX));
            float boundingY = (Math.Max(maxY,rect.maxY) - Math.Min(minY,rect.minY));
            
            return boundingX * boundingY;
        }

        public float Union(Rectangle rect)
        {
            return area + rect.area - Intersection(rect);
        }

        public float GIOU(Rectangle rect)
        {
            float intersection = Intersection(rect);
            float union = Union(rect);
            float bounding = Bounding(rect);

            float giou = intersection/union;// - (bounding-union)/bounding;

            return giou;
        }

        public int GetBestRectangle(List<Rectangle> inRectangleList)
        {
            float maxDistance = GIOU(inRectangleList[0]);
            int maxIndex = 0;
            for(int i=0;i<inRectangleList.Count;i++)
            {
                float distance = GIOU(inRectangleList[i]);
                maxIndex = distance > maxDistance ? i : maxIndex;
                maxDistance = distance > maxDistance ? distance : maxDistance;
            }
            return maxIndex;
        }
    }

    public class ProposalNeuralNet : ConvolutionalNet
    {
        public List<Rectangle> allRectangles;

        public ProposalNeuralNet()
        {
            neuralMaps = new List<List<NeuralMap>>();

            allRectangles = Rectangle.GetAllRectangles();

            int layerCount = 9;
            int[] layerSizes = {3,10,10,15,15,20,20,1,1};
            int[] mapSizes = {48,46,23,20,10,8,4,250,allRectangles.Count};
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
                    {
                        Console.WriteLine(neuralMaps[i-1]);
                        neuralMaps[i].Add(new PoolingMap(filterSizes[i],neuralMaps[i-1][j]));
                    }
                    else if(type==NeuronType.Connected)
                        neuralMaps[i].Add(new ConnectedMap(mapSizes[i],neuralMaps[i-1]));
                }

            }
        }

        public ProposalNeuralNet(int layerCount, int[] layerSizes, int[] mapSizes, NeuronType[] neuronTypes, int[] filterSizes)
        {
            neuralMaps = new List<List<NeuralMap>>();

            allRectangles = Rectangle.GetAllRectangles();

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

        public Rectangle presesntBest()
        {
            List<float> output = GetOutput();

            float maxCert = output[0];
            int maxIndex = 0;
            for(int i=0;i<output.Count;i++)
            {
                maxIndex = output[i]>maxCert ? i : maxIndex;
                maxCert = output[i]>maxCert ? output[i] : maxCert;
            }

            Rectangle bestRectangle = allRectangles[maxIndex];
            return bestRectangle;
        }
    }
}