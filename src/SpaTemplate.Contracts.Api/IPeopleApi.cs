// -----------------------------------------------------------------------
// <copyright file="IPeopleApi.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Contracts.Api
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.JsonPatch;
    using Refit;
    using SpaTemplate.Contracts.Models;
    using SpaTemplate.Contracts.Parameters;
    using Xeinaemm.Hateoas;

    public interface IPeopleApi
    {
        [Post("/api/v1/people")]
        Task<StudentDto> CreateStudent([Body] StudentForCreationDto studentForCreationDto);

        [Delete("/api/v1/people/{id}")]
        Task DeleteStudent(Guid id);

        [Get("/api/v1/people/{id}")]
        Task<StudentDto> GetStudent(Guid id, StudentParameters parameters);

        [Get("/api/v1/people/{id}")]
        Task<HateoasCollectionDto<StudentDto>> GetStudent(Guid id, StudentParameters parameters, [Header("Accept")] string mediaType);

        [Get("/api/v1/people")]
        Task<IEnumerable<StudentDto>> GetPeople(StudentParameters parameters);

        [Get("/api/v1/people")]
        Task<HateoasCollectionDto<StudentDto>> GetPeople(StudentParameters parameters, [Header("Accept")] string mediaType);

        [Get("/api/v1/people/{id}")]
        Task<StudentDto> PartiallyUpdateStudent(Guid id, [Body] JsonPatchDocument<StudentForUpdateDto> patchDoc);
    }
}
