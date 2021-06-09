// Copyright (c) ZeroC, Inc. All rights reserved.

using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace IceRpc.Transports.Internal
{
    internal class TcpAcceptor : IAcceptor
    {
        public Endpoint Endpoint { get; }
        internal IPEndPoint IPEndPoint { get; }

        private readonly ILogger _logger;
        private readonly IncomingConnectionOptions _options;
        private readonly Socket _socket;

        public async ValueTask<MultiStreamConnection> AcceptAsync()
        {
            Socket fd = await _socket.AcceptAsync().ConfigureAwait(false);

            SingleStreamConnection singleStreamConnection = 
                    ((TcpEndpoint)Endpoint).CreateSingleStreamConnection(fd, _logger);

            MultiStreamOverSingleStreamConnection multiStreamConnection = Endpoint.Protocol switch
            {
                Protocol.Ice1 => new Ice1Connection(Endpoint, singleStreamConnection, _options),
                _ => new SlicConnection(Endpoint, singleStreamConnection, _options)
            };
            return multiStreamConnection;
        }

        public void Dispose() => _socket.CloseNoThrow();

        public override string ToString() => $"{base.ToString()} {IPEndPoint}";

        internal TcpAcceptor(Socket socket, TcpEndpoint endpoint, IncomingConnectionOptions options, ILogger logger)
        {
            Debug.Assert(!endpoint.HasDnsHost); // not a DNS name

            _logger = logger;
            _options = options;
            _socket = socket;

            Endpoint = endpoint;
            IPEndPoint = (IPEndPoint)socket.LocalEndPoint!;
        }
    }
}
