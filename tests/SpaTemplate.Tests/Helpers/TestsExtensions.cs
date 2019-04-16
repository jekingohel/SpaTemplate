// -----------------------------------------------------------------------
// <copyright file="TestsExtensions.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Tests.Helpers
{
	using System;
	using System.Collections.Generic;
	using System.Net.Http;
	using System.Text;
	using Newtonsoft.Json;

	public static class TestsExtensions
	{
		public static StringContent Content(this object obj, string mediaType) => new StringContent(
			JsonConvert.SerializeObject(obj),
			Encoding.UTF8,
			mediaType);

		public static string ToApiUrl(this string route, string fields = "") =>
			$"{route}{fields}";

		public static string ToApiUrl(this string route, Guid firstId, string fields = "") =>
				$"{string.Format(route, firstId.ToString())}{fields}";

		public static string ToApiUrl(this string route, IEnumerable<Guid> ids)
		{
			if (ids == null) return string.Format(route, " ");
			var sb = new StringBuilder();

			foreach (var guid in ids)
			{
				_ = sb.Append(guid.ToString());
				_ = sb.Append(",");
			}

			return string.Format(route, sb);
		}

		public static string ToApiUrl(this string route, Guid firstId, Guid secondId, string fields = "") =>
			$"{string.Format(route, firstId.ToString(), secondId.ToString())}{fields}";
	}
}
