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

using Cassandra.Collections;
using Cassandra.Connections;
using Cassandra.ExecutionProfiles;
using Cassandra.Observers.Abstractions;
using Cassandra.Serialization;
using Cassandra.SessionManagement;
using Cassandra.Tasks;

namespace Cassandra.Requests
{
    /// <inheritdoc />
    internal class RequestHandler : IRequestHandler
    {
        public const long StateInit = 0;
        public const long StateCompleted = 1;

        public IExtendedRetryPolicy RetryPolicy { get; }
        public ISerializer Serializer { get; }
        public IStatement Statement { get; }
        public IRequestOptions RequestOptions { get; }

        /// <summary>
        /// Creates a new instance using a request, the statement and the execution profile.
        /// </summary>
        public RequestHandler(
            IInternalSession session, ISerializer serializer, IRequest request, SessionRequestInfo sessionRequestInfo, IRequestOptions requestOptions, IRequestObserver requestObserver)
        {
            Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            Statement = sessionRequestInfo.Statement;
            RequestOptions = requestOptions ?? throw new ArgumentNullException(nameof(requestOptions));

            RetryPolicy = RequestOptions.RetryPolicy;

            if (sessionRequestInfo.Statement?.RetryPolicy != null)
            {
                RetryPolicy = sessionRequestInfo.Statement.RetryPolicy.Wrap(RetryPolicy);
            }
        }

        /// <summary>
        /// Creates a new instance using the statement to build the request.
        /// Statement can not be null.
        /// </summary>
        public RequestHandler(IInternalSession session, ISerializer serializer, SessionRequestInfo sessionRequestInfo, IRequestOptions requestOptions, IRequestObserver requestObserver)
            : this(session, serializer, RequestHandler.GetRequest(sessionRequestInfo.Statement, serializer, requestOptions), sessionRequestInfo, requestOptions, requestObserver)
        {
        }

        /// <summary>
        /// Creates a new instance with no request, suitable for getting a connection.
        /// </summary>
        public RequestHandler(IInternalSession session, ISerializer serializer, SessionRequestInfo sessionRequestInfo, IRequestObserver requestObserver)
            : this(session, serializer, null, sessionRequestInfo, session.Cluster.Configuration.DefaultRequestOptions, requestObserver)
        {
        }

        /// <inheritdoc />
        public IRequest BuildRequest()
        {
            throw new NotImplementedException();
        }

        public bool OnNewNodeExecution(NodeRequestInfo nodeRequestInfo)
        {
            throw new NotImplementedException();
        }

        public bool SetNodeExecutionCompleted(Guid executionId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the Request to send to a cassandra node based on the statement type
        /// </summary>
        internal static IRequest GetRequest(IStatement statement, ISerializer serializer, IRequestOptions requestOptions)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<bool> SetCompletedAsync(Exception ex, RowSet result = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<bool> SetCompletedAsync(RowSet result, Func<Task> action)
        {
            throw new NotImplementedException();
        }

        public Task SetNoMoreHostsAsync(NoHostAvailableException ex, IRequestExecution execution)
        {
            throw new NotImplementedException();
        }

        public bool HasCompleted()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ValidHost GetNextValidHost(Dictionary<IPEndPoint, Exception> triedHosts)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<IConnection> GetNextConnectionAsync(Dictionary<IPEndPoint, Exception> triedHosts)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<IConnection> ValidateHostAndGetConnectionAsync(HostShard hostShard, Dictionary<IPEndPoint, Exception> triedHosts)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<IConnection> GetConnectionToValidHostAsync(ValidHost validHost, IDictionary<IPEndPoint, Exception> triedHosts, int shardID = -1)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a connection from a host or null if its not possible, filling the triedHosts map with the failures.
        /// </summary>
        /// <param name="host">Host to which a connection will be obtained.</param>
        /// <param name="distance">Output parameter that will contain the <see cref="HostDistance"/> associated with
        /// <paramref name="host"/>. It is retrieved from the current <see cref="ILoadBalancingPolicy"/>.</param>
        /// <param name="session">Session from where a connection will be obtained (or created).</param>
        /// <param name="triedHosts">Hosts for which there were attempts to connect and send the request.</param>
        /// <param name="routingKey">Routing key to use for the next host.</param>
        /// <param name="shardID">Shard to use.</param>
        /// <exception cref="InvalidQueryException">When the keyspace is not valid</exception>
        internal static Task<IConnection> GetConnectionFromHostAsync(
            Host host, HostDistance distance, IInternalSession session, IDictionary<IPEndPoint, Exception> triedHosts, RoutingKey routingKey = null, int shardID = -1)
        {
            throw new NotImplementedException();
        }

        public Task<RowSet> SendAsync()
        {
            throw new NotImplementedException();
        }

        public static Task<Tuple<SessionRequestInfo, IRequestObserver>> CreateRequestObserver(IInternalSession session, IStatement statement)
        {
            throw new NotImplementedException();
        }
    }
}