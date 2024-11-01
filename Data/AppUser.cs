using System;
using System.Collections.Generic;

namespace expense_tracker.Data;

public partial class AppUser
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public string HashPassword { get; set; } = null!;

    public string HashKey { get; set; } = null!;

    public virtual ICollection<AuthLog> AuthLogs { get; set; } = new List<AuthLog>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
