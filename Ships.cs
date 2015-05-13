using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Battleships
{
    /// <summary>
    /// Verwaltet die Position aller Schiffe sowie deren Zustand
    /// </summary>
    public class Ships
    {
        /// <summary>
        /// Boot --> 2 Pixel
        /// </summary>
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

        /// <summary>
        /// Cruiser --> 3 Pixel
        /// </summary>
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
