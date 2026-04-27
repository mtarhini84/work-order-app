using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace WorkOrderApp.Helpers;

internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider)
	: IOpenApiDocumentTransformer
{
	public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
	{
		var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();

		if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
		{
			var bearerScheme = new OpenApiSecurityScheme
			{
				Type = SecuritySchemeType.Http,
				Scheme = "bearer",
				BearerFormat = "JWT",
				In = ParameterLocation.Header,
				Description = "Put **_ONLY_** your JWT Bearer token in the box below!"
			};

			document.Components ??= new OpenApiComponents();
			document.AddComponent("Bearer", bearerScheme);

			var securityRequirement = new OpenApiSecurityRequirement
			{
				[new OpenApiSecuritySchemeReference("Bearer", document)] = []
			};

			foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations))
			{
				operation.Value.Security ??= new List<OpenApiSecurityRequirement>();
				operation.Value.Security.Add(securityRequirement);
			}
		}
	}
}