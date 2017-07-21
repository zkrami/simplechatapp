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

namespace Chat__Client_
{
    public partial class Form1 : MetroForm
    {
        enum Command : int
        {
            Text = 0, Image

        }
        Client client;
       // Mutex mutex;
        public Form1()
        {
//
        //     mutex = new Mutex(true, "Client");
     //       if (!mutex.WaitOne())
      //          Environment.Exit(0);
            
            InitializeComponent();
            

            this.MinimumSize = this.Size; 
          
         
            client = new Client();

            client.RefusedEvent += client_RefusedEvent;
            client.ConnectedEvent += client_ConnectedEvent;
            client.RecievedEvent += client_RecievedEvent;
            client.RunTimeErrorEvent += client_RunTimeErrorEvent;
            client.DisconnectedEvent += client_DisconnectedEvent;
            new Thread(() =>
            {
                MessageBox.Show("asdsd");
                Thread.Sleep(1000);
                Thread.CurrentThread.Abort();
            });
        }

        

        void client_DisconnectedEvent()
        {
            MetroMessageBox.Show(this, "The Connection Lost", "Disconnected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
           

            Invoke((MethodInvoker)delegate
            {
                metroLabel1.Visible = false;

                metroTextBox2.Enabled = true;

                metroButton1.Enabled = true;
            });

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
                        listBox1.Items.Add("Rami : " + Encoding.Unicode.GetString(br.ReadBytes(br.ReadInt32())) + "           " + DateTime.Now.Hour+":"+DateTime.Now.Minute);
                        this.BringToFront();
                        
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

        void client_ConnectedEvent()
        {
            client.BeginRecive();
            string name = Environment.UserDomainName;
            byte[] ToSend = Encoding.Unicode.GetBytes(name);
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(2);
            bw.Write(ToSend.Length);
            bw.Write(ToSend);
            client.Send(ms.ToArray());
            MetroMessageBox.Show(this, "Connected To Server", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            metroLabel1.Visible = true;
            metroTextBox2.Enabled = false;
            metroButton1.Enabled = false;
            

        }

        void client_RefusedEvent()
        {
            MetroMessageBox.Show(this, "Can't Connect To Server","Refused",MessageBoxButtons.OK,MessageBoxIcon.Warning);

        }

      
        private void metroButton2_Click(object sender, EventArgs e)
        {

        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(metroTextBox1.Text)) return;
            if (!client.Connected) return;

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

        


        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

       

        private void metroTextBox2_Click(object sender, EventArgs e)
        {

        }

        private void metroTextBox1_Click(object sender, EventArgs e)
        {

        }

        private void metroButton1_Click_2(object sender, EventArgs e)
        {
            client.Connect(metroTextBox2.Text);
        }

        private void metroTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                metroButton1_Click(sender, e);
                e.Handled = e.SuppressKeyPress = true;
            }
        }

        private void Form1_Resize_1(object sender, EventArgs e)
        {
            metroTextBox1.Location = new Point(metroTextBox1.Location.X, 115 + listBox1.Size.Height);
        }
    }
}
