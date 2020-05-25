namespace EventBroker.Grpc.Client.EventToData
{
    internal class PropertyEventData
    {
        public string PropertyName { get; set; }
        public byte[] Bytes { get; set; }
        public int Position { get; set; }
    }
}
