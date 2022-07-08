using NeuronalNetServer.Helpers;

Thread grpcThread = new Thread(ServiceInitialization.StartGrpcService);
Thread neuralNetThread = new Thread(ServiceInitialization.StartNeuralNetService);

grpcThread.Start();
neuralNetThread.Start();

