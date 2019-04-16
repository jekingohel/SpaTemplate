// -----------------------------------------------------------------------
// <copyright file="Api.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Core.SharedKernel
{
	public static class Api
	{
		public static readonly string People = "api/people";
		public static readonly string Student = "api/people/{0}";
		public static readonly string Courses = "api/people/{0}/courses";
		public static readonly string Course = "api/people/{0}/courses/{1}";
		public static readonly string StudentCollections = "api/studentcollections";
		public static readonly string StudentCollectionsIds = "api/studentcollections/({0})";
	}
}