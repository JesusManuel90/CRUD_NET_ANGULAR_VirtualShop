using System;
using System.Collections.Generic;

namespace Api.Data;

public partial class Venta
{
    public int Id { get; set; }

    public DateTime Fecha { get; set; }

    public decimal Total { get; set; }

    public int UsuarioId { get; set; }

    public virtual ICollection<DetalleVenta> DetalleVenta { get; set; } = new List<DetalleVenta>();

    public virtual Usuario Usuario { get; set; } = null!;
}
