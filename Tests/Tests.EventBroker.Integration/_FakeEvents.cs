using System;
using EventBroker.Core;

namespace Tests.EventBroker.Integration
{
	public class FirstEvent : IEvent
	{
		public int IntProperty { get; set; }
		public string StringProperty { get; set; }
		public bool BoolProperty { get; set; }
	}

	[Flags]
	public enum TestEnum
	{
		FirstOption = 0,
		SecondOption = 1,
		ThirdOption = 2,
		FourthOption = 4
	}

	public class SecondEvent : IEvent
	{
		public TestEnum EnumValue { get; set; }
	}
}
