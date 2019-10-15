// -----------------------------------------------------------------------
// <copyright file="ApiPeopleCollectionsControllerShould.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Tests.FunctionalTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using SpaTemplate.Core.FacultyContext;
    using SpaTemplate.Core.SharedKernel;
    using SpaTemplate.Infrastructure.Api;
    using SpaTemplate.Tests.Helpers;
    using Xeinaemm.Hateoas;
    using Xeinaemm.Tests;
    using Xunit;

    /// <inheritdoc />
    /// <summary>
    /// <para>
    ///     A - Standard calls to API without combinations(POST + GET, POST + DELETE etc.)
    ///     B - Extended calls that test API in isolation without prepared data in the database to remove hardcoded values.
    ///         Firstly we call POST to prepare object and then exercise another action(ie. DELETE).
    ///     C - If you exercise more actions (ie. 3) then use next letter to distinct number of calls, ie. C = 3 D = 4, E = 5 etc.
    ///     Number defines tests for particular action(ie. GET)
    ///     1 - GET,
    ///     2 - POST,
    ///     3 - PATCH,
    ///     4 - PUT,
    ///     5 - DELETE.
    /// </para>
    /// <para>    The functional test needs to be tested in particular order due to domino's effect of API, ie. if POST will fail, others too.</para>
    /// </summary>
    public sealed partial class ApiPeopleCollectionsControllerShould : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly HttpClient client;

        public ApiPeopleCollectionsControllerShould(CustomWebApplicationFactory<Startup> factory)
        {
            this.client = factory.CreateClientWithDefaultRequestHeaders();
        }

        [Theory]
        [AutoMoqData]
        public async Task CreateCollectionAfterValidPostAsync(IEnumerable<StudentForManipulationDto> dtos)
        {
            var post = await this.PostAsync(dtos).ConfigureAwait(false);
            Assert.Equal(HttpStatusCode.Created, post.StatusCode);

            var dtoList =
                JsonConvert.DeserializeObject<IEnumerable<StudentForCreationDto>>(
                    await post.Content.ReadAsStringAsync().ConfigureAwait(false));
            Assert.Equal(3, dtoList.Count());
        }

        [Fact]
        public async Task ReturnsBadRequestNoIdsAsync()
        {
            var get = await this.GetAsync().ConfigureAwait(false);

            Assert.Equal(HttpStatusCode.BadRequest, get.StatusCode);
        }

        [Theory]
        [AutoMoqData]
        public async Task ReturnsCollectionAfterValidGetAsync(IEnumerable<StudentForManipulationDto> dtos)
        {
            var post = await this.PostAsync(dtos).ConfigureAwait(false);
            Assert.Equal(HttpStatusCode.Created, post.StatusCode);

            var dtoList =
                JsonConvert.DeserializeObject<IEnumerable<StudentDto>>(await post.Content.ReadAsStringAsync().ConfigureAwait(false));

            var get = await this.GetAsync(dtoList.Select(x => x.Id).Take(3)).ConfigureAwait(false);

            Assert.Equal(HttpStatusCode.OK, get.StatusCode);

            var dtoCollection =
                JsonConvert.DeserializeObject<List<StudentDto>>(await get.Content.ReadAsStringAsync().ConfigureAwait(false));

            Assert.Equal(3, dtoCollection.Count);
        }

        [Fact]
        public async Task ReturnsNotFoundIdsNotExistAsync()
        {
            var ids = new List<Guid>
            {
                Guid.NewGuid(),
                Guid.NewGuid(),
            };
            var get = await this.GetAsync(ids).ConfigureAwait(false);

            Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
        }
    }

    public partial class ApiPeopleCollectionsControllerShould
    {
        private Task<HttpResponseMessage> GetAsync(IEnumerable<Guid> ids = null) =>
            this.client.GetAsync(new Uri(Api.StudentCollectionsIds).ToApiUrl(ids));

        private Task<HttpResponseMessage> PostAsync(IEnumerable<StudentForManipulationDto> dtos) =>
            this.client.PostAsync(new Uri(Api.StudentCollections), dtos.Content(MediaType.InputFormatterJson));
    }
}