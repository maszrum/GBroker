using System;

namespace EventBroker.Grpc
{
    public static class GuidConverter
    {
        public static Guid Parse(string input)
        {
            return Guid.ParseExact(input, "N");
        }

        public static string ToString(Guid guid)
        {
            return guid.ToString("N");
        }
    }
}
