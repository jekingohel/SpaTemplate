﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;

namespace SpaTemplate.Core
{
	public static class ObjectExtensions
	{
		public static ExpandoObject ShapeData<TSource>(this TSource source,
			string fields)
		{
			if (source == null)
				throw new NullReferenceException($"Collection {nameof(source)} cannot be null");

			var dataShapedObject = new ExpandoObject();

			if (string.IsNullOrWhiteSpace(fields))
			{
				var propertyInfos = typeof(TSource)
					.GetProperties(PublicInstances());

				foreach (var propertyInfo in propertyInfos)
				{
					var propertyValue = propertyInfo.GetValue(source);
					((IDictionary<string, object>) dataShapedObject).Add(propertyInfo.Name, propertyValue);
				}

				return dataShapedObject;
			}

			var fieldsAfterSplit = fields.Split(',');

			foreach (var field in fieldsAfterSplit)
			{
				var propertyName = field.Trim();

				var propertyInfo = typeof(TSource)
					.GetProperty(propertyName, PublicInstances());

				if (propertyInfo == null)
					throw new NullReferenceException($"Collection {nameof(propertyInfo)} cannot be null");

				var propertyValue = propertyInfo.GetValue(source);
				((IDictionary<string, object>) dataShapedObject).Add(propertyInfo.Name, propertyValue);
			}

			return dataShapedObject;
		}

		private static BindingFlags PublicInstances() =>
			BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance;
	}
}