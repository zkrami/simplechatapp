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
    struct BufferRecive
    {
        public int ToRecieve;
        const int BufferSize = 1024;
        public byte[] DataBuffer;
        public MemoryStream RecieveMemory;
        public BufferRecive(int toReceive)
        {
            ToRecieve = toReceive;
            DataBuffer = new byte[BufferSize];
            RecieveMemory = new MemoryStream(toReceive);

        }

    }
    public class Client
    {
        Socket sck;
        public delegate void RecievedHandler(Client sender, MemoryStream data);
        public event RecievedHandler RecievedEvent;
        public delegate void DisconnectedHandler(Client sender);
        public event DisconnectedHandler DisconnectedEvent;
        public delegate void RunTimeErrorHandler(Exception ex);
        public event RunTimeErrorHandler RunTimeErrorEvent;
        public bool Running;
        static int LastID;
   
        byte[] Lengthbuffer;
        BufferRecive Buffer;
        public IPEndPoint EndPoint
        {
            get;
            private set;
        }
        public int ID
        {
            get;
            private set;
        }
        public bool Connected
        {
            get
            {
                if (sck != null) return sck.Connected;
                return false;
            }
        }
        public void Dispose()
        {
            sck.Close();
            sck.Dispose();
            

        }
        public Client Clone()
        {

            return (Client)this.MemberwiseClone();
        }
        public Client(Socket s)
        {
            sck = s;
            EndPoint = (IPEndPoint)sck.RemoteEndPoint;
            ID = LastID++;
            Lengthbuffer = new byte[4];
            Running = false;
           
        }
        public void BeginRecive()
        {
            if (!Connected) return;
            sck.BeginReceive(Lengthbuffer, 0, 4, SocketFlags.None, RecieveLengthCallBack, 0);

        }
        public void Disconnect()
        {
            sck.Dispose();
            sck.Close();
            if (DisconnectedEvent != null) DisconnectedEvent(this);

        }
        void RecieveLengthCallBack(IAsyncResult ar)
        {
            try
            {
                int rec = sck.EndReceive(ar);
                if (rec < 0 || rec != 4) throw new Exception("DISCONNECT");
                Buffer = new BufferRecive(BitConverter.ToInt32(Lengthbuffer, 0));
                sck.BeginReceive(Buffer.DataBuffer, 0, Buffer.DataBuffer.Length, SocketFlags.None, RecievePacketCallBack, 0);
            }
            catch (SocketException ex)
            {
                switch ((SocketError)ex.ErrorCode)
                {
                    case SocketError.Disconnecting:
                        Disconnect();
                        break;
                    case SocketError.ConnectionAborted:
                        Disconnect();
                        break;
                    case SocketError.ConnectionReset:
                        Disconnect();
                        break;


                    default:
                        Disconnect();
                        if (RunTimeErrorEvent != null) RunTimeErrorEvent(ex);
                        break;

                }

            }
            catch (Exception ex)
            {

                if (ex.Message == "DISCONNECT")
                {

                    Disconnect();
                    return;
                }
                Disconnect();
                if (RunTimeErrorEvent != null) RunTimeErrorEvent(ex);
                Console.WriteLine(ex.Message);
            }


        }
        void RecievePacketCallBack(IAsyncResult ar)
        {
            try
            {
                int rec = sck.EndReceive(ar);
                if (rec < 0) throw new Exception("DISCONNECT");
                Buffer.ToRecieve -= rec;
                Buffer.RecieveMemory.Write(Buffer.DataBuffer, 0, rec);
                if (Buffer.ToRecieve > 0)
                {
                    Array.Clear(Buffer.DataBuffer, 0, Buffer.DataBuffer.Length);
                    sck.BeginReceive(Buffer.DataBuffer, 0, Buffer.DataBuffer.Length, SocketFlags.None, RecievePacketCallBack, 0);
                    return;
                }

            }
            catch (SocketException ex)
            {
                switch ((SocketError)ex.ErrorCode)
                {
                    case SocketError.Disconnecting:
                        Disconnect();
                        break;
                    case SocketError.ConnectionAborted:
                        Disconnect();
                        break;
                    case SocketError.ConnectionReset:
                        Disconnect();
                        break;


                    default:
                        Disconnect();
                        if (RunTimeErrorEvent != null) RunTimeErrorEvent(ex);
                        break;

                }

            }
            catch (Exception ex)
            {

                if (ex.Message == "DISCONNECT")
                {

                    Disconnect();
                    return;
                }
                Disconnect();
                Console.WriteLine(ex.Message);
                if (RunTimeErrorEvent != null) RunTimeErrorEvent(ex);
            }

            Buffer.RecieveMemory.Position = 0;
            if (RecievedEvent != null) RecievedEvent(this, Buffer.RecieveMemory);
            Array.Clear(Lengthbuffer, 0, Lengthbuffer.Length);
            sck.BeginReceive(Lengthbuffer, 0, 4, SocketFlags.None, RecieveLengthCallBack, 0);
        }
        public void Send(byte[] data)
        {
            try
            {
                sck.BeginSend(BitConverter.GetBytes(data.Length), 0, 4, SocketFlags.None, SendCallBack, 0);
                sck.BeginSend(data, 0, data.Length, SocketFlags.None, SendCallBack, 0);
            }
            catch (SocketException ex)
            {
                switch ((SocketError)ex.ErrorCode)
                {
                    case SocketError.Disconnecting:
                        Disconnect();
                        break;
                    case SocketError.ConnectionAborted:
                        Disconnect();
                        break;
                    case SocketError.ConnectionReset:
                        Disconnect();
                        break;


                    default:

                        if (RunTimeErrorEvent != null) RunTimeErrorEvent(ex);
                        Disconnect();
                        break;

                }

            }
            catch (Exception ex)
            {
                Disconnect();
                Console.WriteLine(ex.Message);
                if (RunTimeErrorEvent != null) RunTimeErrorEvent(ex);
            }
        }
        void SendCallBack(IAsyncResult ar)
        {
            try
            {
                sck.EndSend(ar);

            }
            catch (SocketException ex)
            {
                switch ((SocketError)ex.ErrorCode)
                {
                    case SocketError.Disconnecting:
                        Disconnect();
                        break;
                    case SocketError.ConnectionAborted:
                        Disconnect();
                        break;
                    case SocketError.ConnectionReset:

                        Disconnect();
                        break;


                    default:
                        Disconnect();
                        if (RunTimeErrorEvent != null) RunTimeErrorEvent(ex);
                        break;

                }
            }

            catch (Exception ex)
            {
                Disconnect();
                Console.WriteLine(ex.Message);
                if (RunTimeErrorEvent != null) RunTimeErrorEvent(ex);
            }
        }



    }
}
