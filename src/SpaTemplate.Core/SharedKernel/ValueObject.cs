using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SpaTemplate.Core
{
	// source: https://github.com/jhewlett/ValueObject
	public abstract class ValueObject : IEquatable<ValueObject>
	{
		private List<FieldInfo> _fields;
		private List<PropertyInfo> _properties;

		public bool Equals(ValueObject obj) => Equals(obj as object);

		public static bool operator ==(ValueObject obj1, ValueObject obj2) => obj1?.Equals(obj2) ?? Equals(obj2, null);

		public static bool operator !=(ValueObject obj1, ValueObject obj2) => !(obj1 == obj2);

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType()) return false;

			return GetProperties().All(p => PropertiesAreEqual(obj, p))
			       && GetFields().All(f => FieldsAreEqual(obj, f));
		}

		private bool PropertiesAreEqual(object obj, PropertyInfo p) =>
			Equals(p.GetValue(this, null), p.GetValue(obj, null));

		private bool FieldsAreEqual(object obj, FieldInfo f) => Equals(f.GetValue(this), f.GetValue(obj));

		private IEnumerable<PropertyInfo> GetProperties() => _properties ?? (_properties = GetType()
			                                                     .GetProperties(
				                                                     BindingFlags.Instance | BindingFlags.Public)
			                                                     .Where(p =>
				                                                     p.GetCustomAttribute(
					                                                     typeof(IgnoreMemberAttribute)) == null)
			                                                     .ToList());

		private IEnumerable<FieldInfo> GetFields() =>
			_fields ?? (_fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public)
				.Where(p => p.GetCustomAttribute(typeof(IgnoreMemberAttribute)) == null)
				.ToList());

		public override int GetHashCode()
		{
			var hash = GetProperties().Select(prop => prop.GetValue(this, null)).Aggregate(17, HashValue);

			return GetFields().Select(field => field.GetValue(this)).Aggregate(hash, HashValue);
		}

		private static int HashValue(int seed, object value) => seed * 23 + value?.GetHashCode() ?? 0;
	}
}