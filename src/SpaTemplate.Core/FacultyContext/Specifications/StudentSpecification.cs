﻿// -----------------------------------------------------------------------
// <copyright file="StudentSpecification.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Core.FacultyContext
{
	using System;
	using SpaTemplate.Core.SharedKernel;

	public sealed class StudentSpecification : BaseSpecification<Student>
	{
		public StudentSpecification(Guid studentId)
			: base(student => student.Id == studentId)
		{
			this.AddInclude(student => student.Courses);
		}
	}
}