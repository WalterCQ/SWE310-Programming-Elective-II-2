using System.ComponentModel.DataAnnotations;

namespace ReceiptManagement.Api.Services;

internal static class RequestValidation
{
    public static Dictionary<string, string[]> ValidateObject(object? request, string? prefix = null)
    {
        var errors = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        if (request is null)
        {
            AddError(errors, prefix ?? "request", "Request body is required.");
            return ToArrays(errors);
        }

        var validationResults = new List<ValidationResult>();
        Validator.TryValidateObject(request, new ValidationContext(request), validationResults, validateAllProperties: true);

        foreach (var validationResult in validationResults)
        {
            var message = validationResult.ErrorMessage ?? "The value is invalid.";
            var memberNames = validationResult.MemberNames.Any()
                ? validationResult.MemberNames
                : [prefix ?? "request"];

            foreach (var memberName in memberNames)
            {
                AddError(errors, BuildFieldName(memberName, prefix), message);
            }
        }

        return ToArrays(errors);
    }

    private static string BuildFieldName(string memberName, string? prefix)
    {
        if (memberName.Contains('.', StringComparison.Ordinal))
        {
            return memberName;
        }

        var fieldName = string.IsNullOrWhiteSpace(memberName)
            ? "request"
            : char.ToLowerInvariant(memberName[0]) + memberName[1..];

        return prefix is null ? fieldName : $"{prefix}.{fieldName}";
    }

    private static void AddError(Dictionary<string, List<string>> errors, string fieldName, string message)
    {
        if (!errors.TryGetValue(fieldName, out var messages))
        {
            messages = [];
            errors[fieldName] = messages;
        }

        messages.Add(message);
    }

    private static Dictionary<string, string[]> ToArrays(Dictionary<string, List<string>> errors)
    {
        return errors.ToDictionary(entry => entry.Key, entry => entry.Value.ToArray(), StringComparer.OrdinalIgnoreCase);
    }
}
