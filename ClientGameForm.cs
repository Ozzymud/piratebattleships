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
// <project>Battleships Pirate Edition</project>
// <author>Markus Bohnert</author>
// <team>Simon Hodler, Markus Bohnert</team>
//-----------------------------------------------------------------------

namespace Battleships
{
#region directives
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
#endregion

/// <summary>
/// The join game window.
/// </summary>
public partial class ClientGameForm : Battleships.DoubleBufferedForm
    {
    #region fields
    private IAsyncResult mainResult;
    private AsyncCallback pfnCallBack;

    /// <summary>
    /// Read 10 bytes from the socket connection.
    /// </summary>
    private byte[] mainDataBuffer = new byte[10];

    /// <summary>
    /// Contains the value of the players roll of the dice.
    /// </summary>
    private int roll;

    /// <summary>
    /// Contains the value of the opponents roll of the dice.
    /// </summary>
    private string oroll;

    private Socket clientSocket;
    #endregion

    #region constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="ClientGameForm" /> class.
    /// </summary>
    public ClientGameForm()
        {
            this.InitializeComponent();
            this.textboxIP.Text = GetIP();
        }
    #endregion

    #region delegate
    private delegate void SetRichTextBoxRxCallback(string text);

    private delegate void SetTextMainFormCallback(string text);

    private delegate void UpdateControlsCallback(bool listening);
    #endregion

