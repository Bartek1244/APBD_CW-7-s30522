using System.ComponentModel.DataAnnotations;

namespace apbd_cw7_s30522.Models.DTOs;

public class ClientCreateDTO
{
    [Length(1, 120)]
    public required string FirstName { get; set; }
    [Length(1, 120)]
    public required string LastName { get; set; }
    [RegularExpression("^[^@]+@[^@]+\\.[^@]+$")]
    [Length(1, 120)]
    public required string Email { get; set; }
    [Length(1, 120)]
    public required string Telephone { get; set; }
    [Length(1, 120)]
    public required string Pesel { get; set; }
}