// -----------------------------------------------------------------------
// <copyright file="IdentitySeedDataDevelopment.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Infrastructure
{
	using System;
	using System.Collections.Generic;
	using IdentityServer4.Test;
	using Xeinaemm.Identity;

	public class IdentitySeedDataDevelopment : IdentitySeedData, IIdentitySeedDataDevelopment
	{
		public List<TestUser> GetTestUsers() => throw new NotImplementedException();
	}
}
