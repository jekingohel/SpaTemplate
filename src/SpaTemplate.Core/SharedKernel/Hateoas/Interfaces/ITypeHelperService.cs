﻿// -----------------------------------------------------------------------
// <copyright file="ITypeHelperService.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Core.SharedKernel
{
	public interface ITypeHelperService
	{
		bool TypeHasProperties<T>(string fields)
			where T : IDto;
	}
}