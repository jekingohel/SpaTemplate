using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace SpaTemplate.Core
{
	public static class IEnumerableExtensions
	{
		public static IEnumerable<ExpandoObject> ShapeData<TSource>(
			this IEnumerable<TSource> source,
			string fields)
		{
			if (source == null)
				throw new NullReferenceException($"Collection {nameof(source)} cannot be null");

			var expandoObjectList = new List<ExpandoObject>();
			var propertyInfoList = new List<PropertyInfo>();

			if (string.IsNullOrWhiteSpace(fields))
			{
				var propertyInfos = typeof(TSource)
					.GetProperties(BindingFlags.Public | BindingFlags.Instance);

				propertyInfoList.AddRange(propertyInfos);
			}
			else
			{
				var fieldsAfterSplit = fields.Split(',');

				foreach (var field in fieldsAfterSplit)
				{
					var propertyName = field.Trim();
					var propertyInfo = typeof(TSource)
						.GetProperty(propertyName,
							BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

					if (propertyInfo == null)
						throw new NullReferenceException($"Collection {nameof(propertyInfo)} cannot be null");
					propertyInfoList.Add(propertyInfo);
				}
			}

			foreach (var sourceObject in source)
			{
				var dataShapedObject = new ExpandoObject();

				foreach (var propertyInfo in propertyInfoList)
				{
					var propertyValue = propertyInfo.GetValue(sourceObject);
					((IDictionary<string, object>) dataShapedObject).Add(propertyInfo.Name, propertyValue);
				}

				expandoObjectList.Add(dataShapedObject);
			}

			return expandoObjectList;
		}

		public static IEnumerable<IDictionary<string, object>> GetShapedDtoWithLinks<T>(this IEnumerable<T> dtos,
			IParameters parameters,
			Func<Guid, string, IEnumerable<ILinkDto>> function) where T : IDto =>
			dtos.ShapeData(parameters.Fields)
				.Select(dto =>
				{
					var dictionary = dto as IDictionary<string, object>;
					dictionary.Add(Constants.KeyLink, function((Guid) dictionary["Id"], parameters.Fields));

					return dictionary;
				});
	}
}