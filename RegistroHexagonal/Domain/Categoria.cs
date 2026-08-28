using System;

namespace RegistroHexagonal.Domain
{
    public class Categoria
    {
        public Guid Id { get; private set; }
        public string Nombre { get; private set; }
        public bool Activo { get; private set; } = true;
        public Categoria(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre de la categoría no puede estar vacío.", nameof(nombre));

            Id = Guid.NewGuid();
            Nombre = nombre;
            Activo = true;
        }

        public Categoria(Guid id, string nombre, bool activo)
        {
            Id = id;
            Nombre = nombre;
            Activo = activo;
        }

        public void Actualizar(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre de la categoría no puede estar vacío.", nameof(nombre));

            Nombre = nombre;
        }
        public void Desactivar()
        {
            Activo = false;
        }
        public void Reactivar()
        {
            Activo = true;
        }
    }
}
