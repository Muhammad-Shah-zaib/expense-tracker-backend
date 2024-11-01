using System;
using System.Collections.Generic;

namespace expense_tracker.Data;

public partial class AuthLog
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Type { get; set; } = null!;

    public DateTime Date { get; set; }

    public virtual AppUser User { get; set; } = null!;
}
