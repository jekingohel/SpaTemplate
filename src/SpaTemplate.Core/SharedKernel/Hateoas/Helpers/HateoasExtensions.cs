using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace SpaTemplate.Core.SharedKernel
{
    public static class HateoasExtensions
    {
        public static IEnumerable<ExpandoObject> ShapeDataCollection<TSource>(
            this IEnumerable<TSource> source,
            string fields = "") where TSource : IShapeData
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
                        .GetProperty(propertyName, PublicInstances());

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

        public static IEnumerable<IDictionary<string, object>> ShapeDataCollectionWithHateoasLinks<TDto>(this IEnumerable<TDto> dtos,
            string fields,
            Func<Guid, string, IEnumerable<ILinkDto>> function) where TDto : IDto =>
            dtos.ShapeDataCollection(fields)
                .Select(dto =>
                {
                    var dictionary = dto as IDictionary<string, object>;
                    dictionary.Add(Constants.KeyLink, function((Guid) dictionary["Id"], fields));

                    return dictionary;
                });

        public static ExpandoObject ShapeDataObject<TSource>(this TSource source,
            string fields = "") where TSource : IShapeData
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

        public static BasePagination CreateBasePagination<T>(this IPagedList<T> pagedList)
            where T : BaseEntity => new BasePagination
        {
            TotalCount = pagedList.TotalCount,
            PageSize = pagedList.PageSize,
            CurrentPage = pagedList.CurrentPage,
            TotalPages = pagedList.TotalPages
        };

        public static LinkDto AddRelAndMethod(this string href, string relName,
            string methodName) =>
            new LinkDto(href, relName, methodName);

        private static BindingFlags PublicInstances() =>
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance;
    }
}