﻿using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Sample.Api.Net30.Tests
{
    public class CustomStartupConfigurationsTests
    {
        [Fact]
        public async Task GetException_WhenDeveloperPageIsConfigured_ShouldBeInternalServerError()
        {
            // Arrange
            var builder = new WebHostBuilder();
            builder.ConfigureServices(services =>
            {
                services.AddRouting();
            });
            builder.Configure(app => app
                .UseDeveloperExceptionPage()
                .UseRouting()
                .UseEndpoints(endpoints =>
                {
                    endpoints.Map("/exception", context =>
                    {
                        throw new Exception("Wow!", new Exception("Exactly!"));
                    });
                }));
            using var testServer = new TestServer(builder);
            using var client = testServer.CreateClient();

            // Act
            using var response = await client.GetAsync("/exception");

            // Assert
            response.Should().Be500InternalServerError();
        }

        [Fact]
        public async Task AssertingWithFluentAssertions_WhenRequestHasContent_ShouldPrintContent()
        {
            // Arrange
            var builder = new WebHostBuilder();
            builder.ConfigureServices(services =>
            {
                services.AddRouting();
            });
            builder.Configure(app => app.UseRouting());
            using var testServer = new TestServer(builder);
            using var client = testServer.CreateClient();

            // Act
            using var response = await client.PostAsync("/endpoint", new StringContent("request body"));

            // Assert
            using var scope = new AssertionScope();
            response.Should().Be200Ok();
            var failures = scope.Discard()[0];
            failures.Should().Match("*request body*");
        }

        [Fact]
        public async Task AssertingWithFluentAssertions_WhenClientIsCreatedWithWebApplicationFactory_ShouldPrintContent()
        {
            // Arrange
            using var webApplicationFactory = new WebApplicationFactory<Startup>();
            using var client = webApplicationFactory.CreateClient();

            // Act
            using var response = await client.PostAsync("/endpoint", new StringContent("request body"));

            // Assert
            using var scope = new AssertionScope();
            response.Should().Be200Ok();
            var failures = scope.Discard()[0];
            failures.Should().Match("*request body*");
        }
    }
}
