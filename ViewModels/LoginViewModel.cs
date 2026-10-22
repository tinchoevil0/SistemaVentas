using System.ComponentModel.DataAnnotations;

namespace SistemaVentas.ViewModels;

public class LoginViewModel
{
    [Required]
    public string Usuario { get; set; } = "admin";

    [Required]
    [DataType(DataType.Password)]
    public string Contrasena { get; set; } = "admin123!";

    public bool Recordarme { get; set; }
    public string? ReturnUrl { get; set; }
}
