/*
 * Projekt: Schiffeversenken Pirat Edition
 * Klasse: HostGameForm
 * Beschreibung:
 * Autor: Markus Bohnert
 * Team: Simon Hodler, Markus Bohnert
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace Battleships
{
    public partial class HostGameForm : FormDoubleBuffered
    {
        delegate void SetTextCallback(string text);
        delegate void SetTextMainFormCallback(string text);
        delegate void UpdateControlsCallback(bool listening);

        public AsyncCallback pfnWorkerCallBack;
        private Socket m_mainSocket;
        public Socket m_workerSocket;
        public int m_clientCount = 0;

        private int roll;
        private string oroll;

        public HostGameForm()
        {
            InitializeComponent();
            textBoxIP.Text = GetIP();
        }

        private void btnHostGame_Click(object sender, EventArgs e)
        {
            try
            {
                // Check the port value
                if (textBoxPort.Text == "")
                {
                    MessageBox.Show("Bitte geben Sie einen Port an");
                    return;
                }
                    string portStr = textBoxPort.Text;
                    int port = System.Convert.ToInt32(portStr);
                    // Create the listening socket...
                    m_mainSocket = new Socket(AddressFamily.InterNetwork,
                                              SocketType.Stream,
                                              ProtocolType.Tcp);
                    IPEndPoint ipLocal = new IPEndPoint(IPAddress.Any, port);
                    // Bind to local IP Address...
                    m_mainSocket.Bind(ipLocal);
                    // Start listening...
                    listboxMessage.Items.Add("Initialize Server...");
                    m_mainSocket.Listen(1);
                    listboxMessage.Items.Add("Server initialized");
                    // Create the call back for any client connections...
                    m_mainSocket.BeginAccept(new AsyncCallback(OnClientConnect), null);

                    UpdateControls(true);
                    listboxMessage.Items.Add("Waiting for Client...");   
            }
            catch (SocketException se)
            {
                SetText(se.Message);
            }
        }

        public void OnClientConnect(IAsyncResult asyn)
        {
            try
            {
                if (m_clientCount < 1)
                {
                    // Here we complete/end the BeginAccept() asynchronous call
                    // by calling EndAccept() - which returns the reference to
                    // a new Socket object
                    m_workerSocket = m_mainSocket.EndAccept(asyn);
                    // Let the worker Socket do the further processing for the 
                    // just connected client
                    WaitForData(m_workerSocket);
                    // Display this client connection as a status message on the GUI	
                    String str = String.Format("Client at {0} connected", m_workerSocket.RemoteEndPoint.ToString());
                    //textBoxMsg.Text = str;
                    SetText(str);
                    SetTextLblStatus(str);
                    // Ack zum Client schicken (Verbindungsbestätigung)
                    Object objData = "ACK";
                    byte[] byData = System.Text.Encoding.ASCII.GetBytes(objData.ToString());
                    if (m_workerSocket != null)
                    {
                        if (m_workerSocket.Connected)
                        {
                            m_workerSocket.Send(byData);
                        }
                    }
                   
                    // Now increment the client count
                    ++m_clientCount;
                    // Since the main Socket is now free, it can go back and wait for
                    // other clients who are attempting to connect
                    //m_mainSocket.BeginAccept(new AsyncCallback(OnClientConnect), null);
                }
                else
                {
                    Socket tmp_workerSocket = m_mainSocket.EndAccept(asyn);
                    WaitForData(tmp_workerSocket);
                    SetText("Client at: " + tmp_workerSocket.RemoteEndPoint.ToString() + " tried to connect to Server");
                    SetText("But Server is already full!");
                    SetTextLblStatus("Client at: " + tmp_workerSocket.RemoteEndPoint.ToString() + " tried to connect to Server");
                    SetTextLblStatus("But Server is already full!");
                    Object objData = "FULL";
                    byte[] byData = System.Text.Encoding.ASCII.GetBytes(objData.ToString());
                    tmp_workerSocket.Send(byData);
                    tmp_workerSocket.Close();
                }
            }
            catch (ObjectDisposedException)
            {
                SetText("OnClientConnection: Socket has been closed\n");
            }
            catch (SocketException se)
            {
                SetText(se.Message);
            }

        }

        // Start waiting for data from the client
        public void WaitForData(System.Net.Sockets.Socket soc)
        {
            try
            {
                if (pfnWorkerCallBack == null)
                {
                    // Specify the call back function which is to be 
                    // invoked when there is any write activity by the 
                    // connected client
                    pfnWorkerCallBack = new AsyncCallback(OnDataReceived);
                }
                SocketPacket theSocPkt = new SocketPacket();
                theSocPkt.m_currentSocket = soc;
                // Start receiving any data written by the connected client
                // asynchronously
                soc.BeginReceive(theSocPkt.dataBuffer, 0,
                                   theSocPkt.dataBuffer.Length,
                                   SocketFlags.None,
                                   pfnWorkerCallBack,
                                   theSocPkt);
            }
            catch (SocketException se)
            {
                SetText(se.Message);
                --m_clientCount;     
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
                iRx = socketData.m_currentSocket.EndReceive(asyn);
                char[] chars = new char[iRx + 1];
                Decoder d = Encoding.UTF8.GetDecoder();
                int charLen = d.GetChars(socketData.dataBuffer,
                                         0, iRx, chars, 0);
                System.String data = new System.String(chars);
                //richTextBoxReceivedMsg.AppendText(data);
                //SetTextRichTextBox(data);

                // Empfangene Daten verarbeiten und entsperchenden Service auswählen
                Services(data);

                // Continue the waiting for data on the Socket
                WaitForData(socketData.m_currentSocket);
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode == SocketError.ConnectionReset)
                {
                    SetText("Client at " + m_workerSocket.RemoteEndPoint.ToString() + " disconnected!\n");
                    try
                    {
                        m_workerSocket.Close();
                    }
                    catch (Exception)
                    {
                    }
                    m_workerSocket = null;
                    m_clientCount--;
                }
                //MessageBox.Show(se.Message);
            }
            catch (Exception ex)
            {
                SetText("OnDataReceived: Socket has been closed Socket error: " +ex.Message.ToString());
                CloseSockets();
            }
        }

        /// <summary>
        /// Verarbeitet die empfangenen Daten und führt die entsprechende Operation aus
        /// </summary>
        /// <param name="data">Empfangen Daten vom anderen Spieler</param>
        private void Services(string data)
        {
            // Koordinaten vom Gegner erhalten (Auswerten ob an den Koords ein Schiff gesetzt ist oder nicht)
            if (data.Contains("pb_"))
            {
                // Erhaltene koordinaten ausgeben
                SetText("Coords received: " + data);

                // X und Y aus der Nachricht lesen
                string pos = data.Remove(0, 3);
                string[] posData = pos.Split(':');
                int x = int.Parse(posData[0]);
                int y = int.Parse(posData[1]);

                Object objData;
                // Auswerten ob der Gegner getroffen hat oder nicht
                if (!BattleshipsForm.battlefieldPlayer.hitOrMiss(x,y))
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
                if (m_workerSocket.Connected) m_workerSocket.Send(byData);

                // Wenn Gegner gewonnen hat, dann darf Spieler nicht mehr zum Zug kommen...
                if (objData.ToString().StartsWith("WIN"))
                {
                    // ToDo: Sound abspielen (Losersound...)
                    MessageBox.Show(this, "Loser!", "lose", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else
                {
                    // Spieler ist an der Reihe
                    BattleshipsForm.whosTurn = BattleshipsForm.spielzug.player;
                    SetTextLblStatus("Du bist am Zug!");
                }
            }
            // Du hast gewonnen!!
            else if (data.StartsWith("WIN"))
            {
                // ToDo: Sound abspielen (Gewinnersound....)
                MessageBox.Show(this, "Du hast gewonnen!", "Sieg!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Application.Exit();
            }
            // Du hast einen treffer gelandet!
            else if (data.StartsWith("HIT"))
            {
                string pos = data.Remove(0, 3);
                string[] posData = pos.Split(':');
                int x = int.Parse(posData[0]);
                int y = int.Parse(posData[1]);
                SetText("HIT received at x:" + x.ToString() + " y:" + y.ToString());
                // Dem Spieler anzeigen, dass er getroffen hat (Auf dem Gegnerfeld)
                BattleshipsForm.battlefieldOpponent.setImpact(x, y);
                // Gegner ist an der Reihe
                BattleshipsForm.whosTurn = BattleshipsForm.spielzug.opponend;
                SetTextLblStatus("Gegner ist am Zug!");
            }
            // Der Schuss ging leider daneben, versuchs nochmal!
            else if (data.StartsWith("MISS"))
            {
                string pos = data.Remove(0, 4);
                string[] posData = pos.Split(':');
                int x = int.Parse(posData[0]);
                int y = int.Parse(posData[1]);
                SetText("MISS received at x:" + x.ToString() + " y:" + y.ToString());
                // Dem Spieler anzeigen, dass er nicht getroffen hat (Auf dem Gegnerfeld)
                BattleshipsForm.battlefieldOpponent.setMiss(x, y);
                // Gegner ist an der Reihe
                BattleshipsForm.whosTurn = BattleshipsForm.spielzug.opponend;
                SetTextLblStatus("Gegner ist am Zug!");
            }
            else if (data.StartsWith("RDY_"))
            {
                oroll = data.Remove(0, 4);
                BattleshipsForm.opponendReady2Play = true;
                SetTextLblStatus("Opponend is ready and rolled: " + oroll);

                // Prüfen ob Spieler auch bereit ist?
                if (BattleshipsForm.playerReady2Play)
                {
                    if (int.Parse(oroll) < roll)
                    {
                        SetTextLblStatus("Opponend rolled: " + oroll + " you rolled: " + roll.ToString());
                        SetTextLblStatus("Du darfst anfangen!");
                        BattleshipsForm.whosTurn = BattleshipsForm.spielzug.player;
                    }
                    else if (int.Parse(oroll) > roll)
                    {
                        SetTextLblStatus("Opponend rolled: " + oroll + " you rolled: " + roll.ToString());
                        SetTextLblStatus("Gegner darf anfangen!");
                        BattleshipsForm.whosTurn = BattleshipsForm.spielzug.opponend;
                    }
                }
                // Wenn nicht, dann abwarten bis Spieler bereit ist
                else
                {
                    // Warten bis Spieler bereit ist (Spieler ist bereit, wenn er auf den Bereit-Button gedrückt hat - Siehe Event btnRdy.clicked)
                }
            }
            else
            {
                SetText(data);
            }
        }

        private void btnRdy_Click(object sender, EventArgs e)
        {
            if (BattleshipsForm.zaehler_battleship >= 1 && BattleshipsForm.zaehler_galley >= 1 && BattleshipsForm.zaehler_cruiser >= 3 && BattleshipsForm.zaehler_boat >= 3)
            {
                if (m_workerSocket != null)
                {
                    if (m_workerSocket.Connected)
                    {
                        BattleshipsForm.playerReady2Play = true;
                        btnRdy.Enabled = false;

                        Random rnd = new Random();
                        roll = rnd.Next(101);
                        Object objData = "RDY_" + roll.ToString();
                        byte[] byData = System.Text.Encoding.ASCII.GetBytes(objData.ToString());
                        // Antwort an Gegner schicken
                        if (m_workerSocket.Connected) m_workerSocket.Send(byData);

                        SetTextLblStatus("You rolled: " + roll.ToString());

                        if (BattleshipsForm.opponendReady2Play)
                        {
                            if (int.Parse(oroll) < roll)
                            {
                                SetTextLblStatus("Opponend rolled: " + oroll + " you rolled: " + roll.ToString());
                                SetTextLblStatus("Du darfst anfangen");
                                BattleshipsForm.whosTurn = BattleshipsForm.spielzug.player;
                            }
                            else if (int.Parse(oroll) > roll)
                            {
                                SetTextLblStatus("Opponend rolled: " + oroll + " you rolled: " + roll.ToString());
                                SetTextLblStatus("Gegner darf anfangen");
                                BattleshipsForm.whosTurn = BattleshipsForm.spielzug.opponend;
                            }
                        }
                        else
                        {
                            // Warten bis Gegner bereit ist (Gegner ist Bereit wenn "RDY"-Flag ankommt)
                        }
                    }
                    else
                        MessageBox.Show(this, "Es muss erst ein Gegenspieler verbunden sein!", "Gegenspieler benötigt", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                    MessageBox.Show(this, "Es muss erst ein Gegenspieler verbunden sein!", "Gegenspieler benötigt", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            else
                MessageBox.Show(this, "Es müssen erst alle Schiffe verteilt werden!", "Nocht nicht bereit!", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Ermittelt die interne IP-Adresse des PCs
        /// </summary>
        /// <returns>interne IP-Adresse (String)</returns>
        private String GetIP()
        {
            String strHostName = Dns.GetHostName();

            // Find host by name
            IPHostEntry iphostentry = Dns.GetHostEntry(strHostName);//Dns.GetHostByName(strHostName);

            // Grab the first IP addresses
            String IPStr = "";
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

        /// <summary>
        /// Schickt einen Request an die Seite "http://checkip.dyndns.org/" und wertet den erhaltenen Response (Externe IP-Adresse) aus
        /// </summary>
        /// <returns>Externe IP-Adresse (String)</returns>
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
            s = s.Replace("<html><head><title>Current IP Check</title></head><body>Current IP Address:", "").Replace("</body></html>", "").ToString();
            return s;
        }

        public void SetText(string text)
        {
            if (this.listboxMessage.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.listboxMessage.Items.Add(text);
                this.listboxMessage.SelectedIndex = listboxMessage.Items.Count-1;
            }
        }

        public void SetTextLblStatus(string text)
        {
            if (BattleshipsForm.lblStatus.InvokeRequired)
            {
                SetTextMainFormCallback d = new SetTextMainFormCallback(SetTextLblStatus);
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

        private void UpdateControls(bool listening)
        {
            if (btnCloseGame.InvokeRequired)
            {
                UpdateControlsCallback d = new UpdateControlsCallback(UpdateControls);
                this.Invoke(d, new object[] { listening });
            }
            else
            {
                btnHostGame.Enabled = !listening;
                btnCloseGame.Enabled = listening;
                btnRdy.Enabled = listening;
            }
            
        }

        private void btnCloseGame_Click(object sender, EventArgs e)
        {
            CloseSockets();
            UpdateControls(false);
        }

        void CloseSockets()
        {
            if (m_mainSocket != null)
            {
                m_mainSocket.Close();
            }
            if (m_workerSocket != null)
            {
                m_workerSocket.Close(1);
                m_workerSocket = null;
            }
            BattleshipsForm.playerReady2Play = false;
            BattleshipsForm.opponendReady2Play = false;
            SetText("Server closed!");
            SetTextLblStatus("Server closed!");
        }

        private void HostGameForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            CloseSockets();
            BattleshipsForm.spielBeitretenMenuItem.Enabled = true;
        }

        private void btnExtIp_Click(object sender, EventArgs e)
        {
            textBoxIP.Text = GetExternalIP();
        }

        private void btnInternIP_Click(object sender, EventArgs e)
        {
            textBoxIP.Text = GetIP();
        }
    }
}
