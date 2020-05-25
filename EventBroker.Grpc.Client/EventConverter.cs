using System;
using System.Reflection;
using EventBroker.Grpc.Client.DataToEvent;
using EventBroker.Grpc.Client.EventToData;
using EventBroker.Grpc.Client.TypeResolver;
using EventBroker.Grpc.ValueConverters;

namespace EventBroker.Grpc.Client
{
    public static class EventConverter
    {
        private static readonly Lazy<PropertyValueConverter> Converter = new Lazy<PropertyValueConverter>(CreateParameterConverter);
        private static readonly Lazy<EventTypeResolver> EventTypeResolver = new Lazy<EventTypeResolver>();

        public static void RegisterEventsAssembly(Assembly assembly)
        {
            EventTypeResolver.Value.RegisterEventsAssembly(assembly);
        }

        public static DataToEventConverter DataToEvent()
        {
            return new DataToEventConverter(
                Converter.Value,
                EventTypeResolver.Value);
        }

        public static EventToDataConverter EventToData()
        {
            return new EventToDataConverter(
                Converter.Value,
                EventTypeResolver.Value);
        }

        private static PropertyValueConverter CreateParameterConverter()
        {
            return new PropertyValueConverter()
                .RegisterConverter<string>(new StringValueConverter())
                .RegisterConverter<int>(new IntegerValueConverter())
                .RegisterConverter<bool>(new BooleanValueConverter())
                .RegisterConverter<double>(new DoubleValueConverter())
                .RegisterConverter<float>(new FloatValueConverter());
        }
    }
}
