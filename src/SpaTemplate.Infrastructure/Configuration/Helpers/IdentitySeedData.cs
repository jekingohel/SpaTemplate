// -----------------------------------------------------------------------
// <copyright file="IdentitySeedData.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Infrastructure
{
	using System.Collections.Generic;
	using IdentityServer4.Models;
	using Xeinaemm.Identity;

	public class IdentitySeedData : IIdentitySeedData
	{
		public IEnumerable<IdentityResource> IdentityResources { get; } =
			new List<IClientParameters>
			{
				new ClientParameters(),
			}.Identity();

		public IEnumerable<ApiResource> ApiResources { get; } =
			new List<IApiParameters>
			{
				new ApiParameters(),
			}.Api();

		public IEnumerable<Client> Clients { get; } =
			new List<Client>
			{
				new ClientParameters().Mvc(),
			};

		public IEnumerable<IUser> Users { get; } = new List<IUser>
		{
			new Alice(),
			new Bob(),
		};
	}
}
