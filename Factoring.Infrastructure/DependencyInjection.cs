using Factoring.Application.Interfaces;
using Factoring.Application.Services;
using Factoring.Core.Interfaces;
using Factoring.Infrastructure.Data;
using Factoring.Infrastructure.Repositories;
using Factoring.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Factoring.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Conexión a base de datos
        var connectionString = configuration.GetConnectionString("FactoringDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'FactoringDb' is required. Set it with the ConnectionStrings__FactoringDb environment variable or a secret provider.");
        }
        
        services.AddSingleton<IDbConnectionFactory>(_ => new SqlConnectionFactory(connectionString));
        services.AddSingleton(sp => new SqlConnectionFactory(connectionString));

        // Repositorios de Seguridad
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IBitacoraRepository, BitacoraRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IAutorizacionRepository, AutorizacionRepository>();

        // Repositorios de Maestros (Migrados de F_MA*.frm)
        services.AddScoped<IBancoRepository, BancoRepository>();
        services.AddScoped<IPlazaRepository, PlazaRepository>();
        services.AddScoped<ISucursalRepository, SucursalRepository>();
        services.AddScoped<IParametroRepository, ParametroRepository>();

        // Repositorios de Clientes (Migrados de F_CC*.frm)
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IClienteTelefonoRepository, ClienteTelefonoRepository>();
        services.AddScoped<IClienteContactoRepository, ClienteContactoRepository>();
        services.AddScoped<IClienteBancoRepository, ClienteBancoRepository>();

        // Repositorios de Deudores (Migrados de F_CD*.frm)
        services.AddScoped<IDeudorRepository, DeudorRepository>();
        services.AddScoped<IDeudorTelefonoRepository, DeudorTelefonoRepository>();
        services.AddScoped<IDeudorDocumentoRepository, DeudorDocumentoRepository>();

        // Repositorios de Operaciones (Migrados de F_OP*.frm)
        services.AddScoped<IOperacionRepository, OperacionRepository>();
        services.AddScoped<IFacturaOtorgadaRepository, FacturaOtorgadaRepository>();
        services.AddScoped<IChequeOtorgadoRepository, ChequeOtorgadoRepository>();
        services.AddScoped<ILetraOtorgadaRepository, LetraOtorgadaRepository>();
        services.AddScoped<IOtroDocumentoOtorgadoRepository, OtroDocumentoOtorgadoRepository>();
        services.AddScoped<IDocumentoOperacionRepository, DocumentoOperacionRepository>();
        services.AddScoped<ITipoGastoRepository, TipoGastoRepository>();
        services.AddScoped<IGastoOperacionRepository, GastoOperacionRepository>();

        // Repositorios de Cobranza (Migrados de F_CO*.frm, F_GC*.frm)
        services.AddScoped<IVerificacionDiariaRepository, VerificacionDiariaRepository>();
        services.AddScoped<IDocumentoNoCedidoRepository, DocumentoNoCedidoRepository>();
        services.AddScoped<IConsultaMasivaRepository, ConsultaMasivaRepository>();

        // Servicios de Negocio
        services.AddScoped<AuthService>();
        services.AddScoped<AuthAdvancedService>();
        services.AddScoped<OperacionService>();
        services.AddScoped<CobranzaService>();
        services.AddScoped<AlertaService>();
        services.AddScoped<ExportacionService>();
        services.AddScoped<PdfService>();
        services.AddScoped<AuditService>();
        services.AddScoped<AutorizacionService>();

        // Servicios de Seguridad
        services.AddSingleton<EncryptionService>();

        // Servicios de Integraciones Externas
        services.AddHttpClient<SiiService>();
        services.AddScoped<BancoIntegracionService>();
        services.AddScoped<NotificacionService>();

        return services;
    }
}
