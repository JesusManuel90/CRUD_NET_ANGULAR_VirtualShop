using System;
using System.Collections.Generic;

namespace Api.Data;

public partial class Usuario
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Address { get; set; }

    public string? Nationality { get; set; }

    public string PasswordHash { get; set; } = null!;

    public int RolId { get; set; }

    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
