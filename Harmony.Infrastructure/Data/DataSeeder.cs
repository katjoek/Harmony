using Harmony.ApplicationCore.Interfaces;
using Harmony.Domain.Entities;
using Harmony.Domain.ValueObjects;

namespace Harmony.Infrastructure.Data;

public sealed class DataSeeder
{
    private readonly IPersonRepository _personRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IMembershipService _membershipService;
    private readonly Random _random = new();

    public DataSeeder(
        IPersonRepository personRepository,
        IGroupRepository groupRepository,
        IMembershipService membershipService)
    {
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _groupRepository = groupRepository ?? throw new ArgumentNullException(nameof(groupRepository));
        _membershipService = membershipService ?? throw new ArgumentNullException(nameof(membershipService));
    }

    public async Task SeedAsync()
    {
        // Check if data already exists
        var existingPersons = await _personRepository.GetAllAsync();
        if (existingPersons.Count > 0)
        {
            Console.WriteLine("Data already seeded, skipping...");
            return;
        }

        Console.WriteLine("Starting data seeding...");

        // Create persons
        var persons = await CreatePersonsAsync();
        Console.WriteLine($"Created {persons.Count} persons");

        // Create groups
        var groups = await CreateGroupsAsync(persons);
        Console.WriteLine($"Created {groups.Count} groups");

        // Add members to groups
        await AddMembersToGroupsAsync(groups, persons);
        Console.WriteLine("Added members to groups");

        Console.WriteLine("Data seeding completed!");
    }

    private async Task<List<Person>> CreatePersonsAsync()
    {
        var persons = new List<Person>();
        var firstNames = GetDutchFirstNames();
        var prefixes = GetDutchPrefixes();
        var surnames = GetDutchSurnames();
        var cities = GetDutchCities();
        var streetNames = GetDutchStreetNames();

        for (int i = 0; i < 150; i++)
        {
            var firstName = GetRandomItem(firstNames);
            var prefix = _random.NextDouble() < 0.3 ? GetRandomItem(prefixes) : null;
            var surname = _random.NextDouble() < 0.8 ? GetRandomItem(surnames) : null;
            
            var personName = new PersonName(firstName, prefix, surname);
            var person = Person.Create(personName);

            // Add random date of birth (18-85 years old)
            var age = _random.Next(18, 86);
            var dateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-age).AddDays(_random.Next(-365, 365)));
            person.UpdateDateOfBirth(dateOfBirth);

            // Add address (70% chance)
            if (_random.NextDouble() < 0.7)
            {
                var street = $"{GetRandomItem(streetNames)} {_random.Next(1, 200)}";
                var zipCode = $"{_random.Next(1000, 10000)} {GetRandomZipCodeLetters()}";
                var city = GetRandomItem(cities);
                person.UpdateAddress(new Address(street, zipCode, city));
            }

            // Add phone number (80% chance)
            if (_random.NextDouble() < 0.8)
            {
                var phoneNumber = GeneratePhoneNumber();
                person.UpdatePhoneNumber(new PhoneNumber(phoneNumber));
            }

            // Add email address (60% chance)
            if (_random.NextDouble() < 0.6)
            {
                var email = GenerateEmail(firstName, surname);
                person.UpdateEmailAddress(new EmailAddress(email));
            }

