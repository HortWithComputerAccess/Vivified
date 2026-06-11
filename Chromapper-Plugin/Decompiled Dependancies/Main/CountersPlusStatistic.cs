using System;

[Flags]
public enum CountersPlusStatistic
{
	Invalid = 0,
	Notes = 1,
	Obstacles = 2,
	Events = 4,
	BpmEvents = 8,
	Selection = 0x10,
	Arcs = 0x20,
	Chains = 0x40,
	NJSEvents = 0x80
}
