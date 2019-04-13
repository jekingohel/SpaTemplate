// -----------------------------------------------------------------------
// <copyright file="CourseCompletedEvent.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Core.FacultyContext
{
	using SpaTemplate.Core.SharedKernel;

	public class CourseCompletedEvent : BaseDomainEvent
	{
		public CourseCompletedEvent(Course completedItem) => this.CompletedItem = completedItem;

		public Course CompletedItem { get; set; }
	}
}
