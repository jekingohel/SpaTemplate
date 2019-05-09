// -----------------------------------------------------------------------
// <copyright file="IdentitySeedData.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Infrastructure
{
	using System.Collections.ObjectModel;
	using IdentityServer4.Models;
	using Xeinaemm.Identity;

	public class IdentitySeedData : IIdentitySeedData
	{
		public Collection<ApiResource> GetApiResources() => throw new System.NotImplementedException();

		public Collection<Client> GetClients() => throw new System.NotImplementedException();

		public Collection<IdentityResource> GetIdentityResources() => throw new System.NotImplementedException();
	}
}
