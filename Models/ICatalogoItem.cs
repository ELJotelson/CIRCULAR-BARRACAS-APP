namespace Circulacion_Barracas.Models;

public interface ICatalogoItem
{
    Guid Id { get; set; }
    Guid OperacionId { get; set; }
    string Nombre { get; set; }
    bool Activo { get; set; }
}
