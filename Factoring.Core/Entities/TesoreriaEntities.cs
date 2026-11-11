namespace Factoring.Core.Entities;

/// <summary>
/// Entidad Giro - Migrada de tabla GIR (F_TS1000.frm)
/// </summary>
public class Giro
{
    public int NumeroGiro { get; set; }              // GIR_NUM_GIR
    public long RutCliente { get; set; }             // GIR_RUT_CLI
    public int NumeroOperacion { get; set; }         // GIR_NUM_OPE
    public int TipoGiro { get; set; }                // GIR_TIP_GIR (1=Adelanto, 2=Otorgamiento, 3=Otorgamiento2)
    public DateTime? FechaGiro { get; set; }          // GIR_FEC_GIR
    public decimal MontoGiro { get; set; }           // GIR_MON_GIR
    public string? CuentaCorriente { get; set; }     // GIR_CTA_CTE
    public int? CodigoBanco { get; set; }            // GIR_COD_BCO
    public string? Beneficiario { get; set; }        // GIR_BEN_GIR
    public int Estado { get; set; }                  // GIR_EST_GIR (1=Digitado, 2=Emitido, 3=Anulado)
    public int? FormaCancelacion { get; set; }       // GIR_FOR_CAN
    public string UsuarioCreacion { get; set; } = string.Empty;  // GIR_USR_CRE
    public DateTime? FechaCreacion { get; set; }      // GIR_FEC_CRE
    public string? UsuarioModificacion { get; set; } // GIR_USR_MOD
    public DateTime? FechaModificacion { get; set; } // GIR_FEC_MOD
    
    // Navegación
    public string? NombreCliente { get; set; }
    public string? NombreBanco { get; set; }
    public string? DescripcionTipoGiro { get; set; }
    public string? DescripcionEstado { get; set; }
}

/// <summary>
/// Entidad Movimiento - Migrada de tabla MOV (F_TS1800.frm)
/// </summary>
public class Movimiento
{
    public int NumeroMovimiento { get; set; }        // MOV_NUM_MOV
    public DateTime? FechaMovimiento { get; set; }    // MOV_FEC_MOV
    public int TipoMovimiento { get; set; }          // MOV_TIP_MOV
    public long? RutCliente { get; set; }            // MOV_RUT_CLI
    public int? NumeroOperacion { get; set; }        // MOV_NUM_OPE
    public int? NumeroGiro { get; set; }             // MOV_NUM_GIR
    public decimal Monto { get; set; }               // MOV_MON_MOV
    public string? Glosa { get; set; }               // MOV_GLS_MOV
    public int? CodigoBanco { get; set; }            // MOV_COD_BCO
    public string? CuentaBanco { get; set; }         // MOV_CTA_BCO
    public int Estado { get; set; }                  // MOV_EST_MOV
    public string UsuarioCreacion { get; set; } = string.Empty;  // MOV_USR_CRE
    public DateTime? FechaCreacion { get; set; }      // MOV_FEC_CRE
    public string? UsuarioModificacion { get; set; } // MOV_USR_MOD
    public DateTime? FechaModificacion { get; set; } // MOV_FEC_MOD
    
    // Navegación
    public string? NombreCliente { get; set; }
    public string? NombreBanco { get; set; }
    public string? DescripcionTipoMovimiento { get; set; }
}

/// <summary>
/// Entidad Arqueo - Vista VW_ARQUEO (F_TS2100.frm)
/// Representa un documento (factura, cheque, letra) para el informe de arqueo
/// </summary>
public class ArqueoDocumento
{
    public string TipoDocumento { get; set; } = string.Empty;  // TIP_DOC (FAO, CHO, LEO)
    public int NumeroOperacion { get; set; }                   // NUM_OPE
    public long NumeroDocumento { get; set; }                  // NUM_DOC
    public long RutDeudor { get; set; }                        // RUT_DEU
    public DateTime? FechaVencimiento { get; set; }            // FEC_VTO
    public decimal MontoDocumento { get; set; }                // MTO_DOC
    public int EstadoDocumento { get; set; }                   // EST_DOC
    public long RutCliente { get; set; }                       // RUT_CLI
    public string? NombreCliente { get; set; }                 // NOM_CLI
    public string? NombreDeudor { get; set; }                  // NOM_DEU
    public string? CodigoBanco { get; set; }                   // COD_BCO
    public string? CuentaCorriente { get; set; }               // CTA_COR
    public string? NombreBanco { get; set; }                   // NOM_BCO
    
    // Propiedades calculadas
    public string DescripcionTipoDocumento => TipoDocumento switch
    {
        "FAO" => "Factura",
        "CHO" => "Cheque",
        "LEO" => "Letra",
        _ => TipoDocumento
    };
    
    public string DescripcionEstado => EstadoDocumento switch
    {
        1 => "Vigente",
        2 => "Prorrogado",
        3 => "Renovado",
        4 => "Cancelado",
        5 => "Moroso",
        _ => "Desconocido"
    };
}
