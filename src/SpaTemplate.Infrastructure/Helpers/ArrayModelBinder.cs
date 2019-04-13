// -----------------------------------------------------------------------
// <copyright file="ArrayModelBinder.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Infrastructure
{
	using System.Threading.Tasks;
	using Microsoft.AspNetCore.Mvc.ModelBinding;

	public class ArrayModelBinder : IModelBinder
	{
		public Task BindModelAsync(ModelBindingContext bindingContext)
		{
			if (!bindingContext.ModelMetadata.IsEnumerableType) return bindingContext.FailedResultAsync();
			if (string.IsNullOrWhiteSpace(bindingContext.InputtedValue())) return bindingContext.NullResultAsync();

			var typedValues = bindingContext.CreateArrayOfCertainType();
			bindingContext.ConvertItemsToEnumerableType().CopyTo(typedValues, 0);
			bindingContext.Model = typedValues;

			return bindingContext.SuccessResultAsync();
		}
	}
}