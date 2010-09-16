/*
 * Projekt: Schiffeversenken Pirat Edition
 * Klasse: SocketPacket
 * Beschreibung: Stellt jeweils ein Packet dar welches über den Socket verschickt wird
 * Autor: Markus Bohnert
 * Team: Simon Hodler, Markus Bohnert
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Battleships
{
    class SocketPacket
    {
        public System.Net.Sockets.Socket m_currentSocket;
        // 10 Bytes aus dem Socket lesen
        public byte[] dataBuffer = new byte[10];
    }
}
