using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using CoreData;

namespace CoreServer
{
    public class MessageServer : IServer
    {
        public class Info
        {
            public Socket MySocket = null;
            public int Session = 0;
        }
        public MessageServer(IPAddress address, int port)
        {
            AcceptState = new ManualResetEvent(false);
            Users = new ConcurrentDictionary<string, Info>();
            IPPoint = new IPEndPoint(address, port);
            ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        private string Auth(Socket handler)
        {
            AuthForm auth_data = AuthForm.FromBytes(ReadBytes(handler));
            if (Users.ContainsKey(auth_data.Login))
            {
                if (Users[auth_data.Login].Session == auth_data.Session)
                {
                    Users[auth_data.Login].MySocket = handler;
                    return auth_data.Login;
                }
                else
                {
                    throw new Exception("Unknown user");
                }
            }
            else
            {
                Users[auth_data.Login] = new Info() { MySocket = handler, Session = auth_data.Session };
                return auth_data.Login;
            }
        }
        private byte[] ReadBytes(Socket handler)
        {
            List<byte> data_list = new List<byte>();
            byte[] data = new byte[256];
            data_list.Clear();
            do
            {
                handler.Receive(data);
                data_list.AddRange(data);
            }
            while (handler.Available > 0);
            return data_list.ToArray();
        }
        private void Send(Socket handler, Data data)
        {
            byte[] byteData = data.ToBytes();
            handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
        }
        private void SendCallback(IAsyncResult asyncResult)
        {
            try
            {
                Socket handler = (Socket)asyncResult.AsyncState;
                handler.EndSend(asyncResult);
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine(e.ToString());
#endif
            }
        }
        private void LoopReceiver(string login)
        {
            Socket handler = Users[login].MySocket;
            List<byte> data_list = new List<byte>();
            byte[] data = new byte[256];
            try
            {
                while (true)
                {
                    data_list.Clear();
                    do
                    {
                        handler.Receive(data);
                        data_list.AddRange(data);
                    }
                    while (handler.Available > 0);
                    Data incoming_data = Data.FromBytes(data_list.ToArray());
                    Send(Users[incoming_data.Target].MySocket, incoming_data);
                }
            }
            catch (Exception e)
            {
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
#if DEBUG
                Console.WriteLine(e.ToString());
#endif
            }
        }
        private void AcceptCallback(IAsyncResult asyncResult)
        {
            AcceptState.Set();
            Socket listener = (Socket)asyncResult.AsyncState;
            Socket handler = listener.EndAccept(asyncResult);
            string login = string.Empty;
            try
            {
                login = Auth(handler);
            }
            catch (Exception)
            {
                handler.Send(new Data() { Name = "Server", Message = "Go away!", Target = string.Empty }.ToBytes());
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
                return;
            }
            LoopReceiver(login);
        }
        public void Start()
        {
            try
            {
                ListenSocket.Bind(IPPoint);
                ListenSocket.Listen(10);
                Console.WriteLine("Started.");
                while (true)
                {
                    AcceptState.Reset();
                    Console.WriteLine("Waiting for a connection...");
                    ListenSocket.BeginAccept(new AsyncCallback(AcceptCallback), ListenSocket);
                    AcceptState.WaitOne();
                }
            }
            catch (Exception e)
            {
#if DEBUG
                Console.WriteLine(e.Message);
#endif
            }
        }
        private ManualResetEvent AcceptState;
        private IPEndPoint IPPoint;
        private Socket ListenSocket;
        private ConcurrentDictionary<string, Info> Users;
    }
}
