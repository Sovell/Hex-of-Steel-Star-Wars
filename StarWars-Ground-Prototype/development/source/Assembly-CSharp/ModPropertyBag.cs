using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

[Serializable]
public sealed class ModPropertyBag : ISerializable
{
	private enum DataTypes : byte
	{
		BOOL = 1,
		SBYTE = 2,
		BYTE = 3,
		CHAR = 4,
		SHORT = 5,
		USHORT = 6,
		INT = 7,
		UINT = 8,
		LONG = 9,
		ULONG = 10,
		FLOAT = 11,
		DOUBLE = 12,
		DECIMAL = 13,
		STRING = 14,
		GUID = 15,
		BLOB = 99
	}

	private sealed class DTO
	{
		public uint key;

		public DataTypes type;

		public byte[] blob;
	}

	private const int MaxEntries = 1024;

	private const int MaxTotalSize = 4194304;

	private readonly Dictionary<uint, IModProperty> _propertiesByID = new Dictionary<uint, IModProperty>();

	private readonly List<DTO> _unknownEntries = new List<DTO>();

	[NonSerialized]
	private Dictionary<uint, string> _firstKeyString;

	private Dictionary<uint, string> FirstKeyString => _firstKeyString ?? (_firstKeyString = new Dictionary<uint, string>());

	public int CountKnowns => _propertiesByID.Count;

	public int CountUnknowns => _unknownEntries.Count;

	public int CountTotal => CountKnowns + CountUnknowns;

	public bool IsEmpty => CountTotal == 0;

	public bool TryGet<T>(string p_key, out T o_value)
	{
		uint key = Utils.ComputeKeyID32(p_key);
		if (_propertiesByID.TryGetValue(key, out var value))
		{
			if (value is ModProperty<T> modProperty)
			{
				o_value = modProperty.Value;
				return true;
			}
			if (value.BoxedValue is T val)
			{
				o_value = val;
				return true;
			}
		}
		o_value = default;
		return false;
	}

	public bool TrySet<T>(string p_key, T p_value, bool preferKnownOverUnknown = false)
	{
		uint num = Utils.ComputeKeyID32(p_key);
		if (_firstKeyString != null && _firstKeyString.TryGetValue(num, out var value) && value != p_key)
		{
			Debug.LogError($"Key hash collision detected: '{value}' vs '{p_key}' (ID={num}). Consider renaming your mod key ASAP.");
		}
		else
		{
			FirstKeyString[num] = p_key;
		}
		if (_propertiesByID.ContainsKey(num))
		{
			Dictionary<uint, IModProperty> propertiesByID = _propertiesByID;
			IModProperty value2;
			if (!(p_value is IModProperty modProperty))
			{
				IModProperty modProperty2 = new ModProperty<T>(p_value);
				value2 = modProperty2;
			}
			else
			{
				value2 = modProperty;
			}
			propertiesByID[num] = value2;
			return true;
		}
		if (!RemoveUnknownByID(num) && CountTotal >= 1024)
		{
			if (!preferKnownOverUnknown || _unknownEntries.Count <= 0)
			{
				Debug.LogWarning($"ModProperty '{p_key}' cannot be added because the bag is full ({CountTotal}/{1024}).");
				return false;
			}
			_unknownEntries.RemoveAt(0);
		}
		if (p_value != null)
		{
			Type type = p_value.GetType();
			if (!type.IsSerializable && !typeof(ISerializable).IsAssignableFrom(type))
			{
				Debug.LogWarning("ModProperty '" + p_key + "' value type " + type.FullName + " is not serializable and will be skipped on save.");
			}
		}
		Dictionary<uint, IModProperty> propertiesByID2 = _propertiesByID;
		IModProperty value3;
		if (!(p_value is IModProperty modProperty3))
		{
			IModProperty modProperty2 = new ModProperty<T>(p_value);
			value3 = modProperty2;
		}
		else
		{
			value3 = modProperty3;
		}
		propertiesByID2[num] = value3;
		return true;
	}

