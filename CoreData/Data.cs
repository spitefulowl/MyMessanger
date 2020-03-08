using System.IO;
using System.Net.Sockets;

namespace CoreData
{
    public class Data : IData
    {
        public byte[] ToBytes()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter bw = new BinaryWriter(ms);
                bw.Write(Message);
                bw.Write(Name);
                bw.Write(Target);
                return ms.ToArray();
            }
        }
        public static Data FromBytes(byte[] buffer)
        {
            Data result = new Data();

            using (MemoryStream stream = new MemoryStream(buffer))
            {
                BinaryReader reader = new BinaryReader(stream);
                result.Message = reader.ReadString();
                result.Name = reader.ReadString();
                result.Target = reader.ReadString();
            }

            return result;
        }
        public string Message { get; set; }
        public string Name { get; set; }
        public string Target { get; set; }
    }
    public class StateObject
    {
        public Socket WorkSocket = null;
        public const int BufferSize = 1024;
        public byte[] Buffer = new byte[BufferSize];
    }
}
