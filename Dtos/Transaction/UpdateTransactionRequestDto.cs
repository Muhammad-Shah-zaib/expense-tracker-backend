using System.ComponentModel.DataAnnotations;

namespace expense_tracker.Dtos.Transaction;

public class UpdateTransactionRequestDto
{
    [Required] public string Type { get; set; } = string.Empty;

    [Required] public string Description { get; set; } = string.Empty;

    [Required] public string CardNumber { get; set; } = string.Empty;

    [Required] public string Purpose { get; set; } = string.Empty;

    [Required] public double Amount { get; set; }

    [Required] public bool Marked { get; set; }
}