// -----------------------------------------------------------------------
// <copyright file="StudentForUpdateDto.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Core.FacultyContext
{
	using System.Collections.Generic;

	public class StudentForUpdateDto : StudentForManipulationDto
	{
		public List<CourseForUpdateDto> CourseForUpdateDtos { get; set; }
			= new List<CourseForUpdateDto>();
	}
}