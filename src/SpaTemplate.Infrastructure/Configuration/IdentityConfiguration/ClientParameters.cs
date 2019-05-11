// -----------------------------------------------------------------------
// <copyright file="ClientParameters.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Infrastructure
{
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using Xeinaemm.Identity;

	public class ClientParameters : IClientParameters
	{
		public ClientParameters()
		{
		}

		public ClientParameters(string clientSecret, string clientAuthority)
		{
			this.Secret = clientSecret;
			this.Authority = clientAuthority;
			this.ClaimTypes = new Collection<string>();
		}

		public string Name { get; } = "E29B3139-AB17-485A-9712-F9F70777CB0A";

		public string DisplayName { get; } = "SpaTemplate.Web.Core";

		public string Secret { get; }

		public string Authority { get; }

		public IEnumerable<string> ClaimTypes { get; }

		public IEnumerable<string> ApisNames { get; } = new Collection<string>
		{
			new ApiParameters().Name,
		};
	}
}
