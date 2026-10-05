using SaboresDelValle.Application.Interfaces;

namespace SaboresDelValle.Application.Tests.Fakes;

/// <summary>
/// Simula una tabla: guarda y devuelve copias, de modo que solo
/// los cambios enviados con UpdateAsync quedan persistidos.
/// </summary>
public abstract class RepositorioEnMemoria<T> : IRepository<T> where T : class
{
    private readonly Dictionary<int, T> _filas = new();
    private int _siguienteId = 1;

    public int Cantidad => _filas.Count;

    public T Agregar(T entidad)
    {
        var id = _siguienteId++;
        var guardada = Copiar(entidad, id);
        _filas[id] = guardada;
        return Copiar(guardada, id);
    }

    public Task<int> InsertAsync(T entity)
    {
        return Task.FromResult(IdDe(Agregar(entity)));
    }

    public Task<int> UpdateAsync(T entity)
    {
        var id = IdDe(entity);

        if (!_filas.ContainsKey(id))
        {
            return Task.FromResult(0);
        }

        _filas[id] = Copiar(entity, id);
        return Task.FromResult(1);
    }

    public Task<int> DeleteAsync(int id)
    {
        return Task.FromResult(_filas.Remove(id) ? 1 : 0);
    }

    public Task<T?> SelectByIdAsync(int id)
    {
        return Task.FromResult(_filas.TryGetValue(id, out var fila) ? Copiar(fila, id) : null);
    }

    public Task<List<T>> SelectAllAsync()
    {
        return Task.FromResult(_filas.Select(f => Copiar(f.Value, f.Key)).ToList());
    }

    protected IEnumerable<T> Filas => _filas.Values;

    protected abstract int IdDe(T entidad);

    protected abstract T Copiar(T entidad, int id);
}
