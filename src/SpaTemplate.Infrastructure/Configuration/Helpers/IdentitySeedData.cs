// -----------------------------------------------------------------------
// <copyright file="IdentitySeedData.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Infrastructure
{
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using IdentityServer4.Models;
	using Xeinaemm.Identity;

	public class IdentitySeedData : IIdentitySeedData
	{
		public Collection<IdentityResource> IdentityResources { get; }

		public Collection<ApiResource> ApiResources { get; }

		public Collection<Client> Clients { get; }

		public List<IUser> Users { get; }

	}
}
