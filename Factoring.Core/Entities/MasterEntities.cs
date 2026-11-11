namespace Factoring.Core.Entities;

/// <summary>
/// Entidad Banco - Migrada de tabla BCO
/// </summary>
public class Banco
{
    public int Codigo { get; set; }          // BCO_COD_BCO
    public string Descripcion { get; set; } = string.Empty;  // BCO_DES_BCO
    public string PermiteDeposito { get; set; } = "N";       // BCO_DEP_BCO
    public string? CuentaCorriente { get; set; }             // BCO_CTA_BCO
    public int? CuentaContable { get; set; }                 // BCO_CTA_CON
    public string TransferenciaElectronica { get; set; } = "N"; // BCO_TRA_ELE
    public int Estado { get; set; } = 0;     // BCO_EST_BCO
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}

/// <summary>
/// Entidad Plaza - Migrada de tabla PZA
/// </summary>
public class Plaza
{
    public int Codigo { get; set; }          // PZA_COD_PZA
    public string Descripcion { get; set; } = string.Empty;  // PZA_DES_PZA
    public int DiasCanje { get; set; } = 0;  // PZA_DIA_PZA
    public string PlazaFuera { get; set; } = "N";  // PZA_FUE_PZA
    public int Estado { get; set; } = 0;     // PZA_EST_PZA
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}

/// <summary>
/// Entidad Sucursal - Migrada de tabla SUC
/// </summary>
public class Sucursal
{
    public int Codigo { get; set; }          // SUC_COD_SUC
    public string Descripcion { get; set; } = string.Empty;  // SUC_DES_SUC
    public string? Direccion { get; set; }   // SUC_DIR_SUC
    public string? Telefono { get; set; }    // SUC_TEL_SUC
    public string? Email { get; set; }       // SUC_EMA_SUC
    public int Estado { get; set; } = 0;     // SUC_EST_SUC
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}

/// <summary>
/// Entidad Ejecutivo - Migrada de tabla EJE
/// </summary>
public class Ejecutivo
{
    public int Codigo { get; set; }          // EJE_COD_EJE
    public string Nombre { get; set; } = string.Empty;  // EJE_NOM_EJE
    public string? Rut { get; set; }         // EJE_RUT_EJE
    public string? Email { get; set; }       // EJE_EMA_EJE
    public string? Telefono { get; set; }    // EJE_TEL_EJE
    public int? CodigoSucursal { get; set; } // EJE_COD_SUC
    public int Estado { get; set; } = 0;     // EJE_EST_EJE
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    
    // Navegación
    public Sucursal? Sucursal { get; set; }
}

/// <summary>
/// Entidad Comuna - Migrada de tabla COM
/// </summary>
public class Comuna
{
    public int Codigo { get; set; }          // COM_COD_COM
    public string Descripcion { get; set; } = string.Empty;  // COM_DES_COM
    public int? CodigoRegion { get; set; }   // COM_COD_REG
    public int? CodigoProvincia { get; set; } // COM_COD_PRO
    public int Estado { get; set; } = 0;     // COM_EST_COM
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    
    // Navegación
    public Region? Region { get; set; }
}

/// <summary>
/// Entidad Region - Migrada de tabla REG
/// </summary>
public class Region
{
    public int Codigo { get; set; }          // REG_COD_REG
    public string Descripcion { get; set; } = string.Empty;  // REG_DES_REG
    public int Orden { get; set; } = 0;      // REG_ORD_REG
    public int Estado { get; set; } = 0;     // REG_EST_REG
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    
    // Navegación
    public ICollection<Comuna>? Comunas { get; set; }
}

/// <summary>
/// Entidad Gasto - Migrada de tabla GAS
/// </summary>
public class Gasto
{
    public int Codigo { get; set; }          // GAS_COD_GAS
    public string Descripcion { get; set; } = string.Empty;  // GAS_DES_GAS
    public int TipoGasto { get; set; } = 0;  // GAS_TIP_GAS
    public int TipoCalculo { get; set; } = 0; // GAS_TIP_CAL
    public decimal Valor { get; set; } = 0;  // GAS_VAL_GAS
    public int? CuentaContable { get; set; } // GAS_CTA_CON
    public int Estado { get; set; } = 0;     // GAS_EST_GAS
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}

/// <summary>
/// Entidad Parametro - Migrada de tabla PAR
/// </summary>
public class Parametro
{
    public int CodigoTabla { get; set; }     // PAR_COD_TAB
    public int CodigoParametro { get; set; } // PAR_COD_PAR
    public string Descripcion { get; set; } = string.Empty;  // PAR_DES_PAR
    public string? CodigoAuxiliar { get; set; } // PAR_COD_AUX
    public decimal? ValorNumerico { get; set; } // PAR_VAL_NUM
    public string? ValorTexto { get; set; }  // PAR_VAL_TXT
    public int Estado { get; set; } = 0;     // PAR_EST_PAR
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
}
