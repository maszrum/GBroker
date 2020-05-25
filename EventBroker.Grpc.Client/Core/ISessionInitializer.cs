using System.Threading.Tasks;

namespace EventBroker.Grpc.Client.Core
{
    internal interface ISessionInitializer
    {
        void Initialize();
        Task InitializeAsync();
    }
}
