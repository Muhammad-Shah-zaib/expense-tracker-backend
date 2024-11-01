using System;
using System.Collections.Generic;

namespace expense_tracker.Data;

public partial class Transaction
{
    public int Id { get; set; }

    public string Type { get; set; } = null!;

    public DateTime Date { get; set; }

    public string? Description { get; set; }

    public string CardNumber { get; set; } = null!;

    public string? Purpose { get; set; }

    public int UserId { get; set; }

    public bool Marked { get; set; }

    public double Amount { get; set; }

    public virtual AppUser User { get; set; } = null!;
}
