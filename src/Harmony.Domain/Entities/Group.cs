using Harmony.Domain.ValueObjects;

namespace Harmony.Domain.Entities;

public sealed class Group
{
    private readonly List<PersonId> _memberIds = new();

    public GroupId Id { get; private set; }
    public string Name { get; private set; }
    public PersonId? CoordinatorId { get; private set; }
    public IReadOnlyList<PersonId> MemberIds => _memberIds.AsReadOnly();

    public Group(GroupId id, string name)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Group name is required", nameof(name));
        
        Name = name.Trim();
    }

    // Required for EF Core
    private Group()
    {
        Id = null!;
        Name = null!;
    }

    public static Group Create(string name)
    {
        return new Group(GroupId.New(), name);
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Group name is required", nameof(name));
        
        Name = name.Trim();
    }

    public void SetCoordinator(PersonId? coordinatorId)
    {
        CoordinatorId = coordinatorId;
    }

    public void AddMember(PersonId personId)
    {
        if (personId == null)
            throw new ArgumentNullException(nameof(personId));

        if (!_memberIds.Contains(personId))
        {
            _memberIds.Add(personId);
        }
    }

    public void RemoveMember(PersonId personId)
    {
        if (personId == null)
            throw new ArgumentNullException(nameof(personId));

        _memberIds.Remove(personId);
        
        // If the coordinator is being removed, clear the coordinator
        if (CoordinatorId?.Equals(personId) == true)
        {
            CoordinatorId = null;
        }
    }

    public bool HasMember(PersonId personId)
    {
        return _memberIds.Contains(personId);
    }

    public int MemberCount => _memberIds.Count;
}
