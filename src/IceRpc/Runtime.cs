// Copyright (c) ZeroC, Inc. All rights reserved.

using System.Runtime.CompilerServices;

// Make internals visible to the tests assembly, to allow writing unit tests for the internal classes
[assembly: InternalsVisibleTo("IceRpc.Tests.Internal")]
[assembly: InternalsVisibleTo("IceRpc.Tests.Encoding")]

namespace IceRpc
{
    /// <summary>Provides global configuration for IceRPC in the current process.</summary>
    public static class Runtime
    {
        /// <summary>The IceRPC version in semver format.</summary>
        public const string StringVersion = "0.0.1-alpha";
    }
}
