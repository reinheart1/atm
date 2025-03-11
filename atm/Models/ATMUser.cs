using System.ComponentModel.DataAnnotations;

public class ATMUser
{
    [Key]
    public int Id { get; set; }

    [Required, StringLength(16)]
    public string CardNumber { get; set; }

    [Required, StringLength(4)]
    public string PIN { get; set; }

    public decimal Balance { get; set; }

    [Required, StringLength(100)]
    public string FullName { get; set; }
}
