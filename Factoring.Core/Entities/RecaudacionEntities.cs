namespace Factoring.Core.Entities;

/// <summary>
/// Entidad Gestión de Cobranza - Migrada de tabla GES (F_GC1100.frm)
/// Registra las gestiones telefónicas realizadas por cobradores
/// </summary>
public class GestionCobranza
{
    public int Correlativo { get; set; }             // GES_COR_GES
    public long RutCliente { get; set; }             // GES_RUT_CLI
    public long RutDeudor { get; set; }              // GES_RUT_DEU
    public int TipoDocumento { get; set; }           // GES_TIP_DOC (1=Factura, 2=Cheque, 3=Letra, etc.)
    public long NumeroDocumento { get; set; }        // GES_NUM_FAC
    public int NumeroOperacion { get; set; }         // GES_NUM_OPE
    public string CodigoCobrador { get; set; } = string.Empty;  // GES_COB_TEL
    public DateTime? FechaGestion { get; set; }       // GES_FEC_GES
    public string HoraGestion { get; set; } = string.Empty;     // GES_HOR_GES
    public int EstadoCobranza { get; set; }          // GES_EST_COB
    public DateTime? FechaPago { get; set; }         // GES_FEC_PGO
    public string? HoraPago { get; set; }            // GES_HOR_PGO
    public string? DocumentoNecesario { get; set; }  // GES_DOC_NEC
    public string? Observacion { get; set; }         // GES_OBS_GES
    public DateTime? FechaProximaGestion { get; set; }  // GES_FEC_PRX
    public string? HoraProximaGestion { get; set; }     // GES_HOR_PRX
    public string? CodigoRecaudador { get; set; }    // GES_REC_GES
    
    // Propiedades de navegación
    public string? NombreCliente { get; set; }
    public string? NombreDeudor { get; set; }
    public string? NombreCobrador { get; set; }
    public string? DescripcionEstadoCobranza { get; set; }
    public string? DescripcionTipoDocumento { get; set; }
}

/// <summary>
/// Entidad Cobrador de Recaudación - Migrada de tabla COB (F_GC1000.frm)
/// Versión extendida para gestión de cobranza
/// </summary>
public class CobradorRecaudacion
{
    public string CodigoCobrador { get; set; } = string.Empty;  // COB_COD_COB
    public string NombreCobrador { get; set; } = string.Empty;  // COB_NOM_COB
    public string? Telefono { get; set; }                       // COB_TEL_COB
    public string? Email { get; set; }                          // COB_EMA_COB
    public int Estado { get; set; }                             // COB_EST_COB
    public string? Sector { get; set; }                         // COB_SEC_COB
    
    // Propiedades calculadas
    public int TotalClientesAsignados { get; set; }
    public int TotalGestionesPendientes { get; set; }
}

/// <summary>
/// Documento para gestión de cobranza - Vista combinada de FAO, CHO, LEO, OTO
/// </summary>
public class DocumentoGestion
{
    public long RutCliente { get; set; }
    public long RutDeudor { get; set; }
    public int NumeroOperacion { get; set; }
    public int TipoDocumento { get; set; }
    public long NumeroDocumento { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public decimal MontoDocumento { get; set; }
    public int EstadoDocumento { get; set; }
    public int? EstadoVerificacion { get; set; }
    public DateTime? FechaNotificacion { get; set; }
    public int? EstadoCobranza { get; set; }
    
    // Propiedades de navegación
    public string? DescripcionTipoDocumento { get; set; }
    public string? DescripcionEstadoDocumento { get; set; }
    public string? DescripcionEstadoCobranza { get; set; }
    public bool TieneProblema { get; set; }  // Si GES_EST_COB = 46 (COD_GESPROBLEMA)
}

/// <summary>
/// Deudor para gestión - Vista de DEU con datos de contacto
/// </summary>
public class DeudorGestion
{
    public long RutDeudor { get; set; }
    public string NombreDeudor { get; set; } = string.Empty;
    public string? ContactoCobranza { get; set; }     // DEU_CTO_COB
    public string? DireccionPago { get; set; }        // DEU_DIR_PGO
    public string? Telefono { get; set; }
    public string? Fax { get; set; }
    public int TotalDocumentosPendientes { get; set; }
}

/// <summary>
/// Cliente asignado a cobrador
/// </summary>
public class ClienteCobrador
{
    public long RutCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public string CodigoCobrador { get; set; } = string.Empty;
    public string? NombreCobrador { get; set; }
    public string? NombreEjecutivo { get; set; }
    public int TotalDeudores { get; set; }
    public int TotalDocumentosPendientes { get; set; }
    public decimal MontoTotalPendiente { get; set; }
}

/// <summary>
/// Estados de Cobranza - Vista combinada para reportes (F_GC2600.frm)
/// </summary>
public class EstadoCobranzaDocumento
{
    public long RutCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public long RutDeudor { get; set; }
    public string NombreDeudor { get; set; } = string.Empty;
    public int TipoDocumento { get; set; }
    public string DescripcionTipoDocumento { get; set; } = string.Empty;
    public long NumeroDocumento { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public int EstadoCobranza { get; set; }
    public string DescripcionEstadoCobranza { get; set; } = string.Empty;
    public DateTime? FechaPago { get; set; }
    public decimal MontoDocumento { get; set; }
    public string? NombreBanco { get; set; }
    public string? NombreEjecutivo { get; set; }
    public string? NombreCobrador { get; set; }
    public string? DescripcionEstadoDocumento { get; set; }
    public DateTime? FechaObservacion { get; set; }
    public string? Observacion { get; set; }
}

/// <summary>
/// Deuda Total Cliente - (F_GC2200.frm)
/// </summary>
public class DeudaTotalCliente
{
    public long RutCliente { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public DateTime? FechaCalculo { get; set; }
    public decimal TotalFacturas { get; set; }
    public decimal TotalCheques { get; set; }
    public decimal TotalLetras { get; set; }
    public decimal TotalOtros { get; set; }
    public decimal CuentasPorPagar { get; set; }      // _CXP_
    public decimal FacturasNoFactoring { get; set; }  // _FNC_
    public decimal CuentasPorCobrar { get; set; }     // _CXC_
    public decimal Excedentes { get; set; }           // _EXC_
    public decimal DeudaNeta { get; set; }            // _TOT_
}

/// <summary>
/// Detalle de deuda por tipo de documento
/// </summary>
public class DetalleDeudaDocumento
{
    public long RutDeudor { get; set; }
    public string NombreDeudor { get; set; } = string.Empty;
    public int NumeroOperacion { get; set; }
    public string TipoDocumentoCodigo { get; set; } = string.Empty;
    public long NumeroDocumento { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public decimal MontoDocumento { get; set; }
    public decimal MontoAnticipo { get; set; }
    public decimal InteresVencido { get; set; }
    public decimal InteresMora { get; set; }
    public decimal InteresNoDevengado { get; set; }
    public string EstadoDocumento { get; set; } = string.Empty;
    public decimal Abono { get; set; }
    public decimal DeudaNeta { get; set; }
}

