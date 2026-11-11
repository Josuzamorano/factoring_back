using Factoring.Core.Entities;

namespace Factoring.Core.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByLoginAsync(string login);
    Task<bool> ValidarCredencialesAsync(string login, string password);
    Task<int> IncrementarIntentosErroneos(string login);
    Task BloquearUsuarioAsync(string login);
    Task ResetearIntentosAsync(string login);
    Task CambiarPasswordAsync(string login, string nuevoPassword);
    Task<IEnumerable<MenuOpcion>> GetMenuAutorizadoAsync(int codigoPerfil);
}
