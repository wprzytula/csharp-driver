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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Cassandra.Collections;
using Cassandra.Connections;
using Cassandra.ExecutionProfiles;
using Cassandra.Metrics;
using Cassandra.Metrics.Internal;
using Cassandra.Observers.Abstractions;
using Cassandra.Serialization;
using Cassandra.SessionManagement;
using Cassandra.Tasks;

namespace Cassandra
{
    /// <inheritdoc cref="ISession" />
    public class Session : ISession, /* FIXME: Reimplement tests and remove this stub: */ IInternalSession
    {
        private static readonly Logger Logger = new Logger(typeof(Session));
        private readonly ICluster _cluster;
        private int _disposed;
        private readonly IMetricsManager _metricsManager;

        internal IInternalSession InternalRef
        {
            get { throw new NotImplementedException(); }
            set { }
        }

        public int BinaryProtocolVersion => 4;

        /// <inheritdoc />
        public ICluster Cluster => _cluster;

        /// <summary>
        /// Gets the cluster configuration
        /// </summary>
        public Configuration Configuration { get; protected set; }

        /// <summary>
        /// Determines if the session is already disposed
        /// </summary>
        public bool IsDisposed => Volatile.Read(ref _disposed) > 0;

        /// <summary>
        /// Gets or sets the keyspace
        /// </summary>
        private string _keyspace;
        public string Keyspace
        {
            get => _keyspace;
            private set => _keyspace = value;
        }

        /// <inheritdoc />
        public UdtMappingDefinitions UserDefinedTypes { get; private set; }

        public string SessionName { get; }

        public Policies Policies => Configuration.Policies;

        internal Session(
            ICluster cluster,
            Configuration configuration,
            string keyspace)
        {
            _cluster = cluster;
            Configuration = configuration;
            Keyspace = keyspace;
            // FIXME:
            _metricsManager = null;
            // _metricsManager = new MetricsManager(configuration.MetricsProvider, Configuration.MetricsOptions, Configuration.MetricsEnabled, SessionName);
        }

        /// <inheritdoc />
        public IAsyncResult BeginExecute(IStatement statement, AsyncCallback callback, object state)
        {
            return ExecuteAsync(statement).ToApm(callback, state);
        }

        /// <inheritdoc />
        public IAsyncResult BeginExecute(string cqlQuery, ConsistencyLevel consistency, AsyncCallback callback, object state)
        {
            return BeginExecute(new SimpleStatement(cqlQuery).SetConsistencyLevel(consistency), callback, state);
        }

        /// <inheritdoc />
        public IAsyncResult BeginPrepare(string cqlQuery, AsyncCallback callback, object state)
        {
            return PrepareAsync(cqlQuery).ToApm(callback, state);
        }

        /// <inheritdoc />
        public void ChangeKeyspace(string keyspace)
        {
            if (Keyspace != keyspace)
            {
                // FIXME: Migrate to Rust `Session::use_keyspace()`.

                Execute(new SimpleStatement(CqlQueryTools.GetUseKeyspaceCql(keyspace)));
                Keyspace = keyspace;
            }
        }

        /// <inheritdoc />
        public void CreateKeyspace(string keyspace, Dictionary<string, string> replication = null, bool durableWrites = true)
        {
            WaitForSchemaAgreement(Execute(CqlQueryTools.GetCreateKeyspaceCql(keyspace, replication, durableWrites, false)));
            Session.Logger.Info("Keyspace [" + keyspace + "] has been successfully CREATED.");
        }

        /// <inheritdoc />
        public void CreateKeyspaceIfNotExists(string keyspaceName, Dictionary<string, string> replication = null, bool durableWrites = true)
        {
            // Note: This won't work with current Rust error handling, because we always throw RustException on any error,
            // losing capability to catch specific exceptions like AlreadyExistsException.
            // FIXME: Design a better error handling mechanism to allow this.
            try
            {
                CreateKeyspace(keyspaceName, replication, durableWrites);
            }
            catch (AlreadyExistsException)
            {
                Session.Logger.Info(string.Format("Cannot CREATE keyspace:  {0}  because it already exists.", keyspaceName));
            }
        }

        /// <inheritdoc />
        public void DeleteKeyspace(string keyspaceName)
        {
            Execute(CqlQueryTools.GetDropKeyspaceCql(keyspaceName, false));
        }

