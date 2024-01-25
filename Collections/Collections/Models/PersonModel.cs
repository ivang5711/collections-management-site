namespace Collections.Models;

public class PersonModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime RegistrationDate { get; set; }
    public DateTime LastLoginDate { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool Status { get; set; }
    public bool IsAdmin { get; set; }
}