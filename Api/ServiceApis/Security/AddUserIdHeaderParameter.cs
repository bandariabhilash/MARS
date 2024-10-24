using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
namespace ServiceApis.Security
{
    public class AddUserIdHeaderParameter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.MethodInfo.DeclaringType == typeof(UserJobController))
            {
                if (operation.Parameters == null)
                {
                    operation.Parameters = new List<OpenApiParameter>();
                }

                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "userid",
                    In = ParameterLocation.Header,
                    Required = true,
                    Description = "Enter the user ID."
                });
            }
        }
    }
}
