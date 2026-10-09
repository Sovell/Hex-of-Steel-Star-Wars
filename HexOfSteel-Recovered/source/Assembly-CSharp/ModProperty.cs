using System;

[Serializable]
internal sealed class ModProperty<T> : IModProperty
{
	public T Value { get; }

	public Type ValueType => typeof(T);

	public object BoxedValue => Value;

	public ModProperty()
	{
	}

	public ModProperty(T v)
	{
		Value = v;
	}
}
