namespace RegistroHexagonal.Domain
{
    public class Cliente
    {
        public Guid Id { get; private set; }
        public string Nombre { get; private set; }
        public string Documento { get; private set; }
        public string Email { get; private set; }
        public string Telefono { get; private set; }
        public bool Activo { get; private set; } = true; 
        
        public Cliente(string nombre, string documento, string email, string telefono)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre no puede estar vacío.", nameof(nombre));
            if (string.IsNullOrWhiteSpace(documento))
                throw new ArgumentException("El documento no puede estar vacío.", nameof(documento));

            Id = Guid.NewGuid();
            Nombre = nombre;
            Documento = documento;
            Email = email;
            Telefono = telefono;
            Activo = true;
        }

        public Cliente(Guid id, string nombre, string documento, string email, string telefono, bool activo)
        {
            Id = id;
            Nombre = nombre;
            Documento = documento;
            Email = email;
            Telefono = telefono;
            Activo = activo;
        }

        public void Desactivar()
        {
            Activo = false;
        }

        public void Reactivar()
        {
            Activo = true;
        }

        public void Actualizar(string nombre, string documento, string email, string telefono)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre no puede estar vacío.", nameof(nombre));
            if (string.IsNullOrWhiteSpace(documento))
                throw new ArgumentException("El documento no puede estar vacío.", nameof(documento));

            Nombre = nombre;
            Documento = documento;
            Email = email;
            Telefono = telefono;
        }
    }
}