	public bool HasKey(string p_key, bool p_includeUnknowns = false)
	{
		uint num = Utils.ComputeKeyID32(p_key);
		if (_propertiesByID.ContainsKey(num))
		{
			return true;
		}
		if (p_includeUnknowns)
		{
			foreach (DTO unknownEntry in _unknownEntries)
			{
				if (unknownEntry.key == num)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool Remove(string p_key)
	{
		uint num = Utils.ComputeKeyID32(p_key);
		bool flag = _propertiesByID.Remove(num);
		bool flag2 = RemoveUnknownByID(num);
		return flag | flag2;
	}

	public void Clear()
	{
		_propertiesByID.Clear();
		_unknownEntries.Clear();
		_firstKeyString?.Clear();
	}

	private bool RemoveUnknownByID(uint p_ID)
	{
		bool result = false;
		for (int num = _unknownEntries.Count - 1; num >= 0; num--)
		{
			if (_unknownEntries[num].key == p_ID)
			{
				_unknownEntries.RemoveAt(num);
				result = true;
			}
		}
		return result;
	}

	public ModPropertyBag()
	{
	}

	private ModPropertyBag(SerializationInfo p_info, StreamingContext p_context)
	{
		byte[] array = (byte[])p_info.GetValue("blob", typeof(byte[]));
		if (array == null || array.Length == 0)
		{
			return;
		}
		using (MemoryStream memoryStream = new MemoryStream(array, writable: false))
		{
			using (BinaryReader binaryReader = new BinaryReader(memoryStream, Encoding.UTF8, leaveOpen: false))
			{
				int num = binaryReader.ReadInt32();
				if (num < 0 || num > 1024)
				{
					throw new SerializationException("Corrupted ModPropertyBag: invalid count.");
				}
				long num2 = 0L;
				for (int i = 0; i < num; i++)
				{
					uint num3 = binaryReader.ReadUInt32();
					DataTypes dataTypes = (DataTypes)binaryReader.ReadByte();
					int num4 = binaryReader.ReadInt32();
					long num5 = memoryStream.Length - memoryStream.Position;
					if (num4 < 0 || num4 > num5)
					{
						throw new SerializationException("Corrupted ModPropertyBag: invalid payload length.");
					}
					num2 += num4;
					if (num2 > 4194304 || num4 > 4194304)
					{
						throw new SerializationException("Corrupted ModPropertyBag: total payload too large.");
					}
					byte[] array2 = binaryReader.ReadBytes(num4);
					if (array2.Length != num4)
					{
						throw new SerializationException("Truncated payload.");
					}
					object o_value;
					if (TryGetSystemType(dataTypes, out var o_type))
					{
						try
						{
							object obj = DeserializeKnown(dataTypes, array2);
							Type type = typeof(ModProperty<>).MakeGenericType(o_type);
							_propertiesByID[num3] = (IModProperty)Activator.CreateInstance(type, obj);
						}
						catch
						{
							goto IL_01f9;
						}
					}
					else if (TryDeserializeCustom(array2, out o_value))
					{
						Type type2 = typeof(ModProperty<>).MakeGenericType(o_value.GetType());
						_propertiesByID[num3] = (IModProperty)Activator.CreateInstance(type2, o_value);
					}
					else
					{
						_unknownEntries.Add(new DTO
						{
							key = num3,
							type = dataTypes,
							blob = array2
						});
						Debug.LogError($"Could not deserialize ModProperty ID={num3}");
					}
					continue;
					IL_01f9:
					try
					{
						using (MemoryStream serializationStream = new MemoryStream(array2, writable: false))
						{
							object obj3 = new BinaryFormatter().Deserialize(serializationStream);
							bool flag = !o_type.IsValueType || Nullable.GetUnderlyingType(o_type) != null;
							if ((obj3 == null && !flag) || (obj3 != null && !o_type.IsInstanceOfType(obj3)))
							{
								_unknownEntries.Add(new DTO
								{
									key = num3,
									type = dataTypes,
									blob = array2
								});
							}
							else
							{
								IModProperty value = (IModProperty)Activator.CreateInstance(typeof(ModProperty<>).MakeGenericType(o_type), obj3);
								_propertiesByID[num3] = value;
							}
						}
					}
					catch
					{
						_unknownEntries.Add(new DTO
						{
							key = num3,
							type = dataTypes,
							blob = array2
						});
						Debug.LogError($"Could not deserialize ModProperty with ID={num3}");
					}
				}
			}
		}
	}

	void ISerializable.GetObjectData(SerializationInfo p_info, StreamingContext p_context)
	{
		using (MemoryStream memoryStream = new MemoryStream())
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream, Encoding.UTF8, leaveOpen: true))
			{
				List<DTO> list = new List<DTO>(CountKnowns);
				long num = 0L;
				foreach (KeyValuePair<uint, IModProperty> item in _propertiesByID)
				{
					Type valueType = item.Value.ValueType;
					object boxedValue = item.Value.BoxedValue;
					try
					{
						byte[] array;
						if (TryGetTypeCode(valueType, out var o_dataType))
						{
							array = SerializeKnown(o_dataType, boxedValue);
						}
						else
						{
							using (MemoryStream memoryStream2 = new MemoryStream())
							{
								new BinaryFormatter().Serialize(memoryStream2, boxedValue);
								array = memoryStream2.ToArray();
							}
						}
						if (array.Length > 4194304)
						{
							Debug.LogError($"Skipping ModProperty ID={item.Key} (payload too large: {array.Length} bytes)");
							continue;
						}
						if (num + array.Length > 4194304)
						{
							Debug.LogError($"Stopping ModPropertyBag write: total payload exceeds {4194304} bytes.");
							break;
						}
						num += array.Length;
						list.Add(new DTO
						{
							key = item.Key,
							type = o_dataType,
							blob = array
						});
					}
					catch (SerializationException arg)
					{
						Debug.LogError($"Could not serialize ModProperty ID={item.Key}: {arg}");
					}
					catch (Exception arg2)
					{
						Debug.LogError($"Unexpected error serializing ModProperty ID={item.Key}: {arg2}");
					}
				}
				List<DTO> list2 = new List<DTO>(CountUnknowns);
				foreach (DTO unknownEntry in _unknownEntries)
				{
					int num2 = unknownEntry.blob?.Length ?? 0;
					if (num + num2 > 4194304)
					{
						break;
					}
					num += num2;
					list2.Add(unknownEntry);
				}
				int num3 = _unknownEntries.Count - list2.Count;
				if (num3 > 0)
				{
					Debug.LogWarning($"Trimmed {num3} unknown entries to fit {4194304} bytes.");
				}
				List<DTO> list3 = new List<DTO>(list.Count + list2.Count);
				list3.AddRange(list);
				list3.AddRange(list2);
				list3.Sort((DTO a, DTO b) => a.key.CompareTo(b.key));
				binaryWriter.Write(list3.Count);
				foreach (DTO item2 in list3)
				{
					binaryWriter.Write(item2.key);
					binaryWriter.Write((byte)item2.type);
					binaryWriter.Write(item2.blob?.Length ?? 0);
					if (item2.blob != null && item2.blob.Length != 0)
					{
						binaryWriter.Write(item2.blob);
					}
				}
				binaryWriter.Flush();
				p_info.AddValue("blob", memoryStream.ToArray());
			}
		}
	}

	private static bool TryGetSystemType(DataTypes p_dataType, out Type o_type)
	{
		switch (p_dataType)
		{
		case DataTypes.BOOL:
			o_type = typeof(bool);
			return true;
		case DataTypes.SBYTE:
			o_type = typeof(sbyte);
			return true;
		case DataTypes.BYTE:
			o_type = typeof(byte);
			return true;
		case DataTypes.CHAR:
			o_type = typeof(char);
			return true;
		case DataTypes.SHORT:
			o_type = typeof(short);
			return true;
		case DataTypes.USHORT:
			o_type = typeof(ushort);
			return true;
		case DataTypes.INT:
			o_type = typeof(int);
			return true;
		case DataTypes.UINT:
			o_type = typeof(uint);
			return true;
		case DataTypes.LONG:
			o_type = typeof(long);
			return true;
		case DataTypes.ULONG:
			o_type = typeof(ulong);
			return true;
		case DataTypes.FLOAT:
			o_type = typeof(float);
			return true;
		case DataTypes.DOUBLE:
			o_type = typeof(double);
			return true;
		case DataTypes.DECIMAL:
			o_type = typeof(decimal);
			return true;
		case DataTypes.STRING:
			o_type = typeof(string);
			return true;
		case DataTypes.GUID:
			o_type = typeof(Guid);
			return true;
		default:
			o_type = null;
			return false;
		}
	}

	private static bool TryGetTypeCode(Type p_type, out DataTypes o_dataType)
	{
		if (p_type == typeof(bool))
		{
			o_dataType = DataTypes.BOOL;
			return true;
		}
		if (p_type == typeof(sbyte))
		{
			o_dataType = DataTypes.SBYTE;
			return true;
		}
		if (p_type == typeof(byte))
		{
			o_dataType = DataTypes.BYTE;
			return true;
		}
		if (p_type == typeof(char))
		{
			o_dataType = DataTypes.CHAR;
			return true;
		}
		if (p_type == typeof(short))
		{
			o_dataType = DataTypes.SHORT;
			return true;
		}
		if (p_type == typeof(ushort))
		{
			o_dataType = DataTypes.USHORT;
			return true;
		}
		if (p_type == typeof(int))
		{
			o_dataType = DataTypes.INT;
			return true;
		}
		if (p_type == typeof(uint))
		{
			o_dataType = DataTypes.UINT;
			return true;
		}
		if (p_type == typeof(long))
		{
			o_dataType = DataTypes.LONG;
			return true;
		}
		if (p_type == typeof(ulong))
		{
			o_dataType = DataTypes.ULONG;
			return true;
		}
		if (p_type == typeof(float))
		{
			o_dataType = DataTypes.FLOAT;
			return true;
		}
		if (p_type == typeof(double))
		{
			o_dataType = DataTypes.DOUBLE;
			return true;
		}
		if (p_type == typeof(decimal))
		{
			o_dataType = DataTypes.DECIMAL;
			return true;
		}
		if (p_type == typeof(string))
		{
			o_dataType = DataTypes.STRING;
			return true;
		}
		if (p_type == typeof(Guid))
		{
			o_dataType = DataTypes.GUID;
			return true;
		}
		o_dataType = DataTypes.BLOB;
		return false;
	}

