using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MetroFramework.Forms;
using System.IO;
using MetroFramework;
using System.Threading;
using System.Net.NetworkInformation;
using System.Diagnostics;
namespace Chat__Server_
{
    public partial class Form1 : MetroForm
    {
        enum Command : int
        {
            Text = 0 , Image 
        }
        Listener listner;
        Client client;
        Mutex mutex;
        public Form1()
        {

           
            mutex = new Mutex(true, "Server");
            if (!mutex.WaitOne())
                Environment.Exit(0);
            
            InitializeComponent();
            listner = new Listener(9);
            listner.AcceptedEvent += listner_AcceptedEvent;
            listner.Start();


            
        
        }

        void listner_AcceptedEvent(System.Net.Sockets.Socket s)
        {
            client = new Client(s);
            client.DisconnectedEvent += client_DisconnectedEvent;
            client.RecievedEvent += client_RecievedEvent;
            client.RunTimeErrorEvent += client_RunTimeErrorEvent;
            Invoke((MethodInvoker)delegate
            {
                metroLabel1.Visible = true;
            });
            MetroMessageBox.Show(this,"The Client Has Connected","Success",MessageBoxButtons.OK,MessageBoxIcon.Information);
            client.BeginRecive();
        }

        void client_RunTimeErrorEvent(Exception ex)
        {
            MetroMessageBox.Show(this, ex.Message);
        }

        void client_RecievedEvent(Client sender, System.IO.MemoryStream data)
        {   
            
            BinaryReader br = new BinaryReader(data);
            Command Header = (Command)br.ReadInt32();
            switch (Header)
            {
                case Command.Text:
                    Invoke((MethodInvoker)delegate
                    {
                        listBox1.Items.Add("Rahaf : " + Encoding.Unicode.GetString(br.ReadBytes(br.ReadInt32())) + "           " + DateTime.Now.Hour+":"+DateTime.Now.Minute);
                        listBox1.SetSelected(listBox1.Items.Count - 1, true);
                            if (WindowState == FormWindowState.Minimized)
                            {

                                new Thread(() =>
                                {
                                    System.Media.SoundPlayer player = new System.Media.SoundPlayer(Environment.CurrentDirectory + @"\incoming_bg.wav");
                                    player.Play();
                                }).Start();
                            }
                            else
                            {
                                new Thread(() =>
                                {
                                    System.Media.SoundPlayer player = new System.Media.SoundPlayer(Environment.CurrentDirectory + @"\incoming_fg.wav");
                                    player.Play();
                                }).Start();
                            }
                       
                    });
                    

                    break;

            }
        }

        void client_DisconnectedEvent(Client sender)
        {
            
            MetroMessageBox.Show(this,"The Client Has Disconnected","Warring",MessageBoxButtons.OK,MessageBoxIcon.Warning);
            Invoke((MethodInvoker)delegate
            {
                metroLabel1.Visible = false;
            });
            client.Dispose();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(metroTextBox1.Text)) return;
            if ( client==null || !client.Connected) return;
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write((int)Command.Text);
            byte[] TextByte = Encoding.Unicode.GetBytes(metroTextBox1.Text);
            bw.Write(TextByte.Length);
            bw.Write(TextByte);
            client.Send(ms.ToArray());
            listBox1.Items.Add("Me : " + metroTextBox1.Text + "           " + DateTime.Now.Hour + ":" + DateTime.Now.Minute);
            listBox1.SetSelected(listBox1.Items.Count - 1, true);
            metroTextBox1.Text = "";

            new Thread(() =>
            {
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(Environment.CurrentDirectory + @"\outgoing_fg.wav");
                player.Play();
            }).Start();
        }

       
        private void metroTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                metroButton1_Click(sender, e);
                e.Handled = e.SuppressKeyPress = true;
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            metroTextBox1.Location = new Point(metroTextBox1.Location.X, 75 + listBox1.Size.Height);
            
        }
    }
}
