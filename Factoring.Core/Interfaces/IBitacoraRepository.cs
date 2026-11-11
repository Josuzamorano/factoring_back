using Factoring.Core.Entities;

namespace Factoring.Core.Interfaces;

public interface IBitacoraRepository
{
    Task RegistrarAccesoAsync(BitacoraAcceso bitacora);
    Task RegistrarSalidaAsync(string usuario, string maquina);
}