        /// <inheritdoc />
        public void DeleteKeyspaceIfExists(string keyspaceName)
        {
            // Note: This won't work with current Rust error handling, because we always throw RustException on any error,
            // losing capability to catch specific exceptions like AlreadyExistsException.
            // FIXME: Design a better error handling mechanism to allow this.
            try
            {
                DeleteKeyspace(keyspaceName);
            }
            catch (InvalidQueryException)
            {
                Session.Logger.Info(string.Format("Cannot DELETE keyspace:  {0}  because it not exists.", keyspaceName));
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            ShutdownAsync().GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public Task ShutdownAsync()
        {
            // FIXME: Actually perform shutdown.
            // Remember to dequeue from Cluster's sessions list.
            return Task.FromResult<object>(null);
        }

        /// <inheritdoc />
        public RowSet EndExecute(IAsyncResult ar)
        {
            var task = (Task<RowSet>)ar;
            TaskHelper.WaitToCompleteWithMetrics(_metricsManager, task, Configuration.DefaultRequestOptions.QueryAbortTimeout);
            return task.Result;
        }

        /// <inheritdoc />
        public PreparedStatement EndPrepare(IAsyncResult ar)
        {
            var task = (Task<PreparedStatement>)ar;
            TaskHelper.WaitToCompleteWithMetrics(_metricsManager, task, Configuration.DefaultRequestOptions.QueryAbortTimeout);
            return task.Result;
        }

        /// <inheritdoc />
        public RowSet Execute(IStatement statement, string executionProfileName)
        {
            var task = ExecuteAsync(statement, executionProfileName);
            TaskHelper.WaitToCompleteWithMetrics(_metricsManager, task, Configuration.DefaultRequestOptions.QueryAbortTimeout);
            return task.Result;
        }

        /// <inheritdoc />
        public RowSet Execute(IStatement statement)
        {
            return Execute(statement, Configuration.DefaultExecutionProfileName);
        }

        /// <inheritdoc />
        public RowSet Execute(string cqlQuery)
        {
            return Execute(GetDefaultStatement(cqlQuery));
        }

        /// <inheritdoc />
        public RowSet Execute(string cqlQuery, string executionProfileName)
        {
            return Execute(GetDefaultStatement(cqlQuery), executionProfileName);
        }

        /// <inheritdoc />
        public RowSet Execute(string cqlQuery, ConsistencyLevel consistency)
        {
            return Execute(GetDefaultStatement(cqlQuery).SetConsistencyLevel(consistency));
        }

        /// <inheritdoc />
        public RowSet Execute(string cqlQuery, int pageSize)
        {
            return Execute(GetDefaultStatement(cqlQuery).SetPageSize(pageSize));
        }

        /// <inheritdoc />
        public Task<RowSet> ExecuteAsync(IStatement statement)
        {
            return ExecuteAsync(statement, Configuration.DefaultExecutionProfileName);
        }

        /// <inheritdoc />
        public Task<RowSet> ExecuteAsync(IStatement statement, string executionProfileName)
        {
            // return this.ExecuteAsync(statement, this.GetRequestOptions(executionProfileName));
            throw new NotImplementedException("ExecuteAsync is not yet implemented"); // FIXME: bridge with Rust execution profiles.
        }
        public IDriverMetrics GetMetrics()
        {
            return _metricsManager;
        }

        /// <inheritdoc />
        public PreparedStatement Prepare(string cqlQuery)
        {
            return Prepare(cqlQuery, null, null);
        }

        /// <inheritdoc />
        public PreparedStatement Prepare(string cqlQuery, IDictionary<string, byte[]> customPayload)
        {
            // TODO: support custom payload in Rust Driver, then implement this.
            return Prepare(cqlQuery, null, customPayload);
        }

        /// <inheritdoc />
        public PreparedStatement Prepare(string cqlQuery, string keyspace)
        {
            return Prepare(cqlQuery, keyspace, null);
        }

        /// <inheritdoc />
        public PreparedStatement Prepare(string cqlQuery, string keyspace, IDictionary<string, byte[]> customPayload)
        {
            var task = PrepareAsync(cqlQuery, keyspace, customPayload);
            TaskHelper.WaitToCompleteWithMetrics(_metricsManager, task, Configuration.ClientOptions.QueryAbortTimeout);
            return task.Result;
        }

        /// <inheritdoc />
        public Task<PreparedStatement> PrepareAsync(string query)
        {
            return PrepareAsync(query, null, null);
        }

        /// <inheritdoc />
        public Task<PreparedStatement> PrepareAsync(string query, IDictionary<string, byte[]> customPayload)
        {
            // TODO: support custom payload in Rust Driver, then implement this.
            return PrepareAsync(query, null, customPayload);
        }

        /// <inheritdoc />
        public Task<PreparedStatement> PrepareAsync(string cqlQuery, string keyspace)
        {
            return PrepareAsync(cqlQuery, keyspace, null);
        }

        /// <inheritdoc />
        public Task<PreparedStatement> PrepareAsync(
            string cqlQuery, string keyspace, IDictionary<string, byte[]> customPayload)
        {
            // TODO: support custom payload in Rust Driver, then implement this.
            if (keyspace != null)
            {
                // Validate protocol version here and not at PrepareRequest level, as PrepareRequest can be issued
                // in the background (prepare and retry, prepare on up, ...)
                throw new NotSupportedException($"Protocol version 4 does not support" +
                                                " setting the keyspace as part of the PREPARE request");
            }
            throw new NotImplementedException("PrepareAsync is not yet implemented"); // FIXME: bridge with Rust prepare.
        }

        public void WaitForSchemaAgreement(RowSet rs)
        {
            // Deprecated and implemented as no-op.
        }

        public bool WaitForSchemaAgreement(IPEndPoint hostAddress)
        {
            // Deprecated and implemented as no-op.
            return false;
        }

        private IStatement GetDefaultStatement(string cqlQuery)
        {
            return new SimpleStatement(cqlQuery);
        }

        private IRequestOptions GetRequestOptions(string executionProfileName)
        {
            // FIXME: bridge with Rust execution profiles.
            if (!Configuration.RequestOptions.TryGetValue(executionProfileName, out var profile))
            {
                throw new ArgumentException("The provided execution profile name does not exist. It must be added through the Cluster Builder.");
            }

            return profile;
        }

        /* BEGIN TESTING-ONLY STUBS */
        /* FIXME: Reimplement tests and remove these stubs */

        /// <summary>
        /// Unique Session ID generated by the driver.
        /// </summary>
        Guid IInternalSession.InternalSessionId { get; }

        /// <summary>
        /// Initialize the session
        /// </summary>
        Task IInternalSession.Init() {
            throw new NotImplementedException("STUB: reimplement tests and remove me");
        }

        /// <summary>
        /// Gets or creates the connection pool for a given host
        /// </summary>
        IHostConnectionPool IInternalSession.GetOrCreateConnectionPool(Host host, HostDistance distance) {
            throw new NotImplementedException("STUB: reimplement tests and remove me");
        }

        /// <summary>
        /// Gets a snapshot of the connection pools
        /// </summary>
        IEnumerable<KeyValuePair<IPEndPoint, IHostConnectionPool>> IInternalSession.GetPools() {
            throw new NotImplementedException("STUB: reimplement tests and remove me");
        }

        /// <summary>
        /// Gets the existing connection pool for this host and session or null when it does not exists
        /// </summary>
        IHostConnectionPool IInternalSession.GetExistingPool(IPEndPoint address) {
            throw new NotImplementedException("STUB: reimplement tests and remove me");
        }

        void IInternalSession.CheckHealth(Host host, IConnection connection) {
            throw new NotImplementedException("STUB: reimplement tests and remove me");
        }

        bool IInternalSession.HasConnections(Host host) {
            throw new NotImplementedException("STUB: reimplement tests and remove me");
        }

        void IInternalSession.OnAllConnectionClosed(Host host, IHostConnectionPool pool) {
            throw new NotImplementedException("STUB: reimplement tests and remove me");
        }

        /// <summary>
        /// Gets or sets the keyspace
        /// </summary>
        string IInternalSession.Keyspace { get; set; }

        IInternalCluster IInternalSession.InternalCluster { get; }

        /// <summary>
        /// Fetches the request options that were mapped from the provided execution profile's name.
        /// </summary>
        IRequestOptions IInternalSession.GetRequestOptions(string executionProfileName) {
            throw new NotImplementedException("STUB: reimplement tests and remove me");
        }

        /// <summary>
        /// Returns the number of connected nodes.
        /// </summary>
        int IInternalSession.ConnectedNodes { get; }

        IMetricsManager IInternalSession.MetricsManager { get; }

        IObserverFactory IInternalSession.ObserverFactory { get; }

        Task<RowSet> IInternalSession.ExecuteAsync(IStatement statement, IRequestOptions requestOptions) {
            throw new NotImplementedException("STUB: reimplement tests and remove me");
        }

        /* END TESTING-ONLY STUBS */
    }
}