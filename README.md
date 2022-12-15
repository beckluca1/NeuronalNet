# Neuronal Net

## About

NeuronalNet is going to be a small neural network that recognizes symbols uploaded by a user. These
users can interact with it on a small website which is based on
[Microsoft's Blazor framework](https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor).
Beside Blazor for the frontend, we will use
[gRPC on .NET](https://docs.microsoft.com/en-us/aspnet/core/grpc/?view=aspnetcore-6.0) for the
server application. By using protocol buffers, the client will be able to send its data to
the server to enhance the neural net. The goal is to create a consistent neural network over time
to precisely recognize different types of symbols.

## Client - Side

Users can upload pictures of different kinds of traffic signs. Once uploaded, the server responds
with the best working neural net at that time. Thus our database expands and the corresponding
neural net improves, while the client is able to predict what image has been uploaded.

The website also includes a simple dashboard which contains information about all processed traffic
signs. You can check how many signs of specific sign types have been uploaded so far and what
the latest neural net is capable of.

## Server - Side

The server mainly consists of three parts: a gRPC server to handle client requests, a database to
store traffic signs, entire traffic images as we call them and neural net states, and the
calculation process to improve the neural net.

Any client requests are made via gRPC which seemed more straightforward than creating a dedicated
REST-service or something similar. It processes incoming traffic signs/images and is able to
upload neural net states. Therefore clients can easily predict images locally and do not rely on
an addtional server.

The database (MariaDB) stores all the data we need on one hand, and works as a connection
between the gRPC service and calculation process on the other hand. First one inserts new images
and extracts neural net states while second one extracts all images available and inserts enhanced
version of the neural net.

As already mentioned, the calculation process impvoes the neural net while using all images which
are held by the database. It's basically the heart of the image classification.

## Environment Variables

| Key                         | Value                               |
|:---------------------------:|:-----------------------------------:|
| MARIADB_PASSWORD            | database user password              |
| NEURO__DB_CONNECTION_STRING | credentials for database connection |
