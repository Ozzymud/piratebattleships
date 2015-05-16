//-----------------------------------------------------------------------
// <copyright file="SocketPacket.cs" company="Team 17">
// Copyright 2005 Team 17
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// <project>Battleships Pirate Edition</project>
// <author>Markus Bohnert</author>
// <team>Simon Hodler, Markus Bohnert</team>
//-----------------------------------------------------------------------

namespace Battleships
{
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Read 10 bytes from a network socket.
/// </summary>
internal class SocketPacket
    {
    /// <summary>
    /// The current connection between host and client.
    /// </summary>
    private System.Net.Sockets.Socket currentSocket;

    /// <summary>
    /// Gets or sets to SocketPacket.currentSocket (public access).
    /// </summary>
    public System.Net.Sockets.Socket CurrentSocket
    {
        get { return this.currentSocket; }
        set { this.currentSocket = value; }
    }

    /// <summary>
    /// A packet of 10 bytes sent between host and client.
    /// </summary>
    private byte[] dataBuffer = new byte[10];

    /// <summary>
    /// Gets or sets data in the SocketPacket data buffer.
    /// </summary>
    public byte[] DataBuffer
    {
        get { return this.dataBuffer; }
        set { this.dataBuffer = value; }
    }
}
}