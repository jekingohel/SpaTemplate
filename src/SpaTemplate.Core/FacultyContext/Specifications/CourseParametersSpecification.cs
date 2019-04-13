// -----------------------------------------------------------------------
// <copyright file="CourseParametersSpecification.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Core.FacultyContext
{
	using System;
	using SpaTemplate.Core.SharedKernel;

	public sealed class CourseParametersSpecification : BaseSpecification<Course>
	{
		public CourseParametersSpecification(IParameters parameters, Guid studentId)
			: base(course =>
				CriteriaExpression(course, parameters, studentId))
		{
			this.AddInclude(course => course.Student);
		}

		private static bool CriteriaExpression(Course course, IParameters parameters, Guid studentId)
		{
			if (parameters.SearchQuery == null) return true;
			return course.Student.Id == studentId
				   && (course.Title.IndexOf(parameters.SearchQuery.Trim(), StringComparison.InvariantCultureIgnoreCase)
					>= 0 || course.Description.IndexOf(
						parameters.SearchQuery.Trim(),
						StringComparison.InvariantCultureIgnoreCase) >= 0);
		}
	}
}