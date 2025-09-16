//
//      Copyright (C) DataStax Inc.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Cassandra.Responses;
using Cassandra.Serialization;
using Cassandra.SessionManagement;
using Cassandra.Tasks;

namespace Cassandra.Connections.Control
{
    internal class ControlConnection : IControlConnection
    {
        /// <summary>
        /// Gets the binary protocol version to be used for this cluster.
        /// </summary>
        public ProtocolVersion ProtocolVersion => throw new NotImplementedException();

        /// <inheritdoc />
        public Host Host
        {
            get => throw new NotImplementedException();
            internal set => throw new NotImplementedException();
        }

        public IConnectionEndPoint EndPoint => throw new NotImplementedException();

        public IPEndPoint LocalAddress => throw new NotImplementedException();

        public ISerializerManager Serializer => throw new NotImplementedException();

        internal ControlConnection(
            IInternalCluster cluster,
            ProtocolVersion initialProtocolVersion,
            Configuration config,
            Metadata metadata,
            IEnumerable<IContactPoint> contactPoints)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task InitAsync()
        {
            throw new NotImplementedException();
        }

        internal void OnConnectionClosing(IConnection connection)
        {
            throw new NotImplementedException();
        }

        internal Task<IConnection> Reconnect(IConnection closedConnection)
        {
            throw new NotImplementedException();
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task HandleSchemaChangeEvent(SchemaChangeEventArgs ssc, bool processNow)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Uses the active connection to execute a query
        /// </summary>
        public IEnumerable<IRow> Query(string cqlQuery, bool retry = false)
        {
            throw new NotImplementedException();
    }

        public Task<IEnumerable<IRow>> QueryAsync(string cqlQuery, bool retry = false)
        {
            throw new NotImplementedException();
        }

        public Task<Response> SendQueryRequestAsync(string cqlQuery, bool retry, QueryProtocolOptions queryProtocolOptions)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<Response> UnsafeSendQueryRequestAsync(string cqlQuery, QueryProtocolOptions queryProtocolOptions)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task HandleKeyspaceRefreshLaterAsync(string keyspace)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task ScheduleKeyspaceRefreshAsync(string keyspace, bool processNow)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task ScheduleAllKeyspacesRefreshAsync(bool processNow)
        {
            throw new NotImplementedException();
        }

        public bool IsShardAware()
        {
            throw new NotImplementedException();
        }
    }
}