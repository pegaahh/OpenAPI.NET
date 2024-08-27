﻿using System.Collections.Generic;
using System.Globalization;
using System.IO;
using FluentAssertions;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.References;
using Microsoft.OpenApi.Reader;
using Microsoft.OpenApi.Tests;
using Microsoft.OpenApi.Writers;
using Xunit;

namespace Microsoft.OpenApi.Readers.Tests.V31Tests
{
    public class OpenApiDocumentTests
    {
        private const string SampleFolderPath = "V31Tests/Samples/OpenApiDocument/";

        public OpenApiDocumentTests()
        {
            OpenApiReaderRegistry.RegisterReader(OpenApiConstants.Yaml, new OpenApiYamlReader());
        }

        [Fact]
        public void ParseDocumentWithWebhooksShouldSucceed()
        {
            // Arrange and Act
            var actual = OpenApiDocument.Load(Path.Combine(SampleFolderPath, "documentWithWebhooks.yaml"));
            var petSchema = new OpenApiSchemaReference("petSchema", actual.OpenApiDocument);

            var newPetSchema = new OpenApiSchemaReference("newPetSchema", actual.OpenApiDocument);

            var components = new OpenApiComponents
            {
                Schemas =
                {
                    ["petSchema"] =  new()
                    {
                        Type = "object",
                        Required = new HashSet<string>
                        {
                            "id",
                            "name"
                        },
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            ["id"] = new()
                            {
                                Type = "integer",
                                Format = "int64"
                            },
                            ["name"] = new()
                            {
                                Type = "string"
                            },
                            ["tag"] = new()
                            {
                                Type = "string"
                            },
                        }
                    },
                    ["newPetSchema"] = new()
                    {
                        Type = "object",
                        Required = new HashSet<string>
                        {
                            "name"
                        },
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            ["id"] = new()
                            {
                                Type = "integer",
                                Format = "int64"
                            },
                            ["name"] = new()
                            {
                                Type = "string"
                            },
                            ["tag"] = new()
                            {
                                Type = "string"
                            },
                        }
                    }
                }
            };

