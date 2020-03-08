namespace CoreData
{
    public interface IAuthForm
    {
        bool Equals(object obj);
        byte[] ToBytes();
    }
}
