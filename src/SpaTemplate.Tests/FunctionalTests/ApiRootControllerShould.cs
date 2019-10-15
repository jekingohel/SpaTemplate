// -----------------------------------------------------------------------
// <copyright file="ApiRootControllerShould.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Tests.FunctionalTests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using SpaTemplate.Core.SharedKernel;
    using SpaTemplate.Infrastructure.Api;
    using SpaTemplate.Tests.Helpers;
    using Xeinaemm.Hateoas;
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
    public sealed class ApiRootControllerShould : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly HttpClient client;

        public ApiRootControllerShould(CustomWebApplicationFactory<Startup> factory)
        {
            this.client = factory.CreateClientWithDefaultRequestHeaders();
        }

        [Theory]
        [InlineData(Rel.Self, Method.Get, 0)]
        [InlineData(SpaTemplateRel.People, Method.Get, 1)]
        [InlineData(SpaTemplateRel.CreateStudent, Method.Post, 2)]
        public async Task ReturnsHateoasLinksRootAsync(string rel, string method, int number)
        {
            this.client.DefaultRequestHeaders.TryAddWithoutValidation(Header.Accept, MediaType.OutputFormatterJson);
            var response = await this.client.GetAsync(Route.RootApi).ConfigureAwait(false);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JsonConvert.DeserializeObject<List<LinkDto>>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));

            Assert.Equal(3, result.Count);

            Assert.Equal(rel, result[number].Rel);
            Assert.Equal(method, result[number].Method);
        }
    }
}