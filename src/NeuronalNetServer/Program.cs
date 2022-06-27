using NeuronalNetServer.Helpers;

Thread grpcThread = new Thread(ServiceInitialization.StartGrpcService);
// Thread neuralNetThread = new Thread();

grpcThread.Start();
// neuralNetThread.Start();

