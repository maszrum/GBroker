namespace EventBroker.Grpc.ValueConverters
{
    public interface IValueConverter
    {
        object ToValue(byte[] data);
        byte[] ToBytes(object value);
    }
}
