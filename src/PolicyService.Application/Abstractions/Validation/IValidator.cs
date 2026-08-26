namespace PolicyService.Application.Abstractions.Validation;

public interface IValidator<in T>
{
    void Validate(T value);
}