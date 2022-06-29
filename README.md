# Neuronal Net

## About

NeuronalNet is going to be a small neural network that recognizes symbols drawn by a user. Thes
users can interact with it on a small website which is based on
[Microsoft's Blazor framework](https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor).
Beside Blazor for the frontend, we will use
[gRPC on .NET](https://docs.microsoft.com/en-us/aspnet/core/grpc/?view=aspnetcore-6.0) for the
server application. By using protocol buffers, the client will be able to send its data to
the server to enhance the neural net. The goal is to create a consistent neural network over time
to precisely recognize different types of symbols.

## Client - Side

WORK IN PROGRESS

## Server - Side

WORK IN PROGRESS

## Environment Variables

| Key                         | Value                               |
|:---------------------------:|:-----------------------------------:|
| MARIADB_PASSWORD            | database user password              |
| NEURO__DB_CONNECTION_STRING | credentials for database connection |
