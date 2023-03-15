// Copyright (c) ZeroC, Inc.

using MultipleInterfacesExample;
using IceRpc;

// Create a server that will dispatch all requests to the same service, an instance of Chatbot.
await using var server = new Server(new Chatbot());
server.Listen();

// Wait until the console receives a Ctrl+C.
await CancelKeyPressed;
await server.ShutdownAsync();
