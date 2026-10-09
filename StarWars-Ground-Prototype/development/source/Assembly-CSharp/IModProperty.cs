using System;

internal interface IModProperty
{
	Type ValueType { get; }

	object BoxedValue { get; }
}
