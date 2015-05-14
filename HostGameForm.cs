//-----------------------------------------------------------------------
// <copyright file="HostGameForm.cs" company="Team 17">
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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

public partial class HostGameForm : Battleships.DoubleBufferedForm
    {
        /// <summary>
        /// Sends a request to an external web page.
        /// Then evaluates the received response (external IP).
        /// </summary>
        /// <returns>External IP Address (String).</returns>
        private static string GetExternalIP()
        {
            WebClient client = new WebClient();

            // Add a user agent header in case the requested URI contains a query.
            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR1.0.3705;)");
            string baseurl = "http://checkip.dyndns.org/";
            Stream data = client.OpenRead(baseurl);
            StreamReader reader = new StreamReader(data);
            string s = reader.ReadToEnd();
            data.Close();
            reader.Close();

            // Den erhaltenen Response so zurechtschneiden, dass nur noch die IP-Adresse übrig bleibt
            s = s.Replace("<html><head><title>Current IP Check</title></head><body>Current IP Address:", string.Empty).Replace("</body></html>", string.Empty).ToString();
            return s;
        }

        public Socket MainWorkerSocket;
        public int MainClientCount = 0;
        private AsyncCallback pfnWorkerCallBack;
        private Socket mainSocket;
        private int roll;
        private string oroll;

        private delegate void SetTextCallback(string text);

        private delegate void SetTextMainFormCallback(string text);

        private delegate void UpdateControlsCallback(bool listening);

        public HostGameForm()
        {
            this.InitializeComponent();
            this.textBoxIP.Text = this.GetIP();
        }

        private void ButtonHostGameClick(object sender, EventArgs e)
        {
            try
            {
                // Check the port value
                if (this.textBoxPort.Text == string.Empty)
                {
                    MessageBox.Show("Bitte geben Sie einen Port an");
                    return;
                }

                string portStr = this.textBoxPort.Text;
                    int port = System.Convert.ToInt32(portStr);

                    // Create the listening socket...
                    this.mainSocket = new Socket(
                                       AddressFamily.InterNetwork,
                                       SocketType.Stream,
                                       ProtocolType.Tcp);
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);

                    // Bind to local IP Address...
                    this.mainSocket.Bind(endPoint);

                    // Start listening...
                    this.listboxMessage.Items.Add("Initialize Server...");
                    this.mainSocket.Listen(1);
                    this.listboxMessage.Items.Add("Server initialized");

                    // Create the call back for any client connections...
                    this.mainSocket.BeginAccept(new AsyncCallback(this.OnClientConnect), null);

                    this.UpdateControls(true);
                    this.listboxMessage.Items.Add("Waiting for Client...");   
            }
            catch (SocketException se)
            {
                this.SetText(se.Message);
            }
        }

        public void OnClientConnect(IAsyncResult asyn)
        {
            try
            {
                if (this.MainClientCount < 1)
                {
                    // Here we complete/end the BeginAccept() asynchronous call
                    // by calling EndAccept() - which returns the reference to
                    // a new Socket object
                    this.MainWorkerSocket = this.mainSocket.EndAccept(asyn);

                    // Let the worker Socket do the further processing for the
                    // just connected client
                    this.WaitForData(this.MainWorkerSocket);

                    // Display this client connection as a status message on the GUI
                    string str = string.Format("Client at {0} connected", this.MainWorkerSocket.RemoteEndPoint.ToString());
                    //// textBoxMsg.Text = str;
                    this.SetText(str);
                    this.SetTextLblStatus(str);

                    // Ack zum Client schicken (Verbindungsbestätigung)
                    object objData = "ACK";
                    byte[] byteData = System.Text.Encoding.ASCII.GetBytes(objData.ToString());
                    if (this.MainWorkerSocket != null)
                    {
                        if (this.MainWorkerSocket.Connected)
                        {
                            this.MainWorkerSocket.Send(byteData);
                        }
                    }
                   
                    // Now increment the client count
                    ++this.MainClientCount;

                    // Since the main Socket is now free, it can go back and wait for
                    // other clients who are attempting to connect
                    //// mainSocket.BeginAccept(new AsyncCallback(OnClientConnect), null);
                }
                else
                {
                    Socket tmp_workerSocket = this.mainSocket.EndAccept(asyn);
                    this.WaitForData(tmp_workerSocket);
                    this.SetText("Client at: " + tmp_workerSocket.RemoteEndPoint.ToString() + " tried to connect to Server");
                    this.SetText("But Server is already full!");
                    this.SetTextLblStatus("Client at: " + tmp_workerSocket.RemoteEndPoint.ToString() + " tried to connect to Server");
                    this.SetTextLblStatus("But Server is already full!");
                    object objData = "FULL";
                    byte[] byteData = System.Text.Encoding.ASCII.GetBytes(objData.ToString());
                    tmp_workerSocket.Send(byteData);
                    tmp_workerSocket.Close();
                }
            }
            catch (ObjectDisposedException)
            {
                this.SetText("OnClientConnection: Socket has been closed\n");
            }
            catch (SocketException se)
            {
                this.SetText(se.Message);
            }
        }

        // Start waiting for data from the client
        public void WaitForData(System.Net.Sockets.Socket soc)
        {
            try
            {
                if (this.pfnWorkerCallBack == null)
                {
                    // Specify the call back function which is to be 
                    // invoked when there is any write activity by the 
                    // connected client
                    this.pfnWorkerCallBack = new AsyncCallback(this.OnDataReceived);
                }

                SocketPacket theSocPkt = new SocketPacket();
                theSocPkt.MainCurrentSocket = soc;

                // Start receiving any data written by the connected client
                // asynchronously
                soc.BeginReceive(theSocPkt.DataBuffer, 0, theSocPkt.DataBuffer.Length, SocketFlags.None, this.pfnWorkerCallBack, theSocPkt);
            }
            catch (SocketException se)
            {
                this.SetText(se.Message);
                --this.MainClientCount;     
            }
        }

        // This the call back function which will be invoked when the socket
        // detects any client writing of data on the stream
        public void OnDataReceived(IAsyncResult asyn)
        {
            try
            {
                SocketPacket socketData = (SocketPacket)asyn.AsyncState;

                int iRx = 0;

                // Complete the BeginReceive() asynchronous call by EndReceive() method
                // which will return the number of characters written to the stream 
                // by the client
                iRx = socketData.MainCurrentSocket.EndReceive(asyn);
                char[] chars = new char[iRx + 1];
                Decoder d = Encoding.UTF8.GetDecoder();
                int charLen = d.GetChars(socketData.DataBuffer, 0, iRx, chars, 0);
                string data = new string(chars);
                //// richTextBoxReceivedMsg.AppendText(data);
                //// SetTextRichTextBox(data);

                // Empfangene Daten verarbeiten und entsperchenden Service auswählen
                this.Services(data);

                // Continue the waiting for data on the Socket
                this.WaitForData(socketData.MainCurrentSocket);
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode == SocketError.ConnectionReset)
                {
                    this.SetText("Client at " + this.MainWorkerSocket.RemoteEndPoint.ToString() + " disconnected!\n");
                    try
                    {
                        this.MainWorkerSocket.Close();
                    }
                    catch (Exception)
                    {
                    }

                    this.MainWorkerSocket = null;
                    this.MainClientCount--;
                }

                // MessageBox.Show(se.Message);
            }
            catch (Exception ex)
            {
                this.SetText("OnDataReceived: Socket has been closed Socket error: " + ex.Message.ToString());
                this.CloseSockets();
            }
        }

        /// <summary>
        /// Processes the received data and performs the appropriate operation.
        /// </summary>
        /// <param name="data">Receive data from the other player.</param>
        private void Services(string data)
        {
            // Koordinaten vom Gegner erhalten (Auswerten ob an den Koords ein Schiff gesetzt ist oder nicht)
            if (data.Contains("pb_"))
            {
                // Erhaltene koordinaten ausgeben
                this.SetText("Coords received: " + data);

                // X und Y aus der Nachricht lesen
                string pos = data.Remove(0, 3);
                string[] posData = pos.Split(':');
                int x = int.Parse(posData[0]);
                int y = int.Parse(posData[1]);

                object objData;

                // Evaluate whether the enemy has hit or not
                if (!BattleshipsForm.BattlefieldPlayer.HitOrMiss(x, y))
                {
                    // Inform the enemy that he missed
                    objData = "MISS" + x.ToString() + ":" + y.ToString();
                    BattleshipsForm.BattlefieldPlayer.SetMiss(x, y);
                }
                else
                {
                    // Inform the enemy that he landed a hit
                    objData = "HIT" + x.ToString() + ":" + y.ToString();

                    // Set hit (On own field --> Enemy hit)
                    if (BattleshipsForm.BattlefieldPlayer.SetImpact(x, y))
                    {
                        // Enemy won (all ships were destroyed)
                        objData = "WIN";
                    }
                }

                byte[] byteData = System.Text.Encoding.ASCII.GetBytes(objData.ToString());

                // Send answer to enemy
                if (this.MainWorkerSocket.Connected)
                {
                    this.MainWorkerSocket.Send(byteData);
                }

                // If opponent has won, then players may no longer have a turn...
                if (objData.ToString().StartsWith("WIN"))
                {
                    // TODO: Play Sound (Loser).
                    MessageBox.Show(this, "Loser!", "lose", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
            // TODO: Play Sound (Winner).
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

                // Dem Spieler anzeigen, dass er getroffen hat (Auf dem Gegnerfeld)
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

                // Prüfen ob Spieler auch bereit ist?
                if (BattleshipsForm.PlayerReadyToPlay)
                {
                    if (int.Parse(this.oroll) < this.roll)
                    {
                        this.SetTextLblStatus("Opponend rolled: " + this.oroll + " you rolled: " + this.roll.ToString());
                        this.SetTextLblStatus("Du darfst anfangen!");
                        BattleshipsForm.WhosTurn = BattleshipsForm.TurnIdentifier.player;
                    }
                    else if (int.Parse(this.oroll) > this.roll)
                    {
                        this.SetTextLblStatus("Opponend rolled: " + this.oroll + " you rolled: " + this.roll.ToString());
                        this.SetTextLblStatus("Gegner darf anfangen!");
                        BattleshipsForm.WhosTurn = BattleshipsForm.TurnIdentifier.enemy;
                    }
                }
                else
                {
                    // Warten bis Spieler bereit ist (Spieler ist bereit, wenn er auf den Bereit-Button gedrückt hat - Siehe Event btnRdy.clicked)
                }
            }
            else
            {
                this.SetText(data);
            }
        }

        private void ButtonReadyClick(object sender, EventArgs e)
        {
            if (BattleshipsForm.CounterBattleship >= 1 && BattleshipsForm.CounterGalley >= 1 && BattleshipsForm.CounterCruiser >= 3 && BattleshipsForm.CounterBoat >= 3)
            {
                if (this.MainWorkerSocket != null)
                {
                    if (this.MainWorkerSocket.Connected)
                    {
                        BattleshipsForm.PlayerReadyToPlay = true;
                        this.btnRdy.Enabled = false;

                        Random rnd = new Random();
                        this.roll = rnd.Next(101);
                        object objData = "RDY_" + this.roll.ToString();
                        byte[] byteData = System.Text.Encoding.ASCII.GetBytes(objData.ToString());

                        // Antwort an Gegner schicken
                        if (this.MainWorkerSocket.Connected)
                        {
                            this.MainWorkerSocket.Send(byteData);
                            this.SetTextLblStatus("You rolled: " + this.roll.ToString());
                        }

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
        /// Determines the internal IP address of the PC.
        /// </summary>
        /// <returns>Internal IP-Address as a string.</returns>
        private string GetIP()
        {
            string strHostName = Dns.GetHostName();

            // Find host by name
            IPHostEntry iphostentry = Dns.GetHostEntry(strHostName);
            //// Dns.GetHostByName(strHostName);

            // Grab the first IP addresses
            string stringCurrentIP = string.Empty;
            foreach (IPAddress ipaddress in iphostentry.AddressList)
            {     
                // Die erste IPV4 Adresse aus der Adressliste wählen
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
            if (this.listboxMessage.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(this.SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.listboxMessage.Items.Add(text);
                this.listboxMessage.SelectedIndex = this.listboxMessage.Items.Count - 1;
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

        private void UpdateControls(bool listening)
        {
            if (this.btnCloseGame.InvokeRequired)
            {
                UpdateControlsCallback d = new UpdateControlsCallback(this.UpdateControls);
                this.Invoke(d, new object[] { listening });
            }
            else
            {
                this.btnHostGame.Enabled = !listening;
                this.btnCloseGame.Enabled = listening;
                this.btnRdy.Enabled = listening;
            }
        }

        private void ButtonCloseGameClick(object sender, EventArgs e)
        {
            this.CloseSockets();
            this.UpdateControls(false);
        }

        private void CloseSockets()
        {
            if (this.mainSocket != null)
            {
                this.mainSocket.Close();
            }

            if (this.MainWorkerSocket != null)
            {
                this.MainWorkerSocket.Close(1);
                this.MainWorkerSocket = null;
            }

            BattleshipsForm.PlayerReadyToPlay = false;
            BattleshipsForm.OpponentReadyToPlay = false;
            this.SetText("Server closed!");
            this.SetTextLblStatus("Server closed!");
        }

        private void HostGameForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.CloseSockets();
            BattleshipsForm.JoinGameMenuItem.Enabled = true;
        }

        private void ButtonExternalIpClick(object sender, EventArgs e)
        {
            this.textBoxIP.Text = GetExternalIP();
        }

        private void ButtonInternalIPClick(object sender, EventArgs e)
        {
            this.textBoxIP.Text = this.GetIP();
        }
    }
}