            await _personRepository.AddAsync(person);
            persons.Add(person);
        }

        return persons;
    }

    private async Task<List<Group>> CreateGroupsAsync(List<Person> persons)
    {
        var groups = new List<Group>();
        var groupNames = GetDutchGroupNames();

        foreach (var groupName in groupNames.Take(35))
        {
            var group = Group.Create(groupName);

            // Assign random coordinator (100% chance - all groups must have coordinator)
            if (persons.Count > 0)
            {
                var coordinator = GetRandomItem(persons.ToArray());
                group.SetCoordinator(coordinator.Id);
            }

            await _groupRepository.AddAsync(group);
            groups.Add(group);
        }

        return groups;
    }

    private async Task AddMembersToGroupsAsync(List<Group> groups, List<Person> persons)
    {
        foreach (var group in groups)
        {
            // Determine number of members (typically around 10, up to 50)
            var memberCount = _random.NextDouble() < 0.8 
                ? _random.Next(8, 15)   // 80% chance: 8-14 members
                : _random.Next(15, 51); // 20% chance: 15-50 members

            // Randomly select members
            var selectedPersons = persons
                .OrderBy(x => _random.Next())
                .Take(memberCount)
                .ToList();

            foreach (var person in selectedPersons)
            {
                try
                {
                    await _membershipService.AddPersonToGroupAsync(person.Id, group.Id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to add person {person.Id} to group {group.Id}: {ex.Message}");
                }
            }
        }
    }

    private string GeneratePhoneNumber()
    {
        var prefixes = new[] { "06", "020", "030", "040", "050", "070", "010", "013", "015" };
        var prefix = GetRandomItem(prefixes);
        
        if (prefix == "06")
        {
            return $"{prefix}-{_random.Next(10000000, 99999999)}";
        }
        else
        {
            return $"{prefix}-{_random.Next(1000000, 9999999)}";
        }
    }

    private string GenerateEmail(string firstName, string? surname)
    {
        var domains = new[] { "gmail.com", "hotmail.com", "outlook.com", "yahoo.com", "ziggo.nl", "kpnmail.nl", "xs4all.nl" };
        var domain = GetRandomItem(domains);
        
        // Clean the first name to remove any invalid characters
        var cleanFirstName = System.Text.RegularExpressions.Regex.Replace(firstName.ToLower(), "[^a-z0-9]", "");
        if (string.IsNullOrEmpty(cleanFirstName))
        {
            cleanFirstName = "user";
        }
        
        var localPart = cleanFirstName;
        if (!string.IsNullOrEmpty(surname))
        {
            var cleanSurname = System.Text.RegularExpressions.Regex.Replace(surname.ToLower(), "[^a-z0-9]", "");
            if (!string.IsNullOrEmpty(cleanSurname))
            {
                localPart += _random.NextDouble() < 0.7 ? $".{cleanSurname}" : $"{_random.Next(10, 99)}";
            }
            else
            {
                localPart += _random.Next(10, 99);
            }
        }
        else
        {
            localPart += _random.Next(10, 99);
        }

        return $"{localPart}@{domain}";
    }

    private string GetRandomZipCodeLetters()
    {
        var letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        return $"{letters[_random.Next(letters.Length)]}{letters[_random.Next(letters.Length)]}";
    }

    private T GetRandomItem<T>(T[] items)
    {
        return items[_random.Next(items.Length)];
    }

    private static string[] GetDutchFirstNames() => new[]
    {
        "Jan", "Piet", "Klaas", "Henk", "Johan", "Willem", "Gerrit", "Cornelis", "Johannes", "Adrianus",
        "Maria", "Anna", "Johanna", "Cornelia", "Elisabeth", "Catharina", "Margaretha", "Hendrika", "Adriana", "Jacoba",
        "Emma", "Sophie", "Julia", "Olivia", "Isa", "Tess", "Lisa", "Lotte", "Eva", "Sara",
        "Daan", "Sem", "Lucas", "Milan", "Stijn", "Bram", "Finn", "Lars", "Thijs", "Jesse",
        "Anouk", "Fleur", "Femke", "Lynn", "Noa", "Zoë", "Roos", "Iris", "Amy", "Lieke",
        "Sven", "Tim", "Tom", "Max", "Nick", "Niels", "Ruben", "Rick", "Joep", "Sander"
    };

    private static string[] GetDutchPrefixes() => new[]
    {
        "van", "de", "der", "den", "van de", "van der", "van den", "ten", "ter", "te",
        "op", "op de", "op den", "op 't", "in", "in de", "in den", "in 't", "over", "onder"
    };

    private static string[] GetDutchSurnames() => new[]
    {
        "Jansen", "Hansen", "De Vries", "Van den Berg", "Bakker", "Visser", "Smit", "Meijer", "De Boer", "Mulder",
        "De Groot", "Bos", "Vos", "Peters", "Hendriks", "Van Dijk", "Van Leeuwen", "Dekker", "Brouwer", "De Wit",
        "Dijkstra", "Smits", "De Graaf", "Van der Meer", "Van der Laan", "Koning", "Hermans", "Van den Heuvel", "Van der Heijden", "Morren",
        "Jacobs", "De Haan", "Vermeulen", "Van Beek", "Hoekstra", "Van Es", "Willems", "Scholten", "Van Vliet", "Post",
        "Kuiper", "Timmermans", "Groen", "Gerritsen", "Klein", "Mol", "Kuijpers", "Van de Ven", "Van den Broek", "De Ruiter"
    };

    private static string[] GetDutchCities() => new[]
    {
        "Amsterdam", "Rotterdam", "Den Haag", "Utrecht", "Eindhoven", "Tilburg", "Groningen", "Almere", "Breda", "Nijmegen",
        "Enschede", "Haarlem", "Arnhem", "Amersfoort", "Zaanstad", "Apeldoorn", "s-Hertogenbosch", "Hoofddorp", "Maastricht", "Leiden",
        "Dordrecht", "Zoetermeer", "Zwolle", "Deventer", "Delft", "Alkmaar", "Leeuwarden", "Roosendaal", "Helmond", "Venlo"
    };

    private static string[] GetDutchStreetNames() => new[]
    {
        "Hoofdstraat", "Kerkstraat", "Schoolstraat", "Dorpsstraat", "Molenstraat", "Nieuwstraat", "Stationsstraat", "Beatrixstraat", "Wilhelminastraat", "Julianastraat",
        "Koningstraat", "Prinses Beatrixlaan", "Burgemeester van der Veldenstraat", "Mgr. Nolensstraat", "Prof. dr. Dorgelolaan", "Karel de Grotelaan", "Van Goghstraat", "Rembrandtlaan",
        "Mozartstraat", "Beethovenstraat", "Bachstraat", "Chopinstraat", "Brahmsstraat", "Vondellaan", "Bilderdijkstraat", "Multatulistraat", "Potgieterstraat", "Da Costalaan",
        "Rozenlaan", "Tulpstraat", "Jasmijnstraat", "Violenstraat", "Seringenstraat", "Kastanjelaan", "Eikenlaan", "Berkenlaan", "Populierenlaan", "Lindenlaan"
    };

    private static string[] GetDutchGroupNames() => new[]
    {
        "Bijbelkring Zondag", "Jongerengroep", "Ouderenvereniging", "Kinderkoor", "Gospelkoor", "Mannenkoor", "Dameskoor", "Handwerkgroep", "Wandelgroep", "Fietsteam",
        "Bijbelstudiegroep", "Gebedsgroep", "Evangelisatieteam", "Jeugdwerk", "Kinderdienst", "Crèche", "Alpha cursus", "Huwelijkscursus", "Echtparengroep", "Singles",
        "Huisgroep Noord", "Huisgroep Zuid", "Huisgroep Oost", "Huisgroep West", "Huisgroep Centrum", "Zendingscommissie", "Diaconiegroep", "Kerkenraad", "Muziekgroep", "Praise team",
        "Koffie team", "Welkomstteam", "Techniek team", "Schoonmaakploeg", "Tuingroep", "Bouwteam", "Vrijwilligerscoördinatie", "Pastorale zorg", "Bezoekteam", "Intercedenten"
    };
}
