using Core.CustomEntities;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebUtilities;
using System.Net;
using System.Reflection;

namespace Infrastructure.Filters
{

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class ValidateAttribute : Attribute
    {
    }
    public class ValidationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context == null) {
                return;
            }

            if (!context.ModelState.IsValid) {
                context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Result = new CustomResponseResult(StatusCodes.Status400BadRequest, ReasonPhrases.GetReasonPhrase(StatusCodes.Status400BadRequest),
                    context.ModelState.Values.SelectMany(v => v.Errors, (e, error) => error.ErrorMessage).Aggregate(string.Empty, (current, next) => $"{current} - {next}"));
                return;
            }
            await next();
        }

        public static EndpointFilterDelegate ValidationFilterFactory(EndpointFilterFactoryContext context, EndpointFilterDelegate next)
        {
            IEnumerable<ValidationDescriptor> validationDescriptors = GetValidators(context.MethodInfo, context.ApplicationServices);

            if (validationDescriptors.Any())
            {
                return invocationContext => ValidateAsync(validationDescriptors, invocationContext, next);
            }

            // pass-thru
            return invocationContext => next(invocationContext);
        }

        private static async ValueTask<object?> ValidateAsync(IEnumerable<ValidationDescriptor> validationDescriptors, EndpointFilterInvocationContext invocationContext, EndpointFilterDelegate next)
        {
            foreach (ValidationDescriptor descriptor in validationDescriptors)
            {
                var argument = invocationContext.Arguments[descriptor.ArgumentIndex];

                if (argument is not null)
                {
                    var validationResult = await descriptor.Validator.ValidateAsync(
                        new ValidationContext<object>(argument)
                    );

                    if (!validationResult.IsValid)
                    {
                        return Results.ValidationProblem(validationResult.ToDictionary(),
                            statusCode: (int)HttpStatusCode.UnprocessableEntity);
                    }
                }
            }

            return await next.Invoke(invocationContext);
        }

        static IEnumerable<ValidationDescriptor> GetValidators(MethodBase methodInfo, IServiceProvider serviceProvider)
        {
            ParameterInfo[] parameters = methodInfo.GetParameters();

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo parameter = parameters[i];

                if (parameter.GetCustomAttribute<ValidateAttribute>() is not null)
                {
                    Type validatorType = typeof(IValidator<>).MakeGenericType(parameter.ParameterType);

                    // Note that FluentValidation validators needs to be registered as singleton
                    IValidator? validator = serviceProvider.GetService(validatorType) as IValidator;

                    if (validator is not null)
                    {
                        yield return new ValidationDescriptor { ArgumentIndex = i, ArgumentType = parameter.ParameterType, Validator = validator };
                    }
                }
            }
        }

        private class ValidationDescriptor
        {
            public required int ArgumentIndex { get; init; }
            public required Type ArgumentType { get; init; }
            public required IValidator Validator { get; init; }
        }  
    }
}
