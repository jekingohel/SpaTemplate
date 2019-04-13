// -----------------------------------------------------------------------
// <copyright file="BundleConfig.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Web.Framework
{
	using System.Web.Optimization;

	public static class BundleConfig
	{
		public static void RegisterBundles(BundleCollection bundles)
		{
			bundles
				.Add(new ScriptBundle("~/css")
					.Include(
						"~/ClientApp/dist/styles*"));

			bundles
				.Add(new ScriptBundle("~/app")
					.Include(
						"~/ClientApp/dist/main*",
						"~/ClientApp/dist/polyfills*",
						"~/ClientApp/dist/runtime*",
						"~/ClientApp/dist/vendor*"));
			BundleTable.EnableOptimizations = false;
		}
	}
}