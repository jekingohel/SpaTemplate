// -----------------------------------------------------------------------
// <copyright file="IdentityServerSeedDataConfiguration.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Infrastructure
{
	using System.Collections.Generic;
	using IdentityServer4.Models;
	using Xeinaemm.Identity;

	public class IdentityServerSeedDataConfiguration : IIdentityServerSeedDataConfiguration
	{
		public IEnumerable<ApiResource> GetApiResources() => throw new System.NotImplementedException();

		public IEnumerable<Client> GetClients() => throw new System.NotImplementedException();

		public IEnumerable<IdentityResource> GetIdentityResources() => throw new System.NotImplementedException();
	}
}
