using FluentValidation;
using MediatR;

namespace GroomerManager.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IList<IValidator<TRequest>> _validators = validators.ToList();

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var errors = _validators
            .Select(v => v.Validate(context))
            .SelectMany(v => v.Errors)
            .Where(e => e != null)
            .GroupBy(e => new { e.PropertyName, e.ErrorMessage })
            .ToList();

        if (errors.Any())
        {
            throw new Exceptions.ValidationException()
            {
                Errors = errors.Select(e => new Exceptions.ValidationException.FieldError()
                {
                    Error = e.Key.ErrorMessage,
                    FieldName = e.Key.PropertyName,
                }).ToList()
            };
        }

        return await next();
    }
}