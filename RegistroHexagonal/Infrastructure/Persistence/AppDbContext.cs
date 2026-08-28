using Microsoft.EntityFrameworkCore;
using RegistroHexagonal.Domain;

namespace RegistroHexagonal.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Categoria> Categorias => Set<Categoria>();
        public DbSet<Producto> Productos => Set<Producto>();
        public DbSet<Factura> Facturas => Set<Factura>();
        public DbSet<DetalleFactura> DetallesFactura => Set<DetalleFactura>();
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.ToTable("Clientes");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Nombre).IsRequired().HasMaxLength(200);
                entity.Property(c => c.Documento).IsRequired().HasMaxLength(50);
                entity.Property(c => c.Email).HasMaxLength(200);
                entity.Property(c => c.Telefono).HasMaxLength(50);
                entity.HasIndex(c => c.Documento).IsUnique();   
            });

            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.ToTable("Categorias");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Nombre).IsRequired().HasMaxLength(100);
            });

            modelBuilder.Entity<Producto>(entity =>
            {
                entity.ToTable("Productos");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Nombre).IsRequired().HasMaxLength(200);
                entity.Property(p => p.Descripcion).HasMaxLength(1000);
                entity.Property(p => p.PrecioBruto).HasColumnType("decimal(18,2)");
                entity.Property(p => p.Iva).HasColumnType("decimal(5,2)");

                entity.HasOne(p => p.Categoria)
                      .WithMany()
                      .HasForeignKey(p => p.CategoriaId)
                      .OnDelete(DeleteBehavior.Restrict);   
            });

            modelBuilder.Entity<Factura>(entity =>
            {
                entity.ToTable("Facturas");
                entity.HasKey(f => f.Id);
                entity.Property(f => f.NumeroFactura).IsRequired().HasMaxLength(50);
                entity.Property(f => f.Descuento).HasColumnType("decimal(18,2)");
                entity.Property(f => f.Total).HasColumnType("decimal(18,2)");
                entity.HasIndex(f => f.NumeroFactura).IsUnique();
                entity.Property(f => f.NombreCliente).IsRequired().HasMaxLength(200);
                entity.Property(f => f.DocumentoCliente).IsRequired().HasMaxLength(50);
                
                entity.HasOne(f => f.Cliente)
                      .WithMany()
                      .HasForeignKey(f => f.ClienteId)
                      .OnDelete(DeleteBehavior.Restrict);   

                entity.HasMany(f => f.Detalles)
                      .WithOne()
                      .HasForeignKey(d => d.FacturaId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.Property(f => f.Anulada).IsRequired();
                entity.Property(f => f.Motivo).HasMaxLength(500);
            });

            modelBuilder.Entity<DetalleFactura>(entity =>
            {
                entity.ToTable("DetallesFactura");
                entity.HasKey(d => d.Id);
                entity.Property(d => d.PrecioUnitarioBruto).HasColumnType("decimal(18,2)");
                entity.Property(d => d.Iva).HasColumnType("decimal(5,2)");
                entity.Property(d => d.Descuento).HasColumnType("decimal(18,2)");
                entity.Property(d => d.Subtotal).HasColumnType("decimal(18,2)");
                entity.Property(d => d.BaseGravable).HasColumnType("decimal(18,2)");
                entity.Property(d => d.ValorIva).HasColumnType("decimal(18,2)");
                entity.Property(d => d.NombreProducto).IsRequired().HasMaxLength(200);

                entity.HasOne(d => d.Producto)
                      .WithMany()
                      .HasForeignKey(d => d.ProductoId)
                      .OnDelete(DeleteBehavior.Restrict);   
            });
        }
    }
}
