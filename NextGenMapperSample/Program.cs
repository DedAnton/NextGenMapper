using NextGenMapper;
using System.Text.Json;

var src = new Employee()
{
    FirstName = "David",
    LastName = "Fowler",
    Created = new List<Creation>
    {
        new Creation("Minimal Api"),
        new Creation("SignalR")
    }
};

var dst = src.MapWith<EmployeeDTO>(FullName: $"{src.FirstName} {src.LastName}");

Console.WriteLine(JsonSerializer.Serialize(dst, new JsonSerializerOptions { WriteIndented = true }));

public record Employee
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public List<Creation> Created { get; set; } = new List<Creation>();
}

public record EmployeeDTO
{
    public string? FullName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public Creation[] Created { get; set; } = Array.Empty<Creation>();
}

public record Creation(string Name);