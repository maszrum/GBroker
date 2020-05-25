namespace EventBroker.Grpc.Client.Core
{
    internal interface ISessionProvider
    {
        string GetSessionId();
    }
}
