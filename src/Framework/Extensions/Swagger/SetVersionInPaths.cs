﻿using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Framework.Extensions.Swagger;

public class SetVersionInPaths : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        OpenApiPaths updatedPaths = new();

        foreach (var entry in swaggerDoc.Paths)
            updatedPaths.Add(
                entry.Key.Replace("v{version}", swaggerDoc.Info.Version),
                entry.Value);

        swaggerDoc.Paths = updatedPaths;
    }
}