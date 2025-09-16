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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Cassandra.Connections.Control;
using Cassandra.Compression;
using Cassandra.Metrics;
using Cassandra.Observers.Abstractions;
using Cassandra.Requests;
using Cassandra.Responses;
using Cassandra.Serialization;
using Cassandra.Tasks;
using Microsoft.IO;

namespace Cassandra.Connections
{
    /// <inheritdoc />
    internal class Connection : IConnection
    {
    #pragma warning disable CS0067 
        /// <summary>
        /// The event that represents a event RESPONSE from a Cassandra node
        /// </summary>
        public event CassandraEventHandler CassandraEventResponse;

        /// <summary>
        /// Event raised when there is an error when executing the request to prevent idle disconnects
        /// </summary>
        public event Action<Exception> OnIdleRequestException;

        /// <summary>
        /// Event that gets raised when a write has been completed. Testing purposes only.
        /// </summary>
        public event Action WriteCompleted;

        /// <summary>
        /// Event that gets raised the connection is being closed.
        /// </summary>
        public event Action<IConnection> Closing;
#pragma warning restore CS0067

        private const string IdleQuery = "SELECT key FROM system.local WHERE key='local'";
        private const long CoalescingThreshold = 8000;

        public ISerializer Serializer => throw new NotImplementedException();

        public IFrameCompressor Compressor { get; set; }

        public IConnectionEndPoint EndPoint => throw new NotImplementedException();

        public IPEndPoint LocalAddress => throw new NotImplementedException();

        public int WriteQueueLength => throw new NotImplementedException();

        public int PendingOperationsMapLength => throw new NotImplementedException();

        /// <summary>
        /// Determines the amount of operations that are not finished.
        /// </summary>
        public virtual int InFlight => throw new NotImplementedException();

        /// <summary>
        /// Determines if there isn't any operations pending to be written or inflight.
        /// </summary>
        public virtual bool HasPendingOperations
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the amount of operations that timed out and didn't get a response
        /// </summary>
        public virtual int TimedOutOperations
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Determine if the Connection has been explicitly disposed
        /// </summary>
        public bool IsDisposed
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the current keyspace.
        /// </summary>
        public string Keyspace
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ProtocolOptions Options => Configuration.ProtocolOptions;

        public Configuration Configuration { get; set; }

        public int ShardID { get; set; }

        internal Connection(
            ISerializer serializer,
            IConnectionEndPoint endPoint,
            Configuration configuration,
            IStartupRequestFactory startupRequestFactory,
            IConnectionObserver connectionObserver)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the amount of concurrent requests depending on the protocol version
        /// </summary>
        public int GetMaxConcurrentRequests(ISerializer serializer)
        {
            if (!serializer.ProtocolVersion.Uses2BytesStreamIds())
            {
                return 128;
            }
            //Protocol 3 supports up to 32K concurrent request without waiting a response
            //Allowing larger amounts of concurrent requests will cause large memory consumption
            //Limit to 2K per connection sounds reasonable.
            return 2048;
        }

        public virtual void Dispose()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Close()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes the connection.
        /// </summary>
        /// <exception cref="SocketException">Throws a SocketException when the connection could not be established with the host</exception>
        /// <exception cref="AuthenticationException" />
        /// <exception cref="UnsupportedProtocolVersionException"></exception>
        public Task<Response> Open()
        {
            throw new NotImplementedException();        }

        /// <summary>
        /// Initializes the connection.
        /// </summary>
        /// <param name="shardID">Shard ID</param>
        /// <param name="shardCount">Shard count</param>
        /// <exception cref="SocketException">Throws a SocketException when the connection could not be established with the host</exception>
        /// <exception cref="AuthenticationException" />
        /// <exception cref="UnsupportedProtocolVersionException"></exception>
        public Task<Response> Open(int shardID = -1, int shardCount = 0)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes the connection.
        /// </summary>
        /// <exception cref="SocketException">Throws a SocketException when the connection could not be established with the host</exception>
        /// <exception cref="AuthenticationException" />
        /// <exception cref="UnsupportedProtocolVersionException"></exception>
        public Task<Response> DoOpen(int shardID = -1, int shardCount = 0)
        {
            throw new NotImplementedException();
        }

        public ShardingInfo ShardingInfo()
        {
            throw new NotImplementedException();        }

        public TabletInfo TabletInfo()
        {
            throw new NotImplementedException();        }

        public LwtInfo LwtInfo()
        {
            throw new NotImplementedException();        }

        /// <summary>
        /// Deserializes each frame header and copies the body bytes into a single buffer.
        /// </summary>
        /// <returns>True if a full operation (streamId) has been processed.</returns>
        internal bool ReadParse(byte[] buffer, int length)
        {
            throw new NotImplementedException();
        }

        public Task<Response> Send(IRequest request, int timeoutMillis)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<Response> Send(IRequest request)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public OperationState Send(IRequest request, Func<IRequestError, Response, Task> callback, int timeoutMillis)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public OperationState Send(IRequest request, Func<IRequestError, Response, Task> callback)
        {
            return Send(request, callback, Configuration.DefaultRequestOptions.ReadTimeoutMillis);
        }

        /// <summary>
        /// Removes an operation from pending and frees the stream id
        /// </summary>
        /// <param name="streamId"></param>
        protected internal virtual OperationState RemoveFromPending(short streamId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the keyspace of the connection.
        /// If the keyspace is different from the current value, it sends a Query request to change it
        /// </summary>
        public Task<bool> SetKeyspace(string value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Method that gets executed when a write request has been completed.
        /// </summary>
        protected virtual void WriteCompletedHandler()
        {
            throw new NotImplementedException();
        }
    }
}