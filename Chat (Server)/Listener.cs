using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;
namespace Chat__Server_
{
    class Listener
    {
        Socket sck;
       public int Port {  get; private set; }
       public bool Listening {  get;  private set; }
        public Listener(int port)
        {
            Port = port;
            Listening = false;
            sck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
   
        public delegate void AcceptedHandler(Socket s);
        public event AcceptedHandler AcceptedEvent;
        void StartCallBack(IAsyncResult ar)
        {
            try
            {
                Socket sender = sck.EndAccept(ar);
                if (AcceptedEvent != null) AcceptedEvent(sender);
            }
            catch(Exception ex)
            {
                throw ex;
                Debug.WriteLine(ex.Message);
            }
            sck.BeginAccept(StartCallBack, 0);


        }
        public void Start()
        {
            if (Listening) return;
            Listening = true;
            sck.Bind(new IPEndPoint(0, Port));
            sck.Listen(0);
            sck.BeginAccept(StartCallBack, 0);    
        }
        public void Stop()
        {
            if (!Listening) return;
            Listening = false;
            sck.Dispose();
            sck.Close();
            if (sck.Connected) sck.Disconnect(false);
            sck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        }
        
    }
}
