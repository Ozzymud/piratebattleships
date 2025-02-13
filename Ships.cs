﻿//-----------------------------------------------------------------------
// <copyright file="Ships.cs" company="Team 17">
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
#region directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#endregion

/// <summary>
/// Maintains the position of all ships and their state.
/// </summary>
public static class Ships
    {
    #region struct
    /// <summary>
    /// Boat --> 2 parts.
    /// </summary>
    public struct Boat
        {
            public bool Front;
            public bool Rear;
            public bool ShipDestroyed;
            public int PosFrontX;
            public int PosFrontY;
            public int PosRearX;
            public int PosRearY;
            public string ShipName;
            public bool IsHorizontal;
        }

    /// <summary>
    /// Cruiser --> 3 parts.
    /// </summary>
    public struct Cruiser
        {
            public bool Front;
            public bool Middle;
            public bool Rear;
            public bool ShipDestroyed;
            public int PosFrontX;
            public int PosFrontY;
            public int PosMiddleX;
            public int PosMiddleY;
            public int PosRearX;
            public int PosRearY;
            public string ShipName;
            public bool IsHorizontal;
        }

    /// <summary>
    /// Galley --> 4 parts.
    /// </summary>
    public struct Galley
        {
            public bool Front;
            public bool MiddleFirstPart;
            public bool MiddleSecondPart;
            public bool Rear;
            public bool ShipDestroyed;
            public int PosFrontX;
            public int PosFrontY;
            public int PosMiddleFirstX;
            public int PosMiddleFirstY;
            public int PosMiddleSecondX;
            public int PosMiddleSecondY;
            public int PosRearX;
            public int PosRearY;
            public string ShipName;
            public bool IsHorizontal;
        }

    /// <summary>
    /// Battleship --> 5 parts.
    /// TODO: actually make this 5 parts.
    /// </summary>
    public struct Battleship
        {
            public bool Front;
            public bool MiddleFirstPart;
            public bool MiddleSecondPart;
            public bool Rear;
            public bool ShipDestroyed;
            public int PosFrontX;
            public int PosFrontY;
            public int PosMiddleFirstX;
            public int PosMiddleFirstY;
            public int PosMiddleSecondX;
            public int PosMiddleSecondY;
            public int PosRearX;
            public int PosRearY;
            public string ShipName;
            public bool IsHorizontal;
        }
    #endregion
    }
}