    #region method
    #region public method
    /// <summary>
    /// Gets or sets data in clientSocket (publicly accessible)
    /// </summary>
    public Socket ClientSocket
        {
            get { return this.clientSocket; }
            set { this.clientSocket = value; }
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
                theSocPkt.CurrentSocket = this.ClientSocket;

                // Start listening to the data asynchronously
                this.mainResult = this.ClientSocket.BeginReceive(
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
                    this.SetTextLabelStatus("Server closed!");
                    this.ClientSocket.Close();
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

    public void OnDataReceived(IAsyncResult result)
        {
            try
            {
                SocketPacket theSockId = (SocketPacket)result.AsyncState;
                int iRx = theSockId.CurrentSocket.EndReceive(result);
                char[] chars = new char[iRx + 1];
                Decoder d = Encoding.UTF8.GetDecoder();
                int charLen = d.GetChars(theSockId.DataBuffer, 0, iRx, chars, 0);
                string data = new string(chars);

                // Process incoming data and select service.
                if (this.Services(data))
                {
                    // Wait for data.
                    this.WaitForData();
                }
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode == SocketError.ConnectionReset)
                {
                    this.SetText("Server closed!\n");
                    this.SetTextLabelStatus("Server closed!");
                    this.UpdateControls(false);
                }
            }
            catch (Exception ex)
            {
                this.SetText("OnDataReceived: Socket has been closed Socket error: " + ex.Message.ToString());
                this.CloseSocket();
            }
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

    public void SetTextLabelStatus(string text)
        {
            if (BattleshipsForm.LabelStatus.InvokeRequired)
            {
                SetTextMainFormCallback d = new SetTextMainFormCallback(this.SetTextLabelStatus);
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
    #endregion

    #region private method
    /// <summary>
    /// Determines the internal IP address of your PC.
    /// </summary>
    /// <returns>IP address as a string.</returns>
    private static string GetIP()
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

    private void ButtonConnect_Click(object sender, EventArgs e)
        {
            // See if we have text on the IP and Port text fields
            if (string.IsNullOrEmpty(this.textboxIP.Text) || string.IsNullOrEmpty(this.textboxPort.Text))
            {
                MessageBox.Show("IP Address and Port Number are required to connect to a server.");
                return;
            }

            try
            {
                this.UpdateControls(false);

                // Create the socket instance
                this.ClientSocket = new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp);

                // Cet the remote IP address
                IPAddress ip = IPAddress.Parse(this.textboxIP.Text);
                int internetPortNumber = System.Convert.ToInt16(this.textboxPort.Text, CultureInfo.InvariantCulture);

                // Create the end point
                IPEndPoint endPoint = new IPEndPoint(ip, internetPortNumber);

                // Connect to the remote host
                this.ClientSocket.Connect(endPoint);
                if (this.ClientSocket.Connected)
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
                this.UpdateControls(false);
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
            // else if: get coordinates (pf_) from an opponent (evaluate whether a ship is set to the coords or not) 
            if (data.StartsWith("ACK", StringComparison.Ordinal))
            {
                this.SetTextLabelStatus("Connected to server");
            }
            else if (data.StartsWith("FULL", StringComparison.Ordinal))
            {
                this.SetText("Server is full...closing connection!");
                if (this.ClientSocket != null)
                {
                    this.ClientSocket.Close();
                    this.ClientSocket = null;
                    this.UpdateControls(false);
                    this.SetText("Disconnected from Server!");
                }

                return false;
            }
            else if (data.Contains("pf_"))
            {
                // Output coordinates received.
                this.SetText("Coords received: " + data);

                // X and Y read from the message
                string pos = data.Remove(0, 3);
                string[] posData = pos.Split(':');
                int x = int.Parse(posData[0], CultureInfo.InvariantCulture);
                int y = int.Parse(posData[1], CultureInfo.InvariantCulture);

                object objData;

                // Evaluate whether the opponent has hit or not
                if (!BattleshipsForm.BattlefieldPlayer.HitOrMiss(x, y))
                {
                    // Evaluate whether the opponent has missed.
                    objData = "MISS" + x.ToString(CultureInfo.InvariantCulture) + ":" + y.ToString(CultureInfo.InvariantCulture);
                    BattleshipsForm.BattlefieldPlayer.SetMiss(x, y);
                }
                else
                {
                    // Evaluate Whether the opponent has hit.
                    objData = "HIT" + x.ToString(CultureInfo.InvariantCulture) + ":" + y.ToString(CultureInfo.InvariantCulture);

                    // Show impact (made on private field --> opponent)
                    if (BattleshipsForm.BattlefieldPlayer.SetImpact(x, y))
                    {
                        // Opponent won (all ships were destroyed)
                        objData = "WIN";
                    }
                }

                byte[] byteData = System.Text.Encoding.ASCII.GetBytes(objData.ToString());

                // Send answer to opponent.
                if (this.ClientSocket.Connected)
                {
                    this.ClientSocket.Send(byteData);
                }

                // If opponent has won, then players may no longer have a turn...
                if (objData.ToString().StartsWith("WIN", StringComparison.Ordinal))
                {
                    // TODO: Play sound - Loser...
                    MessageBox.Show(this, "You have lost!", "Loser", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    Application.Exit();
                }
                else
                {
                    // Player's turn
                    BattleshipsForm.WhosTurn = BattleshipsForm.TurnIdentifier.Player;
                    this.SetTextLabelStatus("It's your turn!");
                }
            }
            else if (data.StartsWith("WIN", StringComparison.Ordinal))
            {
                // You have won!
                // TODO: Play sound - Winner...
                MessageBox.Show(this, "You have won!", "Winner!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Application.Exit();
            }
            else if (data.StartsWith("HIT", StringComparison.Ordinal))
            {
                // You've landed a hit!
                string pos = data.Remove(0, 3);
                string[] posData = pos.Split(':');
                int x = int.Parse(posData[0], CultureInfo.InvariantCulture);
                int y = int.Parse(posData[1], CultureInfo.InvariantCulture);
                this.SetText("HIT received at x:" + x.ToString(CultureInfo.InvariantCulture) + " y:" + y.ToString(CultureInfo.InvariantCulture));

                // Announce to the player that he has hit (on the opponent's field)
                BattleshipsForm.BattlefieldOpponent.SetImpact(x, y);

                // Opponent's turn.
                BattleshipsForm.WhosTurn = BattleshipsForm.TurnIdentifier.Enemy;
                this.SetTextLabelStatus("Enemy's turn!");
            }
            else if (data.StartsWith("MISS", StringComparison.Ordinal))
            {
                // Unfortunately, the shot missed, try again!
                string pos = data.Remove(0, 4);
                string[] posData = pos.Split(':');
                int x = int.Parse(posData[0], CultureInfo.InvariantCulture);
                int y = int.Parse(posData[1], CultureInfo.InvariantCulture);
                this.SetText("MISS received at x:" + x.ToString(CultureInfo.InvariantCulture) + " y:" + y.ToString(CultureInfo.InvariantCulture));

                // Show the player that he missed (on the opponent's field)
                BattleshipsForm.BattlefieldOpponent.SetMiss(x, y);

                // Opponent's turn
                BattleshipsForm.WhosTurn = BattleshipsForm.TurnIdentifier.Enemy;
                this.SetTextLabelStatus("Enemy's turn!");
            }
            else if (data.StartsWith("RDY_", StringComparison.Ordinal))
            {
                this.oroll = data.Remove(0, 4);
                BattleshipsForm.OpponentReadyToPlay = true;
                this.SetTextLabelStatus("Opponent is ready!");

                // Check if player is ready
                // if not: then wait until players ready
                if (BattleshipsForm.PlayerReadyToPlay)
                {
                    if (int.Parse(this.oroll, CultureInfo.InvariantCulture) < this.roll)
                    {
                        this.SetTextLabelStatus("Opponent rolled: " + this.oroll + " you rolled: " + this.roll.ToString(CultureInfo.InvariantCulture));
                        this.SetTextLabelStatus("You start!");
                        BattleshipsForm.WhosTurn = BattleshipsForm.TurnIdentifier.Player;
                    }
                    else if (int.Parse(this.oroll, CultureInfo.InvariantCulture) > this.roll)
                    {
                        this.SetTextLabelStatus("Opponent rolled: " + this.oroll + " you rolled: " + this.roll.ToString(CultureInfo.InvariantCulture));
                        this.SetTextLabelStatus("Opponent starts!");
                        BattleshipsForm.WhosTurn = BattleshipsForm.TurnIdentifier.Enemy;
                    }
                }
                else
                {
                    // Wait until player is ready (player is ready if he has clicked on the ready button see buttonReady.clicked event)
                }
            }
            else
            {
                // Other messages
                this.SetText(data);
            }

            return true;
        }

    private void ButtonReady_Click(object sender, EventArgs e)
        {
            // Check whether all the ships were distributed.
            if (BattleshipsForm.CounterBattleship >= 1 && BattleshipsForm.CounterGalley >= 1 && BattleshipsForm.CounterCruiser >= 3 && BattleshipsForm.CounterBoat >= 3)
            {
                if (this.ClientSocket != null)
                {
                    if (this.ClientSocket.Connected)
                    {
                        BattleshipsForm.PlayerReadyToPlay = true;
                        this.WindowState = FormWindowState.Minimized;
                        this.buttonReady.Enabled = false;

                        Random rnd = new Random();
                        this.roll = rnd.Next(101);
                        object objData = "RDY_" + this.roll.ToString();
                        byte[] byteData = System.Text.Encoding.ASCII.GetBytes(objData.ToString());

                        // Send answer to opponent.
                        if (this.ClientSocket.Connected)
                        {
                            this.ClientSocket.Send(byteData);
                        }

                        this.SetTextLabelStatus("You rolled: " + this.roll.ToString(CultureInfo.InvariantCulture));

                        if (BattleshipsForm.OpponentReadyToPlay)
                        {
                            if (int.Parse(this.oroll, CultureInfo.InvariantCulture) < this.roll)
                            {
                                this.SetTextLabelStatus("Opponent rolled: " + this.oroll + " you rolled: " + this.roll.ToString(CultureInfo.InvariantCulture));
                                this.SetTextLabelStatus("You start!");
                                BattleshipsForm.WhosTurn = BattleshipsForm.TurnIdentifier.Player;
                            }
                            else if (int.Parse(this.oroll, CultureInfo.InvariantCulture) > this.roll)
                            {
                                this.SetTextLabelStatus("Opponent rolled: " + this.oroll + " you rolled: " + this.roll.ToString(CultureInfo.InvariantCulture));
                                this.SetTextLabelStatus("Opponent starts!");
                                BattleshipsForm.WhosTurn = BattleshipsForm.TurnIdentifier.Enemy;
                            }
                        }
                        else
                        {
                            // Wait until the opponent is ready (opponent is ready when the 'RDY' flag comes)
                        }
                    }
                    else
                    {
                        MessageBox.Show(this, "An opponent must be connected!", "Not ready", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show(this, "An opponent must be connected!", "Not ready", MessageBoxButtons.OK, MessageBoxIcon.Information);                    
                }
            }
            else
            {
                MessageBox.Show(this, "All ships must be distributed first!", "Not ready", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                this.buttonReady.Enabled = status;
            }
        }

    private void CloseSocket()
        {
            if (this.ClientSocket != null)
            {
                this.ClientSocket.Close();
                this.ClientSocket = null;
            }

            BattleshipsForm.PlayerReadyToPlay = false;
            BattleshipsForm.OpponentReadyToPlay = false;
            this.SetText("Server closed!");
            this.SetTextLabelStatus("Server closed!");
            this.UpdateControls(false);
        }

    private void ButtonDisconnect_Click(object sender, EventArgs e)
        {
            this.CloseSocket();         
        }

    private void ClientGameFormClosed(object sender, FormClosedEventArgs e)
        {
            this.CloseSocket();
            Battleships.BattleshipsForm.NetworkFormOpen = 0;
        }
    #endregion
    #endregion
    }
}
