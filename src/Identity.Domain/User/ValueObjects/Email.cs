using Identity.Domain.User.Exceptions;
using System.Text.RegularExpressions;

namespace Identity.Domain.User.ValueObjects;
public record Email
{
    public string Value { get; set; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidEmailException("E-poçt ünvanı boş ola bilməz.");

        if (!Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new InvalidEmailException("E-poçt formatı yalnışdır.");

        Value = value;
    }
}
