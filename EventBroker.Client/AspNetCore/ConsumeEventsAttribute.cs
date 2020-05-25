using System;
using EventBroker.Core;

namespace EventBroker.Client.AspNetCore
{
	[AttributeUsage(AttributeTargets.Class)]
	internal sealed class ConsumeEventsAttribute : Attribute
	{
		public ConsumptionType ConsumptionType { get; }

		public ConsumeEventsAttribute(ConsumptionType consumptionType)
		{
			ConsumptionType = consumptionType;
		}
	}
}
