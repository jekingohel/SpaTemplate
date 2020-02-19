// -----------------------------------------------------------------------
// <copyright file="RefitExceptions.cs" company="Piotr Xeinaemm Czech">
// Copyright (c) Piotr Xeinaemm Czech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace SpaTemplate.Functional.Tests.Helpers
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Refit;
    using Xunit;

    public static class RefitExceptions
    {
        public static void Verify(Action action, HttpStatusCode httpStatusCode)
        {
            try
            {
                action.Invoke();
            }
            catch (ApiException validationException)
            {
                Assert.Equal(httpStatusCode, validationException.StatusCode);
            }
        }
    }
}
