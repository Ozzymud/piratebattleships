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

    public partial class ClientGameForm : Battleships.DoubleBufferedForm
    {
        public Socket MainClientSocket;
        private byte[] mainDataBuffer = new byte[10];
        private IAsyncResult mainResult;
        private AsyncCallback pfnCallBack;
        private int roll;
        private string oroll;

        private delegate void SetRichTextBoxRxCallback(string text);

        private delegate void SetTextMainFormCallback(string text);

        private delegate void UpdateControlsCallback(bool listening);

        public ClientGameForm()
        {
            this.InitializeComponent();
            this.textboxIP.Text = this.GetIP();
        }

        private void ButtonConnectClick(object sender, EventArgs e)
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

                // Create the socket instance
                this.MainClientSocket = new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp);

                // Cet the remote IP address
                IPAddress ip = IPAddress.Parse(this.textboxIP.Text);
                int internetPortNumber = System.Convert.ToInt16(this.textboxPort.Text);

                // Create the end point
                IPEndPoint endPoint = new IPEndPoint(ip, internetPortNumber);

                // Connect to the remote host
                this.MainClientSocket.Connect(endPoint);
                if (this.MainClientSocket.Connected)
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

                ////MessageBox.Show(str);
                this.UpdateControls(false);
            }
        }

        public void WaitForData()
        {
            try
            {
                if (this.pfnCallBack == null)
                {
                    this.pfnCallBack = new AsyncCallback(this.OnDataReceived);
                }

                SocketPacket theSocPkt = new SocketPacket();
                theSocPkt.MainCurrentSocket = this.MainClientSocket;

                // Start listening to the data asynchronously
                this.mainResult = this.MainClientSocket.BeginReceive(
                    theSocPkt.DataBuffer,
                    0,
                    theSocPkt.DataBuffer.Length,
                    SocketFlags.None,
                    this.pfnCallBack,
                    theSocPkt);
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode == SocketError.ConnectionReset)
                {
                    this.SetText("Server closed!\n");
                    this.SetTextLblStatus("Server closed!");
                    this.MainClientSocket.Close();
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
                int iRx = theSockId.MainCurrentSocket.EndReceive(asyn);
                char[] chars = new char[iRx + 1];
                Decoder d = Encoding.UTF8.GetDecoder();
                int charLen = d.GetChars(theSockId.DataBuffer, 0, iRx, chars, 0);
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
                ////MessageBox.Show(se.Message);
            }
            catch (Exception ex)
            {
                this.SetText("OnDataReceived: Socket has been closed Socket error: " + ex.Message.ToString());
                this.CloseSocket();
            }
        }

        /// <summary>
        /// The message from the server.
        /// evaluates and decides what appropriate operation should be carried out.
        /// </summary>
        /// <param name="data">Data received from the server.</param>
        /// <returns>False if the connection should be closed (e.g. if the server is full).</returns>
        private bool Services(string data)
        {
            // Server has confirmed connection
            // else if: Server is full, close connection
            // else if: get coordinates (pb_) from an opponent (evaluate whether a ship is set to the coords or not) 
            if (data.StartsWith("ACK"))
            {
                this.SetTextLblStatus("Server hat Verbindung bestätigt (ACK)");
            }
            else if (data.StartsWith("FULL"))
            {
                this.SetText("Server is full...closing connection!");
                if (this.MainClientSocket != null)
                {
                    this.MainClientSocket.Close();
                    this.MainClientSocket = null;
                    this.UpdateControls(false);
                    this.SetText("Disconnected from Server!");
                }

                return false;
            }
            else if (data.Contains("pb_"))
            {
                // Erhaltene koordinaten ausgeben
                this.SetText("Coords received: " + data);

                // X und Y aus der Nachricht lesen
                string pos = data.Remove(0, 3);
                string[] posData = pos.Split(':');
                int x = int.Parse(posData[0]);
                int y = int.Parse(posData[1]);

                object objData;

                // Auswerten ob der Gegner getroffen hat oder nicht
                if (!BattleshipsForm.BattlefieldPlayer.HitOrMiss(x, y))
                {
                    // Gegner mitteilen, dass er nicht getroffen hat
                    objData = "MISS" + x.ToString() + ":" + y.ToString();
                    BattleshipsForm.BattlefieldPlayer.SetMiss(x, y);
                }
                else
                {
                    // Gegner mitteilen, dass er getroffen hat
                    objData = "HIT" + x.ToString() + ":" + y.ToString();

                    // Einschlag darstellen (Auf eigenem Feld --> Gegner hat getroffen)
                    if (BattleshipsForm.BattlefieldPlayer.SetImpact(x, y))
                    {
                        // Gegner hat gewonnen (Alle Schiffe wurden zerstört)
                        objData = "WIN";
                    }
                }

                byte[] byteData = System.Text.Encoding.ASCII.GetBytes(objData.ToString());

                // Antwort an Gegner schicken
                if (this.MainClientSocket.Connected)
                {
                    this.MainClientSocket.Send(byteData);
                }

                // If opponent has won, then players may no longer have a turn...
                if (objData.ToString().StartsWith("WIN"))
                {
                    // ToDo: Sound abspielen (Losersound...)
                    MessageBox.Show(this, "Loser!", "lose", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    Application.Exit();
                }
                else
                {
                    // Spieler ist an der Reihe
                    BattleshipsForm.WhosTurn = BattleshipsForm.TurnIdentifier.player;
                    this.SetTextLblStatus("It's your turn now!");
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
                BattleshipsForm.BattlefieldOpponent.SetImpact(x, y);

                // Gegner ist an der Reihe
                BattleshipsForm.WhosTurn = BattleshipsForm.TurnIdentifier.enemy;
                this.SetTextLblStatus("Enemy's turn!");
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
                BattleshipsForm.BattlefieldOpponent.SetMiss(x, y);

                // Gegner ist an der Reihe
                BattleshipsForm.WhosTurn = BattleshipsForm.TurnIdentifier.enemy;
                this.SetTextLblStatus("Enemy's turn!");
            }
            else if (data.StartsWith("RDY_"))
            {
                this.oroll = data.Remove(0, 4);
                BattleshipsForm.OpponentReadyToPlay = true;
                this.SetTextLblStatus("Opponend is ready and rolled: " + this.oroll);

                // Check if player is ready
                // if not: then wait until players ready
                if (BattleshipsForm.PlayerReadyToPlay)
                {
                    if (int.Parse(this.oroll) < this.roll)
                    {
                        this.SetTextLblStatus("Opponend rolled: " + this.oroll + " you rolled: " + this.roll.ToString());
                        this.SetTextLblStatus("Du darfst anfangen");
                        BattleshipsForm.WhosTurn = BattleshipsForm.TurnIdentifier.player;
                    }
                    else if (int.Parse(this.oroll) > this.roll)
                    {
                        this.SetTextLblStatus("Opponend rolled: " + this.oroll + " you rolled: " + this.roll.ToString());
                        this.SetTextLblStatus("Gegner darf anfangen");
                        BattleshipsForm.WhosTurn = BattleshipsForm.TurnIdentifier.enemy;
                    }
                }
                else
                {
                    // Wait until player is ready (player is ready if he has clicked on the ready button see btnRdy.clicked event)
                }
            }
            else
            {
                // Other messages
                this.SetText(data);
            }

            return true;
        }

        private void ButtonReadyClick(object sender, EventArgs e)
        {
            // Überprüfen ob alle Schiffe verteilt wurden
            if (BattleshipsForm.CounterBattleship >= 1 && BattleshipsForm.CounterGalley >= 1 && BattleshipsForm.CounterCruiser >= 3 && BattleshipsForm.CounterBoat >= 3)
            {
                if (this.MainClientSocket != null)
                {
                    if (this.MainClientSocket.Connected)
                    {
                        BattleshipsForm.PlayerReadyToPlay = true;
                        this.btnRdy.Enabled = false;

                        Random rnd = new Random();
                        this.roll = rnd.Next(101);
                        object objData = "RDY_" + this.roll.ToString();
                        byte[] byteData = System.Text.Encoding.ASCII.GetBytes(objData.ToString());

                        // Antwort an Gegner schicken
                        if (this.MainClientSocket.Connected)
                        {
                            this.MainClientSocket.Send(byteData);
                        }

                        this.SetTextLblStatus("You rolled: " + this.roll.ToString());

                        if (BattleshipsForm.OpponentReadyToPlay)
                        {
                            if (int.Parse(this.oroll) < this.roll)
                            {
                                this.SetTextLblStatus("Opponend rolled: " + this.oroll + " you rolled: " + this.roll.ToString());
                                this.SetTextLblStatus("Du darfst anfangen");
                                BattleshipsForm.WhosTurn = BattleshipsForm.TurnIdentifier.player;
                            }
                            else if (int.Parse(this.oroll) > this.roll)
                            {
                                this.SetTextLblStatus("Opponend rolled: " + this.oroll + " you rolled: " + this.roll.ToString());
                                this.SetTextLblStatus("Gegner darf anfangen");
                                BattleshipsForm.WhosTurn = BattleshipsForm.TurnIdentifier.enemy;
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
        /// Determines the internal IP address of your PC.
        /// </summary>
        /// <returns>IP address as a string.</returns>
        private string GetIP()
        {
            string strHostName = Dns.GetHostName();

            // Find host by name
            IPHostEntry iphostentry = Dns.GetHostEntry(strHostName);

            // Grab the first IP addresses
            string stringCurrentIP = string.Empty;
            foreach (IPAddress ipaddress in iphostentry.AddressList)
            {
                // select the first IPV4 address from the address list
                if (ipaddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    stringCurrentIP = ipaddress.ToString();
                    return stringCurrentIP;
                }  
            }

            return stringCurrentIP;
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
            if (BattleshipsForm.LabelStatus.InvokeRequired)
            {
                SetTextMainFormCallback d = new SetTextMainFormCallback(this.SetTextLblStatus);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                BattleshipsForm.LabelStatus.Text += text;
                BattleshipsForm.LabelStatus.Text += "\n";
                BattleshipsForm.PanelStatus.VerticalScroll.Value += BattleshipsForm.PanelStatus.VerticalScroll.SmallChange;
                BattleshipsForm.PanelStatus.Refresh();
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
            if (this.MainClientSocket != null)
            {
                this.MainClientSocket.Close();
                this.MainClientSocket = null;
            }

            BattleshipsForm.PlayerReadyToPlay = false;
            BattleshipsForm.OpponentReadyToPlay = false;
            this.SetText("Server closed!");
            this.SetTextLblStatus("Server closed!");
            this.UpdateControls(false);
        }

        private void ButtonDisconnectClick(object sender, EventArgs e)
        {
            this.CloseSocket();         
        }

        private void ClientGameFormClosed(object sender, FormClosedEventArgs e)
        {
            this.CloseSocket();
            BattleshipsForm.HostGameMenuItem.Enabled = true;
        }
    }
}
