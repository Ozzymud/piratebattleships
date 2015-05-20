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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
#endregion

/// <summary>
/// The host server game window.
/// </summary>
public partial class HostGameForm : Battleships.DoubleBufferedForm
    {
    #region field
    private Socket workerSocket;

    private int clientCount = 0;

    /// <summary>
    /// Define a callback.
    /// </summary>
    private AsyncCallback pfnWorkerCallBack;

    /// <summary>
    /// The main socket between host and client.
    /// </summary>
    private Socket mainSocket;

    /// <summary>
    /// Contains the value of the players roll of the dice.
    /// </summary>
    private int roll;

    /// <summary>
    /// Contains the value of the opponents roll of the dice.
    /// </summary>
    private string oroll;
    #endregion

    #region constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="HostGameForm" /> class.
    /// </summary>
    public HostGameForm()
        {
            this.InitializeComponent();
            this.textBoxIP.Text = GetIP();
        }
    #endregion constructor

    #region delegate
    private delegate void SetTextCallback(string text);

    private delegate void SetTextMainFormCallback(string text);

    private delegate void UpdateControlsCallback(bool listening);
    #endregion

    #region method
    #region public
    /// <summary>
    /// Gets or sets data in workerSocket (publicly accessible)
    /// </summary>
    public Socket WorkerSocket
        {
            get { return this.workerSocket; }
            set { this.workerSocket = value; }
        }

    /// <summary>
    /// Gets or sets the number of clients connected to the host (public accessor to clientCount).
    /// </summary>
    public int ClientCount
        {
            get { return this.clientCount; }
            set { this.clientCount = value; }
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
                BattleshipsForm.PanelStatus.PerformLayout();
                BattleshipsForm.PanelStatus.Refresh();
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
                theSocPkt.CurrentSocket = soc;

                // Start receiving any data written by the connected client
                // asynchronously
                soc.BeginReceive(theSocPkt.DataBuffer, 0, theSocPkt.DataBuffer.Length, SocketFlags.None, this.pfnWorkerCallBack, theSocPkt);
            }
            catch (SocketException se)
            {
                this.SetText(se.Message);
                --this.clientCount;     
            }
        }

    // This the call back function which will be invoked when the socket
    // detects any client writing of data on the stream
    public void OnDataReceived(IAsyncResult result)
        {
            try
            {
                SocketPacket socketData = (SocketPacket)result.AsyncState;

                int iRx = 0;

                // Complete the BeginReceive() asynchronous call by EndReceive() method
                // which will return the number of characters written to the stream 
                // by the client
                iRx = socketData.CurrentSocket.EndReceive(result);
                char[] chars = new char[iRx + 1];
                Decoder d = Encoding.UTF8.GetDecoder();
                int charLen = d.GetChars(socketData.DataBuffer, 0, iRx, chars, 0);
                string data = new string(chars);

                // Process received data and select a suitable action
                this.Services(data);

                // Continue waiting for data on the socket
                this.WaitForData(socketData.CurrentSocket);
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode == SocketError.ConnectionReset)
                {
                    this.SetText("Client at " + this.WorkerSocket.RemoteEndPoint.ToString() + " disconnected!\n");
                    try
                    {
                        this.WorkerSocket.Close();
                    }
                    catch (Exception)
                    {
                    }

                    this.WorkerSocket = null;
                    this.clientCount--;
                }

                // MessageBox.Show(se.Message);
            }
            catch (Exception ex)
            {
                this.SetText("OnDataReceived: " + ex.Message.ToString());
                this.CloseSockets();
            }
        }

    public void OnClientConnect(IAsyncResult result)
        {
            try
            {
                if (this.clientCount < 1)
                {
                    // Here we complete/end the BeginAccept() asynchronous call
                    // by calling EndAccept() - which returns the reference to
                    // a new Socket object
                    this.WorkerSocket = this.mainSocket.EndAccept(result);

                    // Let the worker Socket do the further processing for the
                    // just connected client
                    this.WaitForData(this.WorkerSocket);

                    // Display this client connection as a status message on the GUI
                    string str = string.Format("Client at {0} connected", this.WorkerSocket.RemoteEndPoint, CultureInfo.InvariantCulture);
                    this.SetText(str);
                    this.SetTextLabelStatus(str);

                    // Send ACK to the client (confirmation)
                    object objData = "ACK";
                    byte[] byteData = System.Text.Encoding.ASCII.GetBytes(objData.ToString());
                    if (this.WorkerSocket != null)
                    {
                        if (this.WorkerSocket.Connected)
                        {
                            this.WorkerSocket.Send(byteData);
                        }
                    }
                   
                    // Now increment the client count
                    ++this.clientCount;

                    // Since the main Socket is now free, it can go back and wait for
                    // other clients who are attempting to connect
                    // TODO: this partially works, informs new clients that the server is full
                    // but it also disconnects clients that are already connected
                    this.mainSocket.BeginAccept(new AsyncCallback(this.OnClientConnect), null);
                }
                else
                {
                    Socket tmp_workerSocket = this.mainSocket.EndAccept(result);
                    this.WaitForData(tmp_workerSocket);
                    this.SetText("Client at: " + tmp_workerSocket.RemoteEndPoint.ToString() + " tried to connect");
                    this.SetText("But the server is full!");
                    this.SetTextLabelStatus("Client at: " + tmp_workerSocket.RemoteEndPoint.ToString() + " tried to connect");
                    this.SetTextLabelStatus("But the server is full!");
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
    #endregion

    #region private static
    /// <summary>
    /// Determines the internal IP address of the PC.
    /// </summary>
    /// <returns>Internal IP-Address as a string.</returns>
    private static string GetIP()
        {
            string strHostName = Dns.GetHostName();

            // Find host by name
            IPHostEntry iphostentry = Dns.GetHostEntry(strHostName);

            // Grab the first IP addresses
            string stringCurrentIP = string.Empty;
            foreach (IPAddress ipaddress in iphostentry.AddressList)
            {     
                // Choose the first IPV4 address from the address list
                if (ipaddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    stringCurrentIP = ipaddress.ToString();
                    return stringCurrentIP;
                }                  
            }

            return stringCurrentIP;
        }

    private static string GetExternalIP()
        {
            // Other services:
            // http://icanhazip.com
            // http://bot.whatismyipaddress.com
            // http://ipinfo.io/ip
            string externalip = new WebClient().DownloadString("http://api.ipify.org").ToString();
            return externalip;
        }
    #endregion

    #region private
    /// <summary>
    /// Processes the received data and performs the appropriate operation.
    /// </summary>
    /// <param name="data">Receive data from the other player.</param>
    private void Services(string data)
        {
            // Get the coordinates from the opponents  (evaluate whether a ship is at the coordinates or not)
            if (data.Contains("pf_"))
            {
                // Output coordinates received
                this.SetText("Coords received: " + data);

                // Read X and Y from the message
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
                if (this.WorkerSocket.Connected)
                {
                    this.WorkerSocket.Send(byteData);
                }

                // If opponent has won, then players may no longer have a turn...
                if (objData.ToString().StartsWith("WIN", StringComparison.Ordinal))
                {
                    // TODO: Play sound - Loser...
                    MessageBox.Show(this, "You have lost!", "Loser", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else
                {
                    // Player's Turn
                    BattleshipsForm.WhosTurn = BattleshipsForm.TurnIdentifier.Player;
                    this.SetTextLabelStatus("It's your turn now!");
                }
            }
            else if (data.StartsWith("WIN", StringComparison.Ordinal))
            {
            // You win!
            // TODO: Play sound - Winner...
            MessageBox.Show(this, "You have won!", "Winner", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            Application.Exit();
            }
            else if (data.StartsWith("HIT", StringComparison.Ordinal))
            {
            // You have landed a hit! 
                string pos = data.Remove(0, 3);
                string[] posData = pos.Split(':');
                int x = int.Parse(posData[0]);
                int y = int.Parse(posData[1]);
                this.SetText("HIT received at x:" + x.ToString(CultureInfo.InvariantCulture) + " y:" + y.ToString(CultureInfo.InvariantCulture));

                // Register that the player has hit (on the opponent's field)
                BattleshipsForm.BattlefieldOpponent.SetImpact(x, y);

                // Opponent's turn
                BattleshipsForm.WhosTurn = BattleshipsForm.TurnIdentifier.Enemy;
                this.SetTextLabelStatus("Enemy's turn!");
            }
            else if (data.StartsWith("MISS", StringComparison.Ordinal))
            {
            // Unfortunately the shot missed, try again!
                string pos = data.Remove(0, 4);
                string[] posData = pos.Split(':');
                int x = int.Parse(posData[0]);
                int y = int.Parse(posData[1]);
                this.SetText("MISS received at x:" + x.ToString(CultureInfo.InvariantCulture) + " y:" + y.ToString(CultureInfo.InvariantCulture));

                // Register that the player has missed (on the opponent's field)
                BattleshipsForm.BattlefieldOpponent.SetMiss(x, y);

                // Opponent's turn.
                BattleshipsForm.WhosTurn = BattleshipsForm.TurnIdentifier.Enemy;
                this.SetTextLabelStatus("Enemy's turn!");
            }
            else if (data.StartsWith("RDY_", StringComparison.Ordinal))
            {
                this.oroll = data.Remove(0, 4);
                BattleshipsForm.OpponentReadyToPlay = true;
                this.SetTextLabelStatus("Opponent is ready and rolled: " + this.oroll);

                // Also check whether player is ready? 
                if (BattleshipsForm.PlayerReadyToPlay)
                {
                    if (int.Parse(this.oroll) < this.roll)
                    {
                        this.SetTextLabelStatus("Opponent rolled: " + this.oroll + " you rolled: " + this.roll.ToString());
                        this.SetTextLabelStatus("You start!");
                        BattleshipsForm.WhosTurn = BattleshipsForm.TurnIdentifier.Player;
                    }
                    else if (int.Parse(this.oroll) > this.roll)
                    {
                        this.SetTextLabelStatus("Opponent rolled: " + this.oroll + " you rolled: " + this.roll.ToString());
                        this.SetTextLabelStatus("Opponent starts!");
                        BattleshipsForm.WhosTurn = BattleshipsForm.TurnIdentifier.Enemy;
                    }
                }
                else
                {
                    // Wait until player is ready (player is ready if he has clicked  on the ready button, see event "buttonReady")
                }
            }
            else
            {
                this.SetText(data);
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
                this.buttonReady.Enabled = listening;
            }
        }

    private void CloseSockets()
        {
            if (this.mainSocket != null)
            {
                this.mainSocket.Close();
            }

            if (this.WorkerSocket != null)
            {
                this.WorkerSocket.Close(1);
                this.WorkerSocket = null;
            }

            BattleshipsForm.PlayerReadyToPlay = false;
            BattleshipsForm.OpponentReadyToPlay = false;
            this.SetText("Server closed!");
            this.SetTextLabelStatus("Server closed!");
        }

    private void HostGameForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.CloseSockets();
            Battleships.BattleshipsForm.NetworkFormOpen = 0;
        }

    #region mouse events
    private void ButtonHostGame_Click(object sender, EventArgs e)
        {
            try
            {
                // Check the port value
                if (string.IsNullOrEmpty(this.textBoxPort.Text))
                {
                    MessageBox.Show("Please specify a port.");
                    return;
                }

                string portStr = this.textBoxPort.Text;
                    int port = System.Convert.ToInt32(portStr, CultureInfo.InvariantCulture);

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

    private void ButtonReady_Click(object sender, EventArgs e)
        {
            if (BattleshipsForm.CounterBattleship >= 1 && BattleshipsForm.CounterGalley >= 1 && BattleshipsForm.CounterCruiser >= 3 && BattleshipsForm.CounterBoat >= 3)
            {
                if (this.WorkerSocket != null)
                {
                    if (this.WorkerSocket.Connected)
                    {
                        BattleshipsForm.PlayerReadyToPlay = true;
                        this.WindowState = FormWindowState.Minimized;                    
                        this.buttonReady.Enabled = false;

                        Random rnd = new Random();
                        this.roll = rnd.Next(101);
                        object objData = "RDY_" + this.roll.ToString();
                        byte[] byteData = System.Text.Encoding.ASCII.GetBytes(objData.ToString());

                        // Send answer to opponents
                        if (this.WorkerSocket.Connected)
                        {
                            this.WorkerSocket.Send(byteData);
                            this.SetTextLabelStatus("You rolled: " + this.roll.ToString());
                        }

                        if (BattleshipsForm.OpponentReadyToPlay)
                        {
                            if (int.Parse(this.oroll) < this.roll)
                            {
                                this.SetTextLabelStatus("Opponent rolled: " + this.oroll + " you rolled: " + this.roll.ToString());
                                this.SetTextLabelStatus("You start.");
                                BattleshipsForm.WhosTurn = BattleshipsForm.TurnIdentifier.Player;
                            }
                            else if (int.Parse(this.oroll) > this.roll)
                            {
                                this.SetTextLabelStatus("Opponent rolled: " + this.oroll + " you rolled: " + this.roll.ToString());
                                this.SetTextLabelStatus("Opponent starts.");
                                BattleshipsForm.WhosTurn = BattleshipsForm.TurnIdentifier.Enemy;
                            }
                        }
                        else
                        {
                            // Wait until the opponent is ready (opponent is ready if 'RDY' flag is received)
                        }
                    }
                    else
                    {
                        MessageBox.Show(this, "An opponent must be connected first!", "Not ready", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show(this, "An opponent must be connected first!", "Not ready", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show(this, "All ships must be distributed first!", "Not ready", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

    private void ButtonCloseGame_Click(object sender, EventArgs e)
        {
            this.CloseSockets();
            this.UpdateControls(false);
        }

    private void ButtonExternalIp_Click(object sender, EventArgs e)
        {
            this.textBoxIP.Text = GetExternalIP();
        }

    private void ButtonInternalIP_Click(object sender, EventArgs e)
        {
            this.textBoxIP.Text = GetIP();
        }
    #endregion
    #endregion
    #endregion
    }
}
