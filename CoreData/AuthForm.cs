using System;
using System.IO;

namespace CoreData
{
    public class AuthForm : IAuthForm
    {
        public AuthForm() { }
        public AuthForm(string name)
        {
            Login = name;
            Session = new Random().Next().GetHashCode() + Login.GetHashCode();
        }
        public AuthForm(string name, int session)
        {
            Login = name;
            Session = session;
        }
        public static AuthForm FromBytes(byte[] buffer)
        {
            AuthForm result = new AuthForm();

            using (MemoryStream ms = new MemoryStream(buffer))
            {
                BinaryReader br = new BinaryReader(ms);
                result.Login = br.ReadString();
                result.Session = br.ReadInt32();
            }

            return result;
        }
        public byte[] ToBytes()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter bw = new BinaryWriter(ms);
                bw.Write(Login);
                bw.Write(Session);
                return ms.ToArray();
            }
        }
        public string Login { get; set; }
        public int Session { get; set; }
    }
}
