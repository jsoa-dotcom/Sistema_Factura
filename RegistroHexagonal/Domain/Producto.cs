using Microsoft.Win32;
using System;

namespace RegistroHexagonal.Domain
{
    public class Producto
    {
        public Guid Id { get; private set; }
        public string Nombre { get; private set; }
        public string Descripcion { get; private set; }
        public decimal PrecioBruto { get; private set; }   
        public decimal Iva { get; private set; }            
        public Guid CategoriaId { get; private set; }
        public bool Activo { get; private set; } = true; 
        public Categoria? Categoria { get; private set; }   

        public Producto(string nombre, string descripcion, decimal precioBruto, decimal iva, Guid categoriaId)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre no puede estar vacío.", nameof(nombre));

            ValidarPrecioEIva(precioBruto, iva);

            Id = Guid.NewGuid();
            Nombre = nombre;
            Descripcion = descripcion;
            PrecioBruto = precioBruto;
            Iva = iva;
            CategoriaId = categoriaId;
            Activo = true;
        }

        public Producto(Guid id, string nombre, string descripcion, decimal precioBruto, decimal iva, Guid categoriaId, bool activo)
        {
            Id = id;
            Nombre = nombre;
            Descripcion = descripcion;
            PrecioBruto = precioBruto;
            Iva = iva;
            CategoriaId = categoriaId;
            Activo = activo;
        }

        public void Actualizar(string nombre, string descripcion, decimal precioBruto, decimal iva, Guid categoriaId)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre no puede estar vacío.", nameof(nombre));

            ValidarPrecioEIva(precioBruto, iva);

            Nombre = nombre;
            Descripcion = descripcion;
            PrecioBruto = precioBruto;
            Iva = iva;
            CategoriaId = categoriaId;
        }

        public void Desactivar() 
        {
            Activo = false;
        }

        public void Reactivar()
        {
            Activo = true;
        }

        private static void ValidarPrecioEIva(decimal precioBruto, decimal iva)
        {
            if (precioBruto < 0)
                throw new ArgumentException("El precio bruto no puede ser negativo.", nameof(precioBruto));
            if (iva < 0)
                throw new ArgumentException("El IVA no puede ser negativo.", nameof(iva));
        }
    }
}
