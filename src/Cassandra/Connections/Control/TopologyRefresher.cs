// 
//       Copyright (C) DataStax Inc.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//       http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Cassandra.Serialization;
using Cassandra.Tasks;

namespace Cassandra.Connections.Control
{
    /// <inheritdoc />
    internal class TopologyRefresher : ITopologyRefresher
    {
        private const string SelectPeers = "SELECT * FROM system.peers";
        private const string SelectPeersV2 = "SELECT * FROM system.peers_v2";
        private const string SelectLocal = "SELECT * FROM system.local WHERE key='local'";

        private static readonly IPAddress BindAllAddress = new IPAddress(new byte[4]);

        private readonly Configuration _config;
        private readonly Metadata _metadata;

        public TopologyRefresher(Metadata metadata, Configuration config)
        {
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <inheritdoc />
        public Task<Host> RefreshNodeListAsync(
            IConnectionEndPoint currentEndPoint, IConnection connection, ISerializer serializer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Parses address from system table query response and translates it using the provided <paramref name="translator"/>.
        /// </summary>
        internal IPEndPoint GetRpcEndPoint(bool isPeersV2, IRow row, IAddressTranslator translator, int defaultPort)
        {
            throw new NotImplementedException();
        }
    }
}