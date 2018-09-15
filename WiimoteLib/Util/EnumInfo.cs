using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace WiimoteLib.Util {
	internal static class EnumInfo<TEnum> where TEnum : struct {
		private static Dictionary<string, EnumFieldInfo<TEnum>> nameLookup;
		private static Dictionary<TEnum, EnumFieldInfo<TEnum>> valueLookup;

		public static Type Type { get; }
		public static Type UnderlyingType { get; }
		public static int UnderlyingTypeSize { get; }
		public static bool IsUInt64 { get; }

		static EnumInfo() {
			Type = typeof(TEnum);
			UnderlyingType = Enum.GetUnderlyingType(Type);
			UnderlyingTypeSize = Marshal.SizeOf(UnderlyingType);
			IsUInt64 = UnderlyingType == typeof(ulong);
			if (!typeof(TEnum).IsEnum)
				throw new ArgumentException($"{Type.Name} is not an enum!",
					nameof(TEnum));

			nameLookup = new Dictionary<string, EnumFieldInfo<TEnum>>();
			valueLookup = new Dictionary<TEnum, EnumFieldInfo<TEnum>>();

			foreach (FieldInfo field in Type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) {
				var info = new EnumFieldInfo<TEnum>(field, IsUInt64, UnderlyingTypeSize);
				nameLookup.Add(info.Name, info);
				if (!valueLookup.ContainsKey(info.Value))
					valueLookup.Add(info.Value, info);
			}
		}

		public static IEnumerable<EnumFieldInfo<TEnum>> Fields => nameLookup.Values;

		public static EnumFieldInfo<TEnum> GetField(TEnum value) {
			return valueLookup[value];
		}

		public static EnumFieldInfo<TEnum> TryGetField(TEnum value) {
			if (valueLookup.TryGetValue(value, out var fieldInfo))
				return fieldInfo;
			return null;
		}

		public static TAttr GetAttribute<TAttr>(TEnum value)
			where TAttr : Attribute
		{
			return valueLookup[value].GetAttribute<TAttr>();
		}

		public static TAttr TryGetAttribute<TAttr>(TEnum value)
			where TAttr : Attribute
		{
			EnumFieldInfo<TEnum> field = TryGetField(value);
			return field?.GetAttribute<TAttr>();
		}

		public static EnumFieldInfo<TEnum> GetField(string name) {
			return nameLookup[name];
		}

		public static EnumFieldInfo<TEnum> TryGetField(string name) {
			if (nameLookup.TryGetValue(name, out var fieldInfo))
				return fieldInfo;
			return null;
		}

		public static IEnumerable<EnumFieldInfo<TEnum>> GetFields(TEnum value) {
			string[] flags = value.ToString().Split(new[] { ", " }, StringSplitOptions.None);
			foreach (string name in flags) {
				yield return nameLookup[name];
			}
		}

		public static IEnumerable<TAttr> GetAttributes<TAttr>(TEnum value)
			where TAttr : Attribute
		{
			string[] flags = value.ToString().Split(new[] { ", " }, StringSplitOptions.None);
			foreach (string name in flags) {
				yield return nameLookup[name].GetAttribute<TAttr>();
			}
		}

		public static IEnumerable<EnumFieldInfo<TEnum>> GetFields(string names) {
			string[] flags = names.Split(new[] { ',', '|', ' ' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string name in flags) {
				yield return nameLookup[name.Trim()];
			}
		}
	}

	internal class EnumFieldInfo<TEnum> where TEnum : struct {
		public FieldInfo FieldInfo { get; }

		public string Name => FieldInfo.Name;
		public TEnum Value { get; }
		public long LongValue { get; }
		public int BitCount { get; }

		internal EnumFieldInfo(FieldInfo field, bool isUInt64, int typeSize) {
			FieldInfo = field;
			Value = (TEnum) field.GetValue(null);
			if (isUInt64)
				LongValue = unchecked((long) Convert.ToUInt64(Value));
			else
				LongValue = unchecked(Convert.ToInt64(Value));
		}

		public TAttr GetAttribute<TAttr>() where TAttr : Attribute {
			return FieldInfo.GetCustomAttribute<TAttr>();
		}

		public TResult GetAttributeValue<TAttr, TResult>(Func<TAttr,TResult> getter)
			where TAttr : Attribute
		{
			TAttr attr = FieldInfo.GetCustomAttribute<TAttr>();
			if (attr == null)
				throw new ArgumentNullException(nameof(TAttr),
					$"{typeof(TEnum).Name}.{FieldInfo.Name} does not contain the " +
					$"attribute {typeof(TAttr).Name}!");
			return getter(attr);
		}

		public bool TryGetAttributeValue<TAttr, TResult>(Func<TAttr, TResult> getter,
			out TResult result) where TAttr : Attribute {
			TAttr attr = FieldInfo.GetCustomAttribute<TAttr>();
			if (attr == null) {
				result = default(TResult);
				return false;
			}
			result = getter(attr);
			return true;
		}

		public bool HasAttribute<TAttr>() where TAttr : Attribute {
			return FieldInfo.GetCustomAttribute<TAttr>() != null;
		}
	}
}
