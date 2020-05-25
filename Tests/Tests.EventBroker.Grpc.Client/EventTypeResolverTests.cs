using System;
using System.Linq;
using System.Reflection;
using EventBroker.Core;
using EventBroker.Grpc.Client.TypeResolver;
using NUnit.Framework;

namespace Tests.EventBroker.Grpc.Client
{
    internal class EventTypeResolverTests
    {
        public class StubEvent : IEvent
        {
            public string StringProperty { get; set; }
            public int IntProperty { get; set; }
            public int PrivateSetterProperty => 10;
        }

        public class StubNoParameterlessConstructorEvent : IEvent
        {
            public StubNoParameterlessConstructorEvent(int anyParameter)
            {
                AnyParameter = anyParameter;
            }

            public int AnyParameter { get; set; }
        }

        [Test]
        public void resolve_stub_event_type()
        {
            var resolver = InitializeResolver();

            var typeName = typeof(StubEvent).FullName;
            var type = resolver.GetEventType(typeName);

            Assert.That(type, Is.EqualTo(typeof(StubEvent)));

            type = resolver.GetEventType(typeName);

            Assert.That(type, Is.EqualTo(typeof(StubEvent)));
        }

        [Test]
        public void should_throw_on_resolving_not_exists_event_type()
        {
            var resolver = InitializeResolver();

            var typeName = "SomeNotExistingType";

            Assert.Throws<TypeLoadException>(() =>
            {
                resolver.GetEventType(typeName);
            });
        }

        [Test]
        public void get_constructor_info()
        {
            var resolver = InitializeResolver();

            var eventType = typeof(StubEvent);
            var expectedConstructor = eventType.GetConstructor(Type.EmptyTypes);
            var returnedConstructor = resolver.GetEventConstructor(eventType);

            Assert.That(returnedConstructor, Is.EqualTo(expectedConstructor));

            returnedConstructor = resolver.GetEventConstructor(eventType);

            Assert.That(returnedConstructor, Is.EqualTo(expectedConstructor));
        }

        [Test]
        public void should_throw_on_no_parameterless_event_constructor()
        {
            var resolver = InitializeResolver();

            var eventType = typeof(StubNoParameterlessConstructorEvent);

            Assert.Throws<MissingMethodException>(() =>
            {
                resolver.GetEventConstructor(eventType);
            });
        }

        [Test]
        public void should_throw_on_unknown_property()
        {
            var resolver = InitializeResolver();
            resolver.ThrowOnNotExistingProperty = true;

            Assert.Throws<InvalidOperationException>(() =>
            {
                resolver.GetPropertyInfo(typeof(StubEvent), "NotExistingProperty");
            });
        }

        [Test]
        public void should_return_null_property()
        {
            var resolver = InitializeResolver();
            resolver.ThrowOnNotExistingProperty = false;

            var property = resolver.GetPropertyInfo(typeof(StubEvent), "NotExistingProperty");

            Assert.That(property, Is.Null);
        }

        [Test]
        public void get_property_info()
        {
            var resolver = InitializeResolver();

            var intProperty = resolver.GetPropertyInfo(typeof(StubEvent), "IntProperty");
            var stringProperty = resolver.GetPropertyInfo(typeof(StubEvent), "StringProperty");

            Assert.Multiple(() =>
            {
                Assert.That(intProperty, Is.Not.Null);
                Assert.That(stringProperty, Is.Not.Null);
            });

            Assert.Multiple(() =>
            {
                Assert.That(intProperty.Name, Is.EqualTo("IntProperty"));
                Assert.That(stringProperty.Name, Is.EqualTo("StringProperty"));
            });
        }

        [Test]
        public void get_properties_for_specified_type()
        {
            var resolver = InitializeResolver();

            var properties1 = resolver.GetProperties(typeof(StubEvent));
            var properties2 = resolver.GetProperties(typeof(StubEvent));

            var resolvedPropertiesNames = properties1
                .Select(p => p.Name)
                .ToArray();

            Assert.Multiple(() =>
            {
                Assert.That(properties1, Has.Exactly(2).Items);

                CollectionAssert.AreEqual(properties1, properties2);

                CollectionAssert.AreEqual(
                    new[] {"StringProperty", "IntProperty"},
                    resolvedPropertiesNames);
            });

        }

        private static EventTypeResolver InitializeResolver()
        {
            var resolver = new EventTypeResolver();

            resolver.RegisterEventsAssembly(Assembly.GetExecutingAssembly());

            return resolver;
        }
    }
}
