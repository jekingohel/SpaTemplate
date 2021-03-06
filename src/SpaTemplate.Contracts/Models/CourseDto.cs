﻿// -----------------------------------------------------------------------
// <copyright file="CourseDto.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Contracts.Models
{
    using System;
    using System.Collections.Generic;
    using Xeinaemm.Domain;
    using Xeinaemm.Hateoas;

    public class CourseDto : IDto
    {
        public string Description { get; set; }

        public Guid Id { get; set; }

        public string Title { get; set; }

        public List<BaseDomainEvent> Events { get; }
    }
}