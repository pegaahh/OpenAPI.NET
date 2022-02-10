﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Hidi;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Services;
using Microsoft.OpenApi.Tests.UtilityFiles;
using Moq;
using Xunit;

namespace Microsoft.OpenApi.Tests.Services
{
    public class OpenApiFilterServiceTests
    {
        private readonly OpenApiDocument _openApiDocumentMock;
        private readonly Mock<ILogger<OpenApiService>> _mockLogger;
        private readonly ILogger<OpenApiService> _logger;

        public OpenApiFilterServiceTests()
        {
            _openApiDocumentMock = OpenApiDocumentMock.CreateOpenApiDocument();
            _mockLogger = new Mock<ILogger<OpenApiService>>();
            _logger = _mockLogger.Object;
        }

        [Theory]
        [InlineData("users.user.ListUser", null, 1)]
        [InlineData("users.user.GetUser", null, 1)]
        [InlineData("users.user.ListUser,users.user.GetUser", null, 2)]
        [InlineData("*", null, 12)]
        [InlineData("administrativeUnits.restore", null, 1)]
        [InlineData("graphService.GetGraphService", null, 1)]
        [InlineData(null, "users.user,applications.application", 3)]
        [InlineData(null, "^users\\.", 3)]
        [InlineData(null, "users.user", 2)]
        [InlineData(null, "applications.application", 1)]
        [InlineData(null, "reports.Functions", 2)]
        public void ReturnFilteredOpenApiDocumentBasedOnOperationIdsAndTags(string operationIds, string tags, int expectedPathCount)
        {
            // Act
            var predicate = OpenApiFilterService.CreatePredicate(operationIds, tags);
            var subsetOpenApiDocument = OpenApiFilterService.CreateFilteredDocument(_openApiDocumentMock, predicate);

            // Assert
            Assert.NotNull(subsetOpenApiDocument);
            Assert.NotEmpty(subsetOpenApiDocument.Paths);
            Assert.Equal(expectedPathCount, subsetOpenApiDocument.Paths.Count);
        }

        [Fact]
        public void ReturnFilteredOpenApiDocumentBasedOnPostmanCollection()
        {
            // Arrange
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UtilityFiles\\postmanCollection_ver2.json");
            var fileInput = new FileInfo(filePath);
            var stream = fileInput.OpenRead();

            // Act
            var requestUrls = OpenApiService.ParseJsonCollectionFile(stream, _logger);
            var predicate = OpenApiFilterService.CreatePredicate(requestUrls: requestUrls, source: _openApiDocumentMock);
            var subsetOpenApiDocument = OpenApiFilterService.CreateFilteredDocument(_openApiDocumentMock, predicate);

            // Assert
            Assert.NotNull(subsetOpenApiDocument);
            Assert.NotEmpty(subsetOpenApiDocument.Paths);
            Assert.Equal(3, subsetOpenApiDocument.Paths.Count);
        }

        [Fact]
        public void ThrowsExceptionWhenUrlsInCollectionAreMissingFromSourceDocument()
        {
            // Arrange
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UtilityFiles\\postmanCollection_ver1.json");
            var fileInput = new FileInfo(filePath);
            var stream = fileInput.OpenRead();

            // Act
            var requestUrls = OpenApiService.ParseJsonCollectionFile(stream, _logger);

            // Assert
            var message = Assert.Throws<ArgumentException>(() =>
                OpenApiFilterService.CreatePredicate(requestUrls: requestUrls, source: _openApiDocumentMock)).Message;
            Assert.Equal("The urls in the Postman collection supplied could not be found.", message);
        }

        [Fact]
        public void ThrowsInvalidOperationExceptionInCreatePredicateWhenInvalidArgumentsArePassed()
        {
            // Act and Assert
            var message1 = Assert.Throws<InvalidOperationException>(() => OpenApiFilterService.CreatePredicate(null, null)).Message;
            Assert.Equal("Either operationId(s),tag(s) or Postman collection need to be specified.", message1);

            var message2 = Assert.Throws<InvalidOperationException>(() => OpenApiFilterService.CreatePredicate("users.user.ListUser", "users.user")).Message;
            Assert.Equal("Cannot specify both operationIds and tags at the same time.", message2);
        }
    }
}