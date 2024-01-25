namespace Collections.Models;

public class ViewUser
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string LastLoginDate { get; set; } = string.Empty;
    public string RegistrationDate { get; set; } = string.Empty;
    public bool IsChecked { get; set; } = false;
}