//-----------------------------------------------------------------------
// <copyright file="ClientGameForm.cs" company="Team 17">
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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

public partial class ClientGameForm : DoubleBuffered.FormDoubleBuffered
    {
        public byte[] m_dataBuffer = new byte[10];
        public IAsyncResult m_result;
        public AsyncCallback m_pfnCallBack;
        public Socket m_clientSocket;
        private int roll;
        private string oroll;

        delegate void SetRichTextBoxRxCallback(string text);

        delegate void SetTextMainFormCallback(string text);

        delegate void UpdateControlsCallback(bool listening);

        public ClientGameForm()
        {
            this.InitializeComponent();
            this.textboxIP.Text = this.GetIP();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            // See if we have text on the IP and Port text fields
            if (this.textboxIP.Text == string.Empty || this.textboxPort.Text == string.Empty)
            {
                MessageBox.Show("IP Address and Port Number are required to connect to the Server");
                return;
            }

            try
            {
                this.UpdateControls(false);
                this.m_clientSocket = new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp); // Create the socket instance
                IPAddress ip = IPAddress.Parse(this.textboxIP.Text); // Cet the remote IP address
                int iPortNo = System.Convert.ToInt16(this.textboxPort.Text);
                IPEndPoint ipEnd = new IPEndPoint(ip, iPortNo); // Create the end point
                this.m_clientSocket.Connect(ipEnd); // Connect to the remote host
                if (this.m_clientSocket.Connected)
                {
                    this.listboxRx.Items.Add("Connected to Server...");
                    this.UpdateControls(true);

                    // Wait for data asynchronously
                    this.WaitForData();
                }
            }
            catch (SocketException se)
            {
                string str;
                str = "\nConnection failed, is the server running?\n" + se.Message;
                this.SetText(str);

                // MessageBox.Show(str);
                this.UpdateControls(false);
            }
        }

        public void WaitForData()
        {
            try
            {
                if (this.m_pfnCallBack == null)
                {
                    this.m_pfnCallBack = new AsyncCallback(this.OnDataReceived);
                }

                SocketPacket theSocPkt = new SocketPacket();
                theSocPkt.m_currentSocket = this.m_clientSocket;

                // Start listening to the data asynchronously
                this.m_result = this.m_clientSocket.BeginReceive(theSocPkt.dataBuffer, 0, theSocPkt.dataBuffer.Length, SocketFlags.None, this.m_pfnCallBack, theSocPkt);
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode == SocketError.ConnectionReset)
                {
                    this.SetText("Server closed!\n");
                    this.SetTextLblStatus("Server closed!");
                    this.m_clientSocket.Close();
                    this.UpdateControls(false);
                }
            }
            catch (Exception ex)
            {
                this.SetText("WaitForData: Socket has been closed. Socket error: " + ex.Message.ToString());
                this.CloseSocket();
                this.UpdateControls(false);
            }
        }

        public void OnDataReceived(IAsyncResult asyn)
        {
            try
            {
                SocketPacket theSockId = (SocketPacket)asyn.AsyncState;
                int iRx = theSockId.m_currentSocket.EndReceive(asyn);
                char[] chars = new char[iRx + 1];
                Decoder d = Encoding.UTF8.GetDecoder();
                int charLen = d.GetChars(theSockId.dataBuffer, 0, iRx, chars, 0);
                string data = new string(chars);

                // Ankommende Daten verarbeiten und Service auswählen
                if (this.Services(data))
                {
                    // Auf ankommende Daten warten
                    this.WaitForData();
                }
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode == SocketError.ConnectionReset)
                {
                    this.SetText("Server closed!\n");
                    this.SetTextLblStatus("Server closed!");
                    this.UpdateControls(false);
                }

                // MessageBox.Show(se.Message);
            }
            catch (Exception ex)
            {
                this.SetText("OnDataReceived: Socket has been closed Socket error: " + ex.Message.ToString());
                this.CloseSocket();
            }
        }

        /// <summary>
        /// Wertet die Nachricht vom Server aus und entscheidet welche entsprechende Operation ausgeführt werden soll
        /// </summary>
        /// <param name="data">Nachricht vom Server</param>
        /// <returns>False wenn Verbindung geschlossen werden soll (z.B. bei vollem Server)</returns>
        private bool Services(string data)
        {
            // Server hat Verbindung bestätigt
            if (data.StartsWith("ACK"))
            {
                this.SetTextLblStatus("Server hat Verbindung bestätigt (ACK)");
            }
            else if (data.StartsWith("FULL"))
            {
            // Server ist voll!
                this.SetText("Server is full...closing connection!");
                if (this.m_clientSocket != null)
                {
                    this.m_clientSocket.Close();
                    this.m_clientSocket = null;
                    this.UpdateControls(false);
                    this.SetText("Disconnected from Server!");
                }

                return false;
            }
            else if (data.Contains("pb_"))
            {
            // Koordinaten vom Gegner erhalten (Auswerten ob an den Koords ein Schiff gesetzt ist oder nicht)
                // Erhaltene koordinaten ausgeben
                this.SetText("Coords received: " + data);

                // X und Y aus der Nachricht lesen
                string pos = data.Remove(0, 3);
                string[] posData = pos.Split(':');
                int x = int.Parse(posData[0]);
                int y = int.Parse(posData[1]);

                object objData;

                // Auswerten ob der Gegner getroffen hat oder nicht
                if (!BattleshipsForm.battlefieldPlayer.hitOrMiss(x, y))
                {
                    // Gegner mitteilen, dass er nicht getroffen hat
                    objData = "MISS" + x.ToString() + ":" + y.ToString();
                    BattleshipsForm.battlefieldPlayer.setMiss(x, y);
                }
                else
                {
                    // Gegner mitteilen, dass er getroffen hat
                    objData = "HIT" + x.ToString() + ":" + y.ToString();

                    // Einschlag darstellen (Auf eigenem Feld --> Gegner hat getroffen)
                    if (BattleshipsForm.battlefieldPlayer.setImpact(x, y))
                    {
                        // Gegner hat gewonnen (Alle Schiffe wurden zerstört)
                        objData = "WIN";
                    }
                }

                byte[] byData = System.Text.Encoding.ASCII.GetBytes(objData.ToString());

                // Antwort an Gegner schicken
                if (this.m_clientSocket.Connected)
                {
                    this.m_clientSocket.Send(byData);
                }

                // Wenn Gegner gewonnen hat, dann darf Spieler nicht mehr zum Zug kommen...
                if (objData.ToString().StartsWith("WIN"))
                {
                    // ToDo: Sound abspielen (Losersound...)
                    MessageBox.Show(this, "Loser!", "lose", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    Application.Exit();
                }
                else
                {
                    // Spieler ist an der Reihe
                    BattleshipsForm.whosTurn = BattleshipsForm.spielzug.player;
                    this.SetTextLblStatus("Du bist am Zug!");
                }
            }
            else if (data.StartsWith("WIN"))
            {
            // Du hast gewonnen!!
                // ToDo: Sound abspielen (Gewinnersound....)
                MessageBox.Show(this, "Du hast gewonnen!", "Sieg!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Application.Exit();
            }
            else if (data.StartsWith("HIT"))
            {
            // Du hast einen treffer gelandet!
                string pos = data.Remove(0, 3);
                string[] posData = pos.Split(':');
                int x = int.Parse(posData[0]);
                int y = int.Parse(posData[1]);
                this.SetText("HIT received at x:" + x.ToString() + " y:" + y.ToString());

                // Dem Spieler Anzeigen, dass er getroffen hat (Auf dem Gegnerfeld)
                BattleshipsForm.battlefieldOpponent.setImpact(x, y);

                // Gegner ist an der Reihe
                BattleshipsForm.whosTurn = BattleshipsForm.spielzug.opponend;
                this.SetTextLblStatus("Gegner ist am Zug!");
            }
            else if (data.StartsWith("MISS"))
            {
            // Der Schuss ging leider daneben, versuchs nochmal!
                string pos = data.Remove(0, 4);
                string[] posData = pos.Split(':');
                int x = int.Parse(posData[0]);
                int y = int.Parse(posData[1]);
                this.SetText("MISS received at x:" + x.ToString() + " y:" + y.ToString());

                // Dem Spieler anzeigen, dass er nicht getroffen hat (Auf dem Gegnerfeld)
                BattleshipsForm.battlefieldOpponent.setMiss(x, y);

                // Gegner ist an der Reihe
                BattleshipsForm.whosTurn = BattleshipsForm.spielzug.opponend;
                this.SetTextLblStatus("Gegner ist am Zug!");
            }
            else if (data.StartsWith("RDY_"))
            {
                this.oroll = data.Remove(0, 4);
                BattleshipsForm.opponendReady2Play = true;
                this.SetTextLblStatus("Opponend is ready and rolled: " + this.oroll);

                // Prüfen ob Spieler auch bereit ist?
                if (BattleshipsForm.playerReady2Play)
                {
                    if (int.Parse(this.oroll) < this.roll)
                    {
                        this.SetTextLblStatus("Opponend rolled: " + this.oroll + " you rolled: " + this.roll.ToString());
                        this.SetTextLblStatus("Du darfst anfangen");
                        BattleshipsForm.whosTurn = BattleshipsForm.spielzug.player;
                    }
                    else if (int.Parse(this.oroll) > this.roll)
                    {
                        this.SetTextLblStatus("Opponend rolled: " + this.oroll + " you rolled: " + this.roll.ToString());
                        this.SetTextLblStatus("Gegner darf anfangen");
                        BattleshipsForm.whosTurn = BattleshipsForm.spielzug.opponend;
                    }
                }
                else
                {
                // Wenn nicht, dann abwarten bis Spieler bereit ist
                    // Warten bis Spieler bereit ist (Spieler ist bereit, wenn er auf den Bereit-Button gedrückt hat - Siehe Event btnRdy.clicked)
                }
            }
            else
            {
            // Sonstige Meldungen
                this.SetText(data);
            }

            return true;
        }

        private void BtnRdy_Click(object sender, EventArgs e)
        {
            // Überprüfen ob alle Schiffe verteilt wurden
            if (BattleshipsForm.zaehler_battleship >= 1 && BattleshipsForm.zaehler_galley >= 1 && BattleshipsForm.zaehler_cruiser >= 3 && BattleshipsForm.zaehler_boat >= 3)
            {
                if (this.m_clientSocket != null)
                {
                    if (this.m_clientSocket.Connected)
                    {
                        BattleshipsForm.playerReady2Play = true;
                        this.btnRdy.Enabled = false;

                        Random rnd = new Random();
                        this.roll = rnd.Next(101);
                        object objData = "RDY_" + this.roll.ToString();
                        byte[] byData = System.Text.Encoding.ASCII.GetBytes(objData.ToString());

                        // Antwort an Gegner schicken
                        if (this.m_clientSocket.Connected)
                        {
                            this.m_clientSocket.Send(byData);
                        }

                        this.SetTextLblStatus("You rolled: " + this.roll.ToString());

                        if (BattleshipsForm.opponendReady2Play)
                        {
                            if (int.Parse(this.oroll) < this.roll)
                            {
                                this.SetTextLblStatus("Opponend rolled: " + this.oroll + " you rolled: " + this.roll.ToString());
                                this.SetTextLblStatus("Du darfst anfangen");
                                BattleshipsForm.whosTurn = BattleshipsForm.spielzug.player;
                            }
                            else if (int.Parse(this.oroll) > this.roll)
                            {
                                this.SetTextLblStatus("Opponend rolled: " + this.oroll + " you rolled: " + this.roll.ToString());
                                this.SetTextLblStatus("Gegner darf anfangen");
                                BattleshipsForm.whosTurn = BattleshipsForm.spielzug.opponend;
                            }
                        }
                        else
                        {
                            // Warten bis Gegner bereit ist (Gegner ist Bereit wenn "RDY"-Flag ankommt)
                        }
                    }
                    else
                    {
                        MessageBox.Show(this, "Es muss erst ein Gegenspieler verbunden sein!", "Gegenspieler benötigt", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show(this, "Es muss erst ein Gegenspieler verbunden sein!", "Gegenspieler benötigt", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show(this, "Es müssen erst alle Schiffe verteilt werden!", "Nocht nicht bereit!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Ermittelt die interne IP-Adresse des PCs
        /// </summary>
        /// <returns>interne IP-Adresse (String)</returns>
        private string GetIP()
        {
            string strHostName = Dns.GetHostName();

            // Find host by name
            IPHostEntry iphostentry = Dns.GetHostEntry(strHostName);
            //// Dns.GetHostByName(strHostName);

            // Grab the first IP addresses
            string IPStr = string.Empty;
            foreach (IPAddress ipaddress in iphostentry.AddressList)
            {
                // Die erste IPV4 Adresse aus der Adressliste wählen
                if (ipaddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    IPStr = ipaddress.ToString();
                    return IPStr;
                }  
            }

            return IPStr;
        }

        public void SetText(string text)
        {
            if (this.listboxRx.InvokeRequired)
            {
                SetRichTextBoxRxCallback d = new SetRichTextBoxRxCallback(this.SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.listboxRx.Items.Add(text);
                this.listboxRx.SelectedIndex = this.listboxRx.Items.Count - 1;
            }
        }

        public void SetTextLblStatus(string text)
        {
            if (BattleshipsForm.lblStatus.InvokeRequired)
            {
                SetTextMainFormCallback d = new SetTextMainFormCallback(this.SetTextLblStatus);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                BattleshipsForm.lblStatus.Text += text;
                BattleshipsForm.lblStatus.Text += "\n";
                BattleshipsForm.panelStatus.VerticalScroll.Value += BattleshipsForm.panelStatus.VerticalScroll.SmallChange;
                BattleshipsForm.panelStatus.Refresh();
            }
        }

        private void UpdateControls(bool status)
        {
            if (this.btnConnect.InvokeRequired)
            {
                UpdateControlsCallback d = new UpdateControlsCallback(this.UpdateControls);
                this.Invoke(d, new object[] { status });
            }
            else
            {
                this.btnConnect.Enabled = !status;
                this.btnDisconnect.Enabled = status;
                this.btnRdy.Enabled = status;
            }
        }

        private void CloseSocket()
        {
            if (this.m_clientSocket != null)
            {
                this.m_clientSocket.Close();
                this.m_clientSocket = null;
            }

            BattleshipsForm.playerReady2Play = false;
            BattleshipsForm.opponendReady2Play = false;
            this.SetText("Server closed!");
            this.SetTextLblStatus("Server closed!");
            this.UpdateControls(false);
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            this.CloseSocket();         
        }

        private void ClientGameForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.CloseSocket();
            BattleshipsForm.spielHostenMenuItem.Enabled = true;
        }
    }
}
