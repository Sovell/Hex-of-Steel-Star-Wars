using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

public static class UnitsFilterHelper
{
	private enum LogicOp
	{
		And,
		Or
	}

	private class Condition
	{
		public string PropertyName;

		public string Operator;

		public string RawValue;
	}

	public static List<T> Filter<T>(IEnumerable<T> source, string filterString)
	{
		if (string.IsNullOrWhiteSpace(filterString))
		{
			return source.ToList();
		}
		string[] array = (from s in Regex.Split(filterString.Trim(), "\\s+(AND|OR)\\s+", RegexOptions.IgnoreCase)
			where !string.IsNullOrWhiteSpace(s)
			select s).ToArray();
		if (array.Length % 2 == 0)
		{
			throw new ArgumentException("Filter must start and end with a condition.");
		}
		List<object> list = new List<object>();
		for (int num = 0; num < array.Length; num++)
		{
			if (num % 2 == 1)
			{
				LogicOp logicOp = (array[num].Equals("OR", StringComparison.OrdinalIgnoreCase) ? LogicOp.Or : LogicOp.And);
				list.Add(logicOp);
			}
			else
			{
				list.Add(ParseCondition(array[num]));
			}
		}
		Dictionary<string, PropertyInfo> props = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public).ToDictionary((PropertyInfo p) => p.Name, (PropertyInfo p) => p, StringComparer.OrdinalIgnoreCase);
		List<T> list2 = new List<T>();
		foreach (T item in source)
		{
			bool flag = Eval(item, (Condition)list[0], props);
			for (int num2 = 1; num2 < list.Count; num2 += 2)
			{
				LogicOp logicOp2 = (LogicOp)list[num2];
				Condition cond = (Condition)list[num2 + 1];
				bool flag2 = Eval(item, cond, props);
				flag = ((logicOp2 == LogicOp.And) ? (flag & flag2) : (flag | flag2));
			}
			if (flag)
			{
				list2.Add(item);
			}
		}
		return list2;
	}

	private static Condition ParseCondition(string raw)
	{
		string[] array = new string[5] { ">=", "<=", ">", "<", "=" };
		foreach (string text in array)
		{
			int num = raw.IndexOf(text, StringComparison.Ordinal);
			if (num >= 0)
			{
				return new Condition
				{
					PropertyName = raw.Substring(0, num).Trim(),
					Operator = text,
					RawValue = raw.Substring(num + text.Length).Trim()
				};
			}
		}
		int num2 = raw.IndexOf(" contains ", StringComparison.OrdinalIgnoreCase);
		if (num2 >= 0)
		{
			return new Condition
			{
				PropertyName = raw.Substring(0, num2).Trim(),
				Operator = "CONTAINS",
				RawValue = raw.Substring(num2 + " contains ".Length).Trim()
			};
		}
		return new Condition
		{
			PropertyName = "Name",
			Operator = "CONTAINS",
			RawValue = raw.Trim()
		};
	}

	private static bool Eval<T>(T item, Condition cond, Dictionary<string, PropertyInfo> props)
	{
		if (!props.TryGetValue(cond.PropertyName, out var value))
		{
			return false;
		}
		return Compare(value.GetValue(item), cond.Operator, cond.RawValue, value.PropertyType);
	}

	private static bool Compare(object leftObj, string op, string rawRight, Type t)
	{
		if (leftObj == null)
		{
			return false;
		}
		if (IsNumeric(t))
		{
			double num = Convert.ToDouble(leftObj);
			if (!double.TryParse(rawRight, out var result))
			{
				return false;
			}
			switch (op)
			{
			case ">":
				return num > result;
			case ">=":
				return num >= result;
			case "<":
				return num < result;
			case "<=":
				return num <= result;
			case "=":
				return Math.Abs(num - result) < 1E-09;
			default:
				return false;
			}
		}
		if (t == typeof(bool) && op == "=" && bool.TryParse(rawRight, out var result2))
		{
			return (bool)leftObj == result2;
		}
		if (t == typeof(string))
		{
			string text = (string)leftObj;
			if (op == "=")
			{
				return string.Equals(text, rawRight, StringComparison.OrdinalIgnoreCase);
			}
			if (op.Equals("CONTAINS", StringComparison.OrdinalIgnoreCase))
			{
				if (text == null)
				{
					return false;
				}
				return text.IndexOf(rawRight, StringComparison.OrdinalIgnoreCase) >= 0;
			}
		}
		Debug.LogWarning("FilterHelper: unsupported compare for " + t.Name + " " + op);
		return false;
	}

	private static bool IsNumeric(Type t)
	{
		if (!(t == typeof(byte)) && !(t == typeof(sbyte)) && !(t == typeof(short)) && !(t == typeof(ushort)) && !(t == typeof(int)) && !(t == typeof(uint)) && !(t == typeof(long)) && !(t == typeof(ulong)) && !(t == typeof(float)) && !(t == typeof(double)))
		{
			return t == typeof(decimal);
		}
		return true;
	}
}
