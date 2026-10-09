using System;
using System.Collections.Generic;
using System.Reflection;

public class UnitModification
{
	public interface IOperation
	{
		string TargetName { get; }

		Func<Unit, bool> Predicate { get; }

		void Apply(Unit u);

		void Revert(Unit u);
	}

	public abstract class OperationBase : IOperation
	{
		protected readonly MemberInfo Member;

		public string TargetName => Member.Name;

		public Func<Unit, bool> Predicate { get; }

		protected Type MemberType
		{
			get
			{
				MemberInfo member = Member;
				if (!(member is PropertyInfo propertyInfo))
				{
					if (member is FieldInfo fieldInfo)
					{
						return fieldInfo.FieldType;
					}
					throw new InvalidOperationException();
				}
				return propertyInfo.PropertyType;
			}
		}

		protected OperationBase(string name, Func<Unit, bool> predicate)
		{
			PropertyInfo property = typeof(Unit).GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property != null)
			{
				Member = property;
			}
			else
			{
				FieldInfo field = typeof(Unit).GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (!(field != null))
				{
					throw new ArgumentException("Property or field '" + name + "' not found on Unit");
				}
				Member = field;
			}
			Predicate = predicate;
		}

		public abstract void Apply(Unit u);

		public abstract void Revert(Unit u);

		protected object GetRaw(Unit u)
		{
			MemberInfo member = Member;
			if (!(member is PropertyInfo propertyInfo))
			{
				if (member is FieldInfo fieldInfo)
				{
					return fieldInfo.GetValue(u);
				}
				throw new InvalidOperationException();
			}
			return propertyInfo.GetValue(u);
		}

		protected void SetRaw(Unit u, object value)
		{
			MemberInfo member = Member;
			if (!(member is PropertyInfo propertyInfo))
			{
				if (!(member is FieldInfo fieldInfo))
				{
					throw new InvalidOperationException();
				}
				fieldInfo.SetValue(u, value);
			}
			else
			{
				propertyInfo.SetValue(u, value);
			}
		}

		protected static object DefaultOf(Type t)
		{
			if (!t.IsValueType)
			{
				return null;
			}
			return Activator.CreateInstance(t);
		}

		protected object ClampToMemberType(double value)
		{
			switch (Type.GetTypeCode(MemberType))
			{
			case TypeCode.Byte:
				return (byte)Math.Clamp((int)Math.Round(value), 0, 255);
			case TypeCode.SByte:
				return (sbyte)Math.Clamp((int)Math.Round(value), -128, 127);
			case TypeCode.UInt16:
				return (ushort)Math.Clamp((int)Math.Round(value), 0, 65535);
			case TypeCode.Int16:
				return (short)Math.Clamp((int)Math.Round(value), -32768, 32767);
			case TypeCode.UInt32:
				return (uint)Math.Clamp((long)Math.Round(value), 0L, 4294967295L);
			case TypeCode.Int32:
				return (int)Math.Clamp((long)Math.Round(value), -2147483648L, 2147483647L);
			case TypeCode.UInt64:
				if (value < 0.0)
				{
					return 0uL;
				}
				if (value > 1.8446744073709552E+19)
				{
					return ulong.MaxValue;
				}
				return (ulong)Math.Round(value);
			case TypeCode.Int64:
				if (value < -9.223372036854776E+18)
				{
					return long.MinValue;
				}
				if (value > 9.223372036854776E+18)
				{
					return long.MaxValue;
				}
				return (long)Math.Round(value);
			case TypeCode.Single:
				return (float)value;
			case TypeCode.Double:
				return value;
			case TypeCode.Decimal:
				if (value < -7.922816251426434E+28)
				{
					return decimal.MinValue;
				}
				if (value > 7.922816251426434E+28)
				{
					return decimal.MaxValue;
				}
				return (decimal)value;
			default:
				return Convert.ChangeType(value, MemberType);
			}
		}
	}

	public class SetOp : OperationBase
	{
		private readonly object _value;

		private readonly object _default;

		public object Value => _value;

		public object DefaultValue => _default;

		public SetOp(string name, object value, Func<Unit, bool> predicate)
			: base(name, predicate)
		{
			if (IsNumeric(MemberType))
			{
				_value = ClampToMemberType(Convert.ToDouble(value));
			}
			else
			{
				_value = Convert.ChangeType(value, MemberType);
			}
			_default = OperationBase.DefaultOf(MemberType);
		}

		public override void Apply(Unit u)
		{
			SetRaw(u, _value);
		}

		public override void Revert(Unit u)
		{
			SetRaw(u, _default);
		}

		private static bool IsNumeric(Type t)
		{
			TypeCode typeCode = Type.GetTypeCode(t);
			if ((uint)(typeCode - 5) <= 10u)
			{
				return true;
			}
			return false;
		}
	}

	public class AddOp : OperationBase
	{
		private readonly double _delta;

		public double Delta => _delta;

		public AddOp(string name, double delta, Func<Unit, bool> predicate)
			: base(name, predicate)
		{
			_delta = delta;
		}

		public override void Apply(Unit u)
		{
			Change(u, _delta);
		}

		public override void Revert(Unit u)
		{
			Change(u, 0.0 - _delta);
		}

		private void Change(Unit u, double d)
		{
			double value = Convert.ToDouble(GetRaw(u)) + d;
			SetRaw(u, ClampToMemberType(value));
		}
	}

	public class MultiplyAddOp : OperationBase
	{
		private readonly MemberInfo _baseMember;

		private readonly double _factor;

		public double Factor => _factor;

		public MultiplyAddOp(string targetProp, string baseProp, double factor, Func<Unit, bool> predicate)
			: base(targetProp, predicate)
		{
			PropertyInfo property = typeof(Unit).GetProperty(baseProp, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property != null)
			{
				_baseMember = property;
			}
			else
			{
				FieldInfo field = typeof(Unit).GetField(baseProp, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (!(field != null))
				{
					throw new ArgumentException("Property or field '" + baseProp + "' not found on Unit");
				}
				_baseMember = field;
			}
			_factor = factor;
		}

		public override void Apply(Unit u)
		{
			Change(u, _factor);
		}

		public override void Revert(Unit u)
		{
			Change(u, 0.0 - _factor);
		}

		private void Change(Unit u, double fac)
		{
			MemberInfo baseMember = _baseMember;
			object value;
			if (!(baseMember is PropertyInfo propertyInfo))
			{
				if (!(baseMember is FieldInfo fieldInfo))
				{
					throw new InvalidOperationException();
				}
				value = fieldInfo.GetValue(u);
			}
			else
			{
				value = propertyInfo.GetValue(u);
			}
			double num = Convert.ToDouble(value) * fac;
			double value2 = Convert.ToDouble(GetRaw(u)) + num;
			SetRaw(u, ClampToMemberType(value2));
		}
	}

	private readonly List<IOperation> _ops = new List<IOperation>();

	public UnitModification Set(string propName, object value, Func<Unit, bool> predicate = null)
	{
		_ops.Add(new SetOp(propName, value, predicate));
		return this;
	}

	public UnitModification Add(string propName, double amount, Func<Unit, bool> predicate = null)
	{
		_ops.Add(new AddOp(propName, amount, predicate));
		return this;
	}

	public UnitModification MultiplyAdd(string targetProp, string baseProp, double factor, Func<Unit, bool> predicate = null)
	{
		_ops.Add(new MultiplyAddOp(targetProp, baseProp, factor, predicate));
		return this;
	}

	public UnitModification Remove(string propName)
	{
		_ops.RemoveAll((IOperation op) => op.TargetName == propName);
		return this;
	}

	public UnitModification Remove(string propName, Type opType)
	{
		_ops.RemoveAll((IOperation op) => op.TargetName == propName && op.GetType() == opType);
		return this;
	}

	public bool TryGetOperation(string propName, out IOperation op)
	{
		op = _ops.Find((IOperation o) => o.TargetName == propName);
		return op != null;
	}

	public bool TryGetOperation(string propName, Type opType, out IOperation op)
	{
		op = _ops.Find((IOperation o) => o.TargetName == propName && o.GetType() == opType);
		return op != null;
	}

	public void Apply(Unit u)
	{
		foreach (IOperation op in _ops)
		{
			Func<Unit, bool> predicate = op.Predicate;
			if (predicate == null || predicate(u))
			{
				op.Apply(u);
			}
		}
	}

	public void Revert(Unit u)
	{
		foreach (IOperation op in _ops)
		{
			Func<Unit, bool> predicate = op.Predicate;
			if (predicate == null || predicate(u))
			{
				op.Revert(u);
			}
		}
	}
}
