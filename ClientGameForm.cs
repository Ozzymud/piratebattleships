/*
 * Projekt: Schiffeversenken Pirat Edition
 * Klasse: ConnectGameForm
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

namespace Battleships
{
    public partial class ClientGameForm : Battleships.DoubleBufferedForm
    {
        byte[] m_dataBuffer = new byte[10];
        IAsyncResult m_result;
        public AsyncCallback m_pfnCallBack;
        public Socket m_clientSocket;

        private int roll;
        private string oroll;

        delegate void SetRichTextBoxRxCallback(string text);
        delegate void SetTextMainFormCallback(string text);
        delegate void UpdateControlsCallback(bool listening);

        public ClientGameForm()
        {
            InitializeComponent();
            textboxIP.Text = GetIP();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            // See if we have text on the IP and Port text fields
            if (textboxIP.Text == "" || textboxPort.Text == "")
            {
                MessageBox.Show("IP Address and Port Number are required to connect to the Server");
                return;
            }
            try
            {
                UpdateControls(false);
                // Create the socket instance
                m_clientSocket = new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp);

                // Cet the remote IP address
                IPAddress ip = IPAddress.Parse(textboxIP.Text);
                int iPortNo = System.Convert.ToInt16(textboxPort.Text);
                // Create the end point 
                IPEndPoint ipEnd = new IPEndPoint(ip, iPortNo);
                // Connect to the remote host
                m_clientSocket.Connect(ipEnd);
                if (m_clientSocket.Connected)
                {
                    listboxRx.Items.Add("Connected to Server...");
                    UpdateControls(true);
                    //Wait for data asynchronously 
                    WaitForData();
                }
            }
            catch (SocketException se)
            {
                string str;
                str = "\nConnection failed, is the server running?\n" + se.Message;
                SetText(str);
                //MessageBox.Show(str);
                UpdateControls(false);
            }
        }

        public void WaitForData()
        {
            try
            {
                if (m_pfnCallBack == null)
                {
                    m_pfnCallBack = new AsyncCallback(OnDataReceived);
                }
                SocketPacket theSocPkt = new SocketPacket();
                theSocPkt.m_currentSocket = m_clientSocket;
                // Start listening to the data asynchronously
                m_result = m_clientSocket.BeginReceive(theSocPkt.dataBuffer,
                                                        0, theSocPkt.dataBuffer.Length,
                                                        SocketFlags.None,
                                                        m_pfnCallBack,
                                                        theSocPkt);
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode == SocketError.ConnectionReset)
                {
                    SetText("Server closed!\n");
                    SetTextLblStatus("Server closed!");
                    m_clientSocket.Close();
                    UpdateControls(false);
                }
            }
            catch (Exception ex)
            {
                SetText("WaitForData: Socket has been closed. Socket error: " + ex.Message.ToString());
                CloseSocket();
                UpdateControls(false);
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
                String data = new String(chars);

                // Ankommende Daten verarbeiten und Service auswählen
                if (Services(data))
                {
                    // Auf ankommende Daten warten
                    WaitForData();
                }
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode == SocketError.ConnectionReset)
                {
                    SetText("Server closed!\n");
                    SetTextLblStatus("Server closed!");
                    UpdateControls(false);
                }
                //MessageBox.Show(se.Message);
            }
            catch (Exception ex)
            {
                SetText("OnDataReceived: Socket has been closed Socket error: " + ex.Message.ToString());
                CloseSocket();
            }
        }

        /// <summary>
        /// Wertet die Nachricht vom Server aus und entscheidet welche entsprechende Operation ausgeführt werden soll
        /// </summary>
        /// <param name="data">Nachricht vom Server</param>
        /// <returns>False wenn Verbindung geschlossen werden soll (z.B. bei vollem Server)</returns>
        private bool Services(String data)
        {
            // Server hat Verbindung bestätigt
            if (data.StartsWith("ACK"))
            {
                SetTextLblStatus("Server hat Verbindung bestätigt (ACK)");
            }
            // Server ist voll!
            else if (data.StartsWith("FULL"))
            {
                SetText("Server is full...closing connection!");
                if (m_clientSocket != null)
                {
                    m_clientSocket.Close();
                    m_clientSocket = null;
                    UpdateControls(false);
                    SetText("Disconnected from Server!");
                }
                return false;
            }
            // Koordinaten vom Gegner erhalten (Auswerten ob an den Koords ein Schiff gesetzt ist oder nicht)
            else if (data.Contains("pb_"))
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
                if (m_clientSocket.Connected) m_clientSocket.Send(byData);

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
            //  Du hast einen treffer gelandet!
            else if (data.StartsWith("HIT"))
            {
                string pos = data.Remove(0, 3);
                string[] posData = pos.Split(':');
                int x = int.Parse(posData[0]);
                int y = int.Parse(posData[1]);
                SetText("HIT received at x:" +x.ToString() + " y:" + y.ToString());
                // Dem Spieler Anzeigen, dass er getroffen hat (Auf dem Gegnerfeld)
                BattleshipsForm.battlefieldOpponent.setImpact(x, y);
                // Gegner ist an der Reihe
                BattleshipsForm.whosTurn = BattleshipsForm.spielzug.opponent;
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
                BattleshipsForm.whosTurn = BattleshipsForm.spielzug.opponent;
                SetTextLblStatus("Gegner ist am Zug!");
            }
            else if (data.StartsWith("RDY_"))
            {
                oroll = data.Remove(0, 4);
                BattleshipsForm.opponentReady2Play = true;
                SetTextLblStatus("Opponend is ready and rolled: " + oroll);

                // Prüfen ob Spieler auch bereit ist?
                if (BattleshipsForm.playerReady2Play)
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
                        BattleshipsForm.whosTurn = BattleshipsForm.spielzug.opponent;
                    }
                }
                // Wenn nicht, dann abwarten bis Spieler bereit ist
                else
                {
                    // Warten bis Spieler bereit ist (Spieler ist bereit, wenn er auf den Bereit-Button gedrückt hat - Siehe Event btnRdy.clicked)
                }
            }
            // Sonstige Meldungen
            else
            {
                SetText(data);
            }
            return true;
        }

        private void btnRdy_Click(object sender, EventArgs e)
        {
            // Überprüfen ob alle Schiffe verteilt wurden
            if (BattleshipsForm.zaehler_battleship >= 1 && BattleshipsForm.zaehler_galley >= 1 && BattleshipsForm.zaehler_cruiser >= 3 && BattleshipsForm.zaehler_boat >= 3)
            {
                if (m_clientSocket != null)
                {
                    if (m_clientSocket.Connected)
                    {
                        BattleshipsForm.playerReady2Play = true;
                        btnRdy.Enabled = false;

                        Random rnd = new Random();
                        roll = rnd.Next(101);
                        Object objData = "RDY_" + roll.ToString();
                        byte[] byData = System.Text.Encoding.ASCII.GetBytes(objData.ToString());
                        // Antwort an Gegner schicken
                        if (m_clientSocket.Connected) m_clientSocket.Send(byData);
                        SetTextLblStatus("You rolled: " + roll.ToString());

                        if (BattleshipsForm.opponentReady2Play)
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
                                BattleshipsForm.whosTurn = BattleshipsForm.spielzug.opponent;
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

        public void SetText(string text)
        {
            if (listboxRx.InvokeRequired)
            {
                SetRichTextBoxRxCallback d = new SetRichTextBoxRxCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                listboxRx.Items.Add(text);
                listboxRx.SelectedIndex = listboxRx.Items.Count - 1;
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

        private void UpdateControls(bool status)
        {
            if (btnConnect.InvokeRequired)
            {
                UpdateControlsCallback d = new UpdateControlsCallback(UpdateControls);
                this.Invoke(d, new object[] { status });
            }
            else
            {
                btnConnect.Enabled = !status;
                btnDisconnect.Enabled = status;
                btnRdy.Enabled = status;
            }
            
        }

        void CloseSocket()
        {
            if (m_clientSocket != null)
            {
                m_clientSocket.Close();
                m_clientSocket = null;
            }
            BattleshipsForm.playerReady2Play = false;
            BattleshipsForm.opponentReady2Play = false;
            SetText("Server closed!");
            SetTextLblStatus("Server closed!");
            UpdateControls(false);
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            CloseSocket();         
        }

        private void ClientGameForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            CloseSocket();
            BattleshipsForm.spielHostenMenuItem.Enabled = true;
        }
    }
}