            var expected = new OpenApiDocument
            {
                Info = new OpenApiInfo
                {
                    Version = "1.0.0",
                    Title = "Webhook Example"
                },
                Webhooks = new Dictionary<string, OpenApiPathItem>
                {
                    ["pets"] = new OpenApiPathItem
                    {
                        Operations = new Dictionary<OperationType, OpenApiOperation>
                        {
                            [OperationType.Get] = new OpenApiOperation
                            {
                                Description = "Returns all pets from the system that the user has access to",
                                OperationId = "findPets",
                                Parameters = new List<OpenApiParameter>
                                    {
                                        new OpenApiParameter
                                        {
                                            Name = "tags",
                                            In = ParameterLocation.Query,
                                            Description = "tags to filter by",
                                            Required = false,
                                            Schema = new()
                                            {
                                                Type = "array",
                                                Items = new()
                                                {
                                                    Type = "string"
                                                }
                                            }
                                        },
                                        new OpenApiParameter
                                        {
                                            Name = "limit",
                                            In = ParameterLocation.Query,
                                            Description = "maximum number of results to return",
                                            Required = false,
                                            Schema = new()
                                            {
                                                Type = "integer",
                                                Format = "int32"
                                            }
                                        }
                                    },
                                Responses = new OpenApiResponses
                                {
                                    ["200"] = new OpenApiResponse
                                    {
                                        Description = "pet response",
                                        Content = new Dictionary<string, OpenApiMediaType>
                                        {
                                            ["application/json"] = new OpenApiMediaType
                                            {
                                                Schema = new()
                                                {
                                                    Type = "array",
                                                    Items = petSchema
                                                }
                                            },
                                            ["application/xml"] = new OpenApiMediaType
                                            {
                                                Schema = new()
                                                {
                                                    Type = "array",
                                                    Items = petSchema
                                                }
                                            }
                                        }
                                    }
                                }
                            },
                            [OperationType.Post] = new OpenApiOperation
                            {
                                RequestBody = new OpenApiRequestBody
                                {
                                    Description = "Information about a new pet in the system",
                                    Required = true,
                                    Content = new Dictionary<string, OpenApiMediaType>
                                    {
                                        ["application/json"] = new OpenApiMediaType
                                        {
                                            Schema = newPetSchema
                                        }
                                    }
                                },
                                Responses = new OpenApiResponses
                                {
                                    ["200"] = new OpenApiResponse
                                    {
                                        Description = "Return a 200 status to indicate that the data was received successfully",
                                        Content = new Dictionary<string, OpenApiMediaType>
                                        {
                                            ["application/json"] = new OpenApiMediaType
                                            {
                                                Schema = petSchema
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                Components = components
            };

            // Assert            
            actual.OpenApiDiagnostic.Should().BeEquivalentTo(new OpenApiDiagnostic() { SpecificationVersion = OpenApiSpecVersion.OpenApi3_1 });
            actual.OpenApiDocument.Should().BeEquivalentTo(expected, options => options.Excluding(x => x.Workspace).Excluding(y => y.BaseUri));
        }

        [Fact]
        public void ParseDocumentsWithReusablePathItemInWebhooksSucceeds()
        {
            // Arrange && Act
            var actual = OpenApiDocument.Load("V31Tests/Samples/OpenApiDocument/documentWithReusablePaths.yaml");

            var components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, OpenApiSchema>
                {
                    ["petSchema"] = new()
                    {
                        Type = "object",
                        Required = new HashSet<string>
                        {
                            "id",
                            "name"
                        },
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            ["id"] = new()
                            {
                                Type = "integer",
                                Format = "int64"
                            },
                            ["name"] = new()
                            {
                                Type = "string"
                            },
                            ["tag"] = new()
                            {
                                Type = "string"
                            },
                        }
                    },
                    ["newPetSchema"] = new()
                    {
                        Type = "object",
                        Required = new HashSet<string>
                        {
                            "name"
                        },
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            ["id"] = new()
                            {
                                Type = "integer",
                                Format = "int64"
                            },
                            ["name"] = new()
                            {
                                Type = "string"
                            },
                            ["tag"] = new()
                            {
                                Type = "string"
                            },
                        }
                    }
                }
            };

            // Create a clone of the schema to avoid modifying things in components.
            var petSchema = new OpenApiSchemaReference("petSchema", actual.OpenApiDocument);

            var newPetSchema = new OpenApiSchemaReference("newPetSchema", actual.OpenApiDocument);

            components.PathItems = new Dictionary<string, OpenApiPathItem>
            {
                ["pets"] = new OpenApiPathItem
                {
                    Operations = new Dictionary<OperationType, OpenApiOperation>
                    {
                        [OperationType.Get] = new OpenApiOperation
                        {
                            Description = "Returns all pets from the system that the user has access to",
                            OperationId = "findPets",
                            Parameters = new List<OpenApiParameter>
                                {
                                    new OpenApiParameter
                                    {
                                        Name = "tags",
                                        In = ParameterLocation.Query,
                                        Description = "tags to filter by",
                                        Required = false,
                                        Schema = new()
                                        {
                                            Type = "array",
                                            Items = new()
                                            {
                                                Type = "string"
                                            }
                                        }
                                    },
                                    new OpenApiParameter
                                    {
                                        Name = "limit",
                                        In = ParameterLocation.Query,
                                        Description = "maximum number of results to return",
                                        Required = false,
                                        Schema = new()
                                        {
                                            Type = "integer",
                                            Format = "int32"
                                        }
                                    }
                                },
                            Responses = new OpenApiResponses
                            {
                                ["200"] = new OpenApiResponse
                                {
                                    Description = "pet response",
                                    Content = new Dictionary<string, OpenApiMediaType>
                                    {
                                        ["application/json"] = new OpenApiMediaType
                                        {
                                            Schema = new OpenApiSchema
                                            {
                                                Type = "array",
                                                Items = petSchema
                                            }
                                        },
                                        ["application/xml"] = new OpenApiMediaType
                                        {
                                            Schema = new OpenApiSchema
                                            {
                                                Type = "array",
                                                Items = petSchema
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        [OperationType.Post] = new OpenApiOperation
                        {
                            RequestBody = new OpenApiRequestBody
                            {
                                Description = "Information about a new pet in the system",
                                Required = true,
                                Content = new Dictionary<string, OpenApiMediaType>
                                {
                                    ["application/json"] = new OpenApiMediaType
                                    {
                                        Schema = newPetSchema
                                    }
                                }
                            },
                            Responses = new OpenApiResponses
                            {
                                ["200"] = new OpenApiResponse
                                {
                                    Description = "Return a 200 status to indicate that the data was received successfully",
                                    Content = new Dictionary<string, OpenApiMediaType>
                                    {
                                        ["application/json"] = new OpenApiMediaType
                                        {
                                            Schema = petSchema
                                        },
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var expected = new OpenApiDocument
            {
                Info = new OpenApiInfo
                {
                    Title = "Webhook Example",
                    Version = "1.0.0"
                },
                JsonSchemaDialect = "http://json-schema.org/draft-07/schema#",
                Webhooks = new Dictionary<string, OpenApiPathItem>
                {
                    ["pets"] = components.PathItems["pets"]
                },
                Components = components
            };

            // Assert
            actual.OpenApiDocument.Should().BeEquivalentTo(expected, options => options
            .Excluding(x => x.Webhooks["pets"].Reference)
            .Excluding(x => x.Workspace)
            .Excluding(y => y.BaseUri));
            actual.OpenApiDiagnostic.Should().BeEquivalentTo(
    new OpenApiDiagnostic() { SpecificationVersion = OpenApiSpecVersion.OpenApi3_1 });

            var outputWriter = new StringWriter(CultureInfo.InvariantCulture);
            var writer = new OpenApiJsonWriter(outputWriter, new() { InlineLocalReferences = true } );
            actual.OpenApiDocument.SerializeAsV31(writer);
            var serialized = outputWriter.ToString();
        }

        [Fact]
        public void ParseDocumentWithExampleInSchemaShouldSucceed()
        {
            // Arrange
            var outputStringWriter = new StringWriter(CultureInfo.InvariantCulture);
            var writer = new OpenApiJsonWriter(outputStringWriter, new OpenApiJsonWriterSettings { Terse = false });

            // Act
            var actual = OpenApiDocument.Load(Path.Combine(SampleFolderPath, "docWithExample.yaml"));
            actual.OpenApiDocument.SerializeAsV31(writer);

            // Assert
            Assert.NotNull(actual);
        }

        [Fact]
        public void ParseDocumentWithPatternPropertiesInSchemaWorks()
        {
            // Arrange and Act
            var result = OpenApiDocument.Load(Path.Combine(SampleFolderPath, "docWithPatternPropertiesInSchema.yaml"));
            var actualSchema = result.OpenApiDocument.Paths["/example"].Operations[OperationType.Get].Responses["200"].Content["application/json"].Schema;

            var expectedSchema = new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    ["prop1"] = new OpenApiSchema
                    {
                        Type = "string"
                    },
                    ["prop2"] = new OpenApiSchema
                    {
                        Type = "string"
                    },
                    ["prop3"] = new OpenApiSchema
                    {
                        Type = "string"
                    }
                },
                PatternProperties = new Dictionary<string, OpenApiSchema>
                {
                    ["^x-.*$"] = new OpenApiSchema
                    {
                        Type = "string"
                    }
                }
            };
            
            // Serialization
            var mediaType = result.OpenApiDocument.Paths["/example"].Operations[OperationType.Get].Responses["200"].Content["application/json"];

            var expectedMediaType = @"schema:
  patternProperties:
    ^x-.*$:
      type: string
  type: object
  properties:
    prop1:
      type: string
    prop2:
      type: string
    prop3:
      type: string";
            
            var actualMediaType = mediaType.SerializeAsYaml(OpenApiSpecVersion.OpenApi3_1);

            // Assert
            actualSchema.Should().BeEquivalentTo(expectedSchema);
            actualMediaType.MakeLineBreaksEnvironmentNeutral().Should().BeEquivalentTo(expectedMediaType.MakeLineBreaksEnvironmentNeutral());
        }

        [Fact]
        public void ParseDocumentWithReferenceByIdGetsResolved()
        {
            // Arrange and Act
            var result = OpenApiDocument.Load(Path.Combine(SampleFolderPath, "docWithReferenceById.yaml"));

            var responseSchema = result.OpenApiDocument.Paths["/resource"].Operations[OperationType.Get].Responses["200"].Content["application/json"].Schema;
            var requestBodySchema = result.OpenApiDocument.Paths["/resource"].Operations[OperationType.Post].RequestBody.Content["application/json"].Schema;
            var parameterSchema = result.OpenApiDocument.Paths["/resource"].Operations[OperationType.Get].Parameters[0].Schema;

            // Assert
            Assert.Equal("object", responseSchema.Type);
            Assert.Equal("object", requestBodySchema.Type);
            Assert.Equal("string", parameterSchema.Type);
        }
    }
}
