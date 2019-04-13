// -----------------------------------------------------------------------
// <copyright file="IStudentService.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Core.FacultyContext
{
	using System;
	using System.Collections.Generic;
	using SpaTemplate.Core.SharedKernel;

	public interface IStudentService
	{
		List<Student> GetCollection(IEnumerable<Guid> ids);

		Student GetStudent(Guid studentId);

		bool AddStudent(Student student);

		bool DeleteStudent(Student student);

		bool UpdateStudent(Student student);

		bool StudentExists(Guid studentId);

		bool StudentMappingExists<TParameters>(TParameters parameters)
			where TParameters : IParameters;

		bool StudentPropertiesExists<TParameters>(TParameters parameters)
			where TParameters : IParameters;

		PagedList<Student> GetPagedList<TParameters>(TParameters parameters)
			where TParameters : IParameters;
	}
}