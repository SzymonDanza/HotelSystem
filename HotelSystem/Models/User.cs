public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty; // Domyślna wartość
    public string Password { get; set; } = string.Empty; // Domyślna wartość
    public bool IsAdmin { get; set; }
}

