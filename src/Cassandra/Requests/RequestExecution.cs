//
//       Copyright DataStax, Inc.
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
using System.Net.Sockets;
using System.Threading.Tasks;
using Cassandra.Connections;
using Cassandra.Observers.Abstractions;
using Cassandra.Responses;
using Cassandra.SessionManagement;
using Cassandra.Tasks;

namespace Cassandra.Requests
{
    internal class RequestExecution : IRequestExecution
    {
        public RequestExecution(IRequestHandler parent, IInternalSession session, IRequest request, IRequestObserver requestObserver, SessionRequestInfo sessionRequestInfo)
        {
        }

        public void Cancel()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Host Start(bool currentHostRetry)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a new connection to the current host and send the request with it. Useful for retries on the same host.
        /// </summary>
        private Task SendToCurrentHostAsync()
        {
            throw new NotImplementedException();
        }

        internal static RequestErrorType GetErrorType(IRequestError error)
        {
            if (error.Exception is OperationTimedOutException)
            {
                return RequestErrorType.ClientTimeout;
            }

            if (error.Unsent)
            {
                return RequestErrorType.Unsent;
            }

            return error.IsServerError ? RequestErrorType.Other : RequestErrorType.Aborted;
        }
    }
}
