using System;
using System.Collections.Generic;
using AutoMapper;
using SpaTemplate.Core;

namespace SpaTemplate.Infrastructure
{
	public static class IDictionaryExtensions
	{
		public static IDictionary<string, object> ShapeDataWithoutParameters<TDto, TEntity>(this TEntity entity,
			Func<Guid, string, IEnumerable<ILinkDto>> function, string fields = null)
			where TDto : IDto where TEntity : BaseEntity
		{
			var dictionary = (IDictionary<string, object>) Mapper.Map<TDto>(entity).ShapeData(fields);
			dictionary.Add(Constants.KeyLink, function(entity.Id, fields));
			return dictionary;
		}
	}
}