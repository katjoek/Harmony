using Harmony.Domain.ValueObjects;

namespace Harmony.Domain.Entities;

public sealed class Person
{
    private readonly List<GroupId> _groupIds = new();

    public PersonId Id { get; private set; }
    public PersonName Name { get; private set; }
    public DateOnly? DateOfBirth { get; private set; }
    public Address? Address { get; private set; }
    public PhoneNumber? PhoneNumber { get; private set; }
    public EmailAddress? EmailAddress { get; private set; }
    public IReadOnlyList<GroupId> GroupIds => _groupIds.AsReadOnly();

    public Person(PersonId id, PersonName name)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    // Required for EF Core
    private Person()
    {
        Id = null!;
        Name = null!;
    }

    public static Person Create(PersonName name)
    {
        return new Person(PersonId.New(), name);
    }

    public void UpdateName(PersonName name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public void UpdateDateOfBirth(DateOnly? dateOfBirth)
    {
        DateOfBirth = dateOfBirth;
    }

    public void UpdateAddress(Address? address)
    {
        Address = address;
    }

    public void UpdatePhoneNumber(PhoneNumber? phoneNumber)
    {
        PhoneNumber = phoneNumber;
    }

    public void UpdateEmailAddress(EmailAddress? emailAddress)
    {
        EmailAddress = emailAddress;
    }

    public void AddToGroup(GroupId groupId)
    {
        if (groupId == null)
            throw new ArgumentNullException(nameof(groupId));

        if (!_groupIds.Contains(groupId))
        {
            _groupIds.Add(groupId);
        }
    }

    public void RemoveFromGroup(GroupId groupId)
    {
        if (groupId == null)
            throw new ArgumentNullException(nameof(groupId));

        _groupIds.Remove(groupId);
    }

    public bool IsMemberOf(GroupId groupId)
    {
        return _groupIds.Contains(groupId);
    }
}