	private static byte[] SerializeKnown(DataTypes p_dataType, object p_object)
	{
		using (MemoryStream memoryStream = new MemoryStream())
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream, Encoding.UTF8, leaveOpen: true))
			{
				switch (p_dataType)
				{
				case DataTypes.BOOL:
					binaryWriter.Write((bool)p_object);
					break;
				case DataTypes.SBYTE:
					binaryWriter.Write((sbyte)p_object);
					break;
				case DataTypes.BYTE:
					binaryWriter.Write((byte)p_object);
					break;
				case DataTypes.CHAR:
					binaryWriter.Write((char)p_object);
					break;
				case DataTypes.SHORT:
					binaryWriter.Write((short)p_object);
					break;
				case DataTypes.USHORT:
					binaryWriter.Write((ushort)p_object);
					break;
				case DataTypes.INT:
					binaryWriter.Write((int)p_object);
					break;
				case DataTypes.UINT:
					binaryWriter.Write((uint)p_object);
					break;
				case DataTypes.LONG:
					binaryWriter.Write((long)p_object);
					break;
				case DataTypes.ULONG:
					binaryWriter.Write((ulong)p_object);
					break;
				case DataTypes.FLOAT:
					binaryWriter.Write((float)p_object);
					break;
				case DataTypes.DOUBLE:
					binaryWriter.Write((double)p_object);
					break;
				case DataTypes.DECIMAL:
					binaryWriter.Write((decimal)p_object);
					break;
				case DataTypes.GUID:
					binaryWriter.Write(((Guid)p_object).ToByteArray());
					break;
				case DataTypes.STRING:
				{
					bool flag = p_object != null;
					binaryWriter.Write(flag);
					if (flag)
					{
						binaryWriter.Write((string)p_object);
					}
					break;
				}
				default:
					throw new InvalidOperationException("Not a known type.");
				}
				return memoryStream.ToArray();
			}
		}
	}

	private static object DeserializeKnown(DataTypes p_dataType, byte[] p_payload)
	{
		using (MemoryStream input = new MemoryStream(p_payload, writable: false))
		{
			using (BinaryReader binaryReader = new BinaryReader(input, Encoding.UTF8, leaveOpen: false))
			{
				switch (p_dataType)
				{
				case DataTypes.BOOL:
					return binaryReader.ReadBoolean();
				case DataTypes.SBYTE:
					return binaryReader.ReadSByte();
				case DataTypes.BYTE:
					return binaryReader.ReadByte();
				case DataTypes.CHAR:
					return binaryReader.ReadChar();
				case DataTypes.SHORT:
					return binaryReader.ReadInt16();
				case DataTypes.USHORT:
					return binaryReader.ReadUInt16();
				case DataTypes.INT:
					return binaryReader.ReadInt32();
				case DataTypes.UINT:
					return binaryReader.ReadUInt32();
				case DataTypes.LONG:
					return binaryReader.ReadInt64();
				case DataTypes.ULONG:
					return binaryReader.ReadUInt64();
				case DataTypes.FLOAT:
					return binaryReader.ReadSingle();
				case DataTypes.DOUBLE:
					return binaryReader.ReadDouble();
				case DataTypes.DECIMAL:
					return binaryReader.ReadDecimal();
				case DataTypes.STRING:
					return binaryReader.ReadBoolean() ? binaryReader.ReadString() : null;
				case DataTypes.GUID:
					return new Guid(binaryReader.ReadBytes(16));
				default:
					throw new InvalidOperationException("Not a known type.");
				}
			}
		}
	}

	private static bool TryDeserializeCustom(byte[] p_blob, out object o_value)
	{
		try
		{
			using (MemoryStream serializationStream = new MemoryStream(p_blob, writable: false))
			{
				o_value = new BinaryFormatter().Deserialize(serializationStream);
				return o_value != null;
			}
		}
		catch
		{
			o_value = null;
			return false;
		}
	}
}
