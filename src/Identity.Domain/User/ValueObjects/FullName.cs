using Identity.Domain.User.Exceptions;

namespace Identity.Domain.User.ValueObjects;
public record FullName
{
    public string FirstName { get; }
    public string LastName { get; }

    public FullName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new InvalidFullnameException("Ad boş ola bilməz.");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new InvalidFullnameException("Soyad boş ola bilməz.");

        if (firstName.Length < 2 || firstName.Length > 50)
            throw new InvalidFullnameException("Ad minimum 2, maksimum 50 simvol olmalıdır.");
        if (lastName.Length < 2 || lastName.Length > 50)
            throw new InvalidFullnameException("Soyad minimum 2, maksimum 50 simvol olmalıdır.");

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
    }

    public override string ToString() => $"{FirstName} {LastName}";
}
