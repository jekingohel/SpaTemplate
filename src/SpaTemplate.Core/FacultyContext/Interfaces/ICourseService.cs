// -----------------------------------------------------------------------
// <copyright file="ICourseService.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Core.FacultyContext
{
	using System;
	using SpaTemplate.Core.SharedKernel;

	public interface ICourseService
	{
		PagedList<Course> GetPagedList<TParameters>(Guid studentId, TParameters parameters)
			where TParameters : IParameters;

		Course GetCourse(Guid studentId, Guid courseId);

		bool AddCourse(Guid studentId, Course course);

		bool DeleteCourse(Course course);

		bool UpdateCourse(Course course);

		bool StudentExists(Guid studentId);

		bool CourseMappingExists<TParameters>(TParameters parameters)
			where TParameters : IParameters;

		bool CoursePropertiesExists<TParameters>(TParameters parameters)
			where TParameters : IParameters;
	}
}