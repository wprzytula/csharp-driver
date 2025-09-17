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
using System.Net;
using System.Threading.Tasks;

using Cassandra.Connections;
using Cassandra.ExecutionProfiles;

namespace Cassandra.SessionManagement
{
    /// <inheritdoc />
    /// <remarks>This is an internal interface designed to declare the internal methods that are called
    /// across multiple locations of the driver's source code.</remarks>
    internal interface IInternalSession : ISession
    {
    }
}