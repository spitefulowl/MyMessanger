using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using CoreData;

namespace CoreClient
{
    public class MessageClient
    {
        public MessageClient(IPAddress addr, int port, AuthForm auth)
        {
            Auth = false;
            IPPoint = new IPEndPoint(addr, port);
            authForm = auth;
            Items = new ConcurrentQueue<Data>();
            MySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        private void Receive()
        {
            try
            {
                StateObject state = new StateObject();
                state.WorkSocket = MySocket;
                MySocket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                Socket socket = state.WorkSocket;

                int bytesRead = socket.EndReceive(ar);
                Data incoming_data = Data.FromBytes(state.Buffer);
                Items.Enqueue(incoming_data);
                bool is_valid = socket.Poll(1000, SelectMode.SelectRead);
                bool has_data = (socket.Available == 0);
                if (is_valid & has_data)
                {
                    return;
                }
                socket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public bool Authorize()
        {
            MySocket.Connect(IPPoint);
            MySocket.Send(authForm.ToBytes());
            Auth = true;
            return true;
        }
        public void StartReceiving()
        {
            if (Auth)
            {
                Receive();
            }
        }
        public void SendMessage(Data packed_message)
        {
            if (Auth)
            {
                MySocket.Send(packed_message.ToBytes());
            }
        }
        public int GetSession()
        {
            return authForm.Session;
        }
        public string GetUsername()
        {
            return authForm.Login;
        }
        private bool Auth { get; set; }
        private IPEndPoint IPPoint;
        private AuthForm authForm;
        private Socket MySocket;
        public ConcurrentQueue<Data> Items;
    }
}
