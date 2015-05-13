//-----------------------------------------------------------------------
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
// <project>Schiffeversenken Pirat Edition</project>
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
/// Maintains the position of all ships and their state
/// </summary>
public class Ships
    {
        // Boat --> 2 Pixel
        public struct boat
        {
            // Das boot besteht aus 2 Teilen
            public bool Front;
            public bool Heck;
            public bool shipDestryoed;
            public int posFrontX;
            public int posFrontY;
            public int posHeckX;
            public int posHeckY;
            public string name;
            public bool horizontal;
        }

        // Cruiser --> 3 Pixel
        public struct cruiser
        {
            public bool Front;
            public bool Heck;
            public bool middle;
            public bool shipDestryoed;
            public int posHeckX;
            public int posHeckY;
            public int posMiddleX;
            public int posMiddleY;
            public int posFrontX;
            public int posFrontY;
            public string name;
            public bool horizontal;
        }

        /// <summary>
        /// Galley --> 4 Pixel
        /// </summary>
        public struct galley
        {
            public bool Front;
            public bool Heck;
            public bool middle1;
            public bool middle2;
            public bool shipDestryoed;
            public int posHeckX;
            public int posHeckY;
            public int posMiddle1X;
            public int posMiddle1Y;
            public int posMiddle2X;
            public int posMiddle2Y;
            public int posFrontX;
            public int posFrontY;
            public string name;
            public bool horizontal;
        }

        /// <summary>
        /// Galley --> 5 Pixel
        /// </summary>
        public struct battleship
        {
            public bool Front;
            public bool Heck;
            public bool middle1;
            public bool middle2;
            public bool shipDestryoed;
            public int posHeckX;
            public int posHeckY;
            public int posMiddle1X;
            public int posMiddle1Y;
            public int posMiddle2X;
            public int posMiddle2Y;
            public int posFrontX;
            public int posFrontY;
            public string name;
            public bool horizontal;
        }
    }
}
