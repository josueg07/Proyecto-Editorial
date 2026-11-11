using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PlataformaEditorial.Web.Models.Entidades;

namespace PlataformaEditorial.Web.Data.Negocio;

public partial class EditorialDbContext : DbContext
{
    public EditorialDbContext(DbContextOptions<EditorialDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<tblAutor> tblAutors { get; set; }

    public virtual DbSet<tblBanner> tblBanners { get; set; }

    public virtual DbSet<tblCarrito> tblCarritos { get; set; }

    public virtual DbSet<tblCategorium> tblCategoria { get; set; }

    public virtual DbSet<tblConfiguracionSistema> tblConfiguracionSistemas { get; set; }

    public virtual DbSet<tblCotizacion> tblCotizacions { get; set; }

    public virtual DbSet<tblDetalleCarrito> tblDetalleCarritos { get; set; }

    public virtual DbSet<tblDetalleCotizacion> tblDetalleCotizacions { get; set; }

    public virtual DbSet<tblDetallePedido> tblDetallePedidos { get; set; }

    public virtual DbSet<tblDetalleSolicitudCotizacion> tblDetalleSolicitudCotizacions { get; set; }

    public virtual DbSet<tblEditorial> tblEditorials { get; set; }

    public virtual DbSet<tblFormatoLibro> tblFormatoLibros { get; set; }

    public virtual DbSet<tblLibro> tblLibros { get; set; }

    public virtual DbSet<tblPedido> tblPedidos { get; set; }

    public virtual DbSet<tblPerfilInstitucional> tblPerfilInstitucionals { get; set; }

    public virtual DbSet<tblPresentacionLibro> tblPresentacionLibros { get; set; }

    public virtual DbSet<tblSolicitudCotizacion> tblSolicitudCotizacions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<tblAutor>(entity =>
        {
            entity.HasKey(e => e.IDAutor);

            entity.ToTable("tblAutor");

            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_tblAutor_Activo");
            entity.Property(e => e.NombreAutor).HasMaxLength(150);
        });

        modelBuilder.Entity<tblBanner>(entity =>
        {
            entity.HasKey(e => e.IDBanner);

            entity.ToTable("tblBanner");

            entity.Property(e => e.EnlaceURL).HasMaxLength(500);
            entity.Property(e => e.ImagenURL).HasMaxLength(500);
            entity.Property(e => e.Titulo).HasMaxLength(150);
        });

        modelBuilder.Entity<tblCarrito>(entity =>
        {
            entity.HasKey(e => e.IDCarrito);

            entity.ToTable("tblCarrito");

            entity.HasIndex(e => e.IDUsuario, "idx_fk_tblCarrito_AspNetUsers_IDUsuario").IsUnique();

            entity.Property(e => e.FechaCreacion).HasPrecision(6);
        });

        modelBuilder.Entity<tblCategorium>(entity =>
        {
            entity.HasKey(e => e.IDCategoria);

            entity.HasIndex(e => e.NombreCategoria, "uq_Categoria_Nombre").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_tblCategoria_Activo");
            entity.Property(e => e.NombreCategoria).HasMaxLength(100);
        });

        modelBuilder.Entity<tblConfiguracionSistema>(entity =>
        {
            entity.HasKey(e => e.IDConfiguracion);

            entity.ToTable("tblConfiguracionSistema");

            entity.Property(e => e.PorcentajeImpuesto).HasColumnType("decimal(5, 2)");
        });

        modelBuilder.Entity<tblCotizacion>(entity =>
        {
            entity.HasKey(e => e.IDCotizacion);

            entity.ToTable("tblCotizacion");

            entity.HasIndex(e => e.IDSolicitud, "uq_Cotizacion_Solicitud").IsUnique();

            entity.Property(e => e.Descuento).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.EstadoCotizacion).HasMaxLength(20);
            entity.Property(e => e.FechaEmision).HasPrecision(0);
            entity.Property(e => e.Impuestos).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.Observaciones).HasMaxLength(500);
            entity.Property(e => e.SubtotalBase).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.TotalFinal).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.IDSolicitudNavigation).WithOne(p => p.tblCotizacion)
                .HasForeignKey<tblCotizacion>(d => d.IDSolicitud)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<tblDetalleCarrito>(entity =>
        {
            entity.HasKey(e => e.IDDetalleCarrito);

            entity.ToTable("tblDetalleCarrito");

            entity.HasIndex(e => e.IDPresentacion, "fk_tblDetalleCarrito_tblPresentacionLibro1_idx");

            entity.HasIndex(e => new { e.IDCarrito, e.IDPresentacion }, "uq_DetalleCarrito_CarritoPresentacion").IsUnique();

            entity.HasOne(d => d.IDCarritoNavigation).WithMany(p => p.tblDetalleCarritos)
                .HasForeignKey(d => d.IDCarrito)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.IDPresentacionNavigation).WithMany(p => p.tblDetalleCarritos)
                .HasForeignKey(d => d.IDPresentacion)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<tblDetalleCotizacion>(entity =>
        {
            entity.HasKey(e => e.IDDetalleCotizacion);

            entity.ToTable("tblDetalleCotizacion");

            entity.HasIndex(e => e.IDCotizacion, "fk_tblDetalleCotizacion_tblCotizacion1_idx");

            entity.HasIndex(e => e.IDDetalleSolicitud, "fk_tblDetalleCotizacion_tblDetalleSolicitudCotizacion1_idx");

            entity.Property(e => e.Descripcion).HasMaxLength(255);
            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IDCotizacionNavigation).WithMany(p => p.tblDetalleCotizacions)
                .HasForeignKey(d => d.IDCotizacion)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.IDDetalleSolicitudNavigation).WithMany(p => p.tblDetalleCotizacions).HasForeignKey(d => d.IDDetalleSolicitud);
        });

        modelBuilder.Entity<tblDetallePedido>(entity =>
        {
            entity.HasKey(e => e.IDDetallePedido);

            entity.ToTable("tblDetallePedido");

            entity.HasIndex(e => e.IDPedido, "fk_DetallePedido_tblPedido1_idx");

            entity.HasIndex(e => e.IDPresentacion, "fk_DetallePedido_tblPresentacionLibro1_idx");

            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IDPedidoNavigation).WithMany(p => p.tblDetallePedidos)
                .HasForeignKey(d => d.IDPedido)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.IDPresentacionNavigation).WithMany(p => p.tblDetallePedidos)
                .HasForeignKey(d => d.IDPresentacion)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<tblDetalleSolicitudCotizacion>(entity =>
        {
            entity.HasKey(e => e.IDDetalleSolicitud);

            entity.ToTable("tblDetalleSolicitudCotizacion");

            entity.HasIndex(e => e.IDLibro, "fk_tblDetalleSolicitudCotizacion_tblLibro1_idx");

            entity.HasIndex(e => e.IDSolicitud, "fk_tblDetalleSolicitudCotizacion_tblSolicitudCotizacion1_idx");

            entity.Property(e => e.AcabadoPortada).HasMaxLength(100);
            entity.Property(e => e.TipoPapel).HasMaxLength(100);
            entity.Property(e => e.TituloSolicitado).HasMaxLength(200);

            entity.HasOne(d => d.IDLibroNavigation).WithMany(p => p.tblDetalleSolicitudCotizacions).HasForeignKey(d => d.IDLibro);

            entity.HasOne(d => d.IDSolicitudNavigation).WithMany(p => p.tblDetalleSolicitudCotizacions)
                .HasForeignKey(d => d.IDSolicitud)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<tblEditorial>(entity =>
        {
            entity.HasKey(e => e.IDEditorial);

            entity.ToTable("tblEditorial");

            entity.HasIndex(e => e.NombreEditorial, "uq_Editorial_Nombre").IsUnique();

            entity.Property(e => e.NombreEditorial).HasMaxLength(150);
        });

        modelBuilder.Entity<tblFormatoLibro>(entity =>
        {
            entity.HasKey(e => e.IDFormato);

            entity.ToTable("tblFormatoLibro");

            entity.HasIndex(e => e.NombreFormato, "uq_Formato_Nombre").IsUnique();

            entity.Property(e => e.NombreFormato).HasMaxLength(50);
        });

        modelBuilder.Entity<tblLibro>(entity =>
        {
            entity.HasKey(e => e.IDLibro);

            entity.ToTable("tblLibro");

            entity.HasIndex(e => e.IDEditorial, "fk_tblLibro_tblEditorial1_idx");

            entity.Property(e => e.Activo).HasDefaultValue(true, "DF_tblLibro_Activo");
            entity.Property(e => e.Titulo).HasMaxLength(200);
            entity.Property(e => e.fechaRegistro).HasPrecision(6);
            entity.Property(e => e.portadaURL).HasMaxLength(500);

            entity.HasOne(d => d.IDEditorialNavigation).WithMany(p => p.tblLibros)
                .HasForeignKey(d => d.IDEditorial)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasMany(d => d.IDAutors).WithMany(p => p.IDLibros)
                .UsingEntity<Dictionary<string, object>>(
                    "tblLibro_Autor",
                    r => r.HasOne<tblAutor>().WithMany()
                        .HasForeignKey("IDAutor")
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    l => l.HasOne<tblLibro>().WithMany()
                        .HasForeignKey("IDLibro")
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j =>
                    {
                        j.HasKey("IDLibro", "IDAutor");
                        j.ToTable("tblLibro_Autor");
                        j.HasIndex(new[] { "IDAutor" }, "fk_tblLibro_has_tblAutor_tblAutor1_idx");
                    });

            entity.HasMany(d => d.IDCategoria).WithMany(p => p.IDLibros)
                .UsingEntity<Dictionary<string, object>>(
                    "tblLibro_Categorium",
                    r => r.HasOne<tblCategorium>().WithMany()
                        .HasForeignKey("IDCategoria")
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    l => l.HasOne<tblLibro>().WithMany()
                        .HasForeignKey("IDLibro")
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j =>
                    {
                        j.HasKey("IDLibro", "IDCategoria");
                        j.ToTable("tblLibro_Categoria");
                        j.HasIndex(new[] { "IDCategoria" }, "fk_tblLibro_has_tblCategoria_tblCategoria1_idx");
                    });
        });

        modelBuilder.Entity<tblPedido>(entity =>
        {
            entity.HasKey(e => e.IDPedido);

            entity.ToTable("tblPedido");

            entity.HasIndex(e => e.IDUsuario, "idx_fk_tblPedido_AspNetUsers_IDUsuario");

            entity.Property(e => e.DescuentoInstitucional).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.DescuentoVolumen).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.DireccionEntrega).HasMaxLength(255);
            entity.Property(e => e.EstadoPedido).HasMaxLength(20);
            entity.Property(e => e.FechaPedido).HasPrecision(6);
            entity.Property(e => e.Impuestos).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.PorcentajeImpuesto).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.SubtotalBase).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.TotalFinal).HasColumnType("decimal(12, 2)");
        });

        modelBuilder.Entity<tblPerfilInstitucional>(entity =>
        {
            entity.HasKey(e => e.IDUsuario);

            entity.ToTable("tblPerfilInstitucional");

            entity.HasIndex(e => e.IDUsuario, "idx_fk_tblPerfilInstitucional_AspNetUsers_IDUsuario").IsUnique();

            entity.Property(e => e.Direccion).HasMaxLength(255);
            entity.Property(e => e.EstadoValidacion).HasMaxLength(20);
            entity.Property(e => e.FechaValidacion).HasPrecision(6);
            entity.Property(e => e.NIT).HasMaxLength(20);
            entity.Property(e => e.NombreInstitucion).HasMaxLength(200);
        });

        modelBuilder.Entity<tblPresentacionLibro>(entity =>
        {
            entity.HasKey(e => e.IDPresentacion);

            entity.ToTable("tblPresentacionLibro");

            entity.HasIndex(e => e.IDFormato, "fk_tblPresentacionLibro_tblFormatoLibro1_idx");

            entity.HasIndex(e => e.isbn, "uq_Presentacion_ISBN").IsUnique();

            entity.HasIndex(e => new { e.IDLibro, e.IDFormato }, "uq_Presentacion_LibroFormato").IsUnique();

            entity.Property(e => e.PrecioBase).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.isbn).HasMaxLength(20);

            entity.HasOne(d => d.IDFormatoNavigation).WithMany(p => p.tblPresentacionLibros)
                .HasForeignKey(d => d.IDFormato)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.IDLibroNavigation).WithMany(p => p.tblPresentacionLibros)
                .HasForeignKey(d => d.IDLibro)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<tblSolicitudCotizacion>(entity =>
        {
            entity.HasKey(e => e.IDSolicitud);

            entity.ToTable("tblSolicitudCotizacion");

            entity.HasIndex(e => e.IDAsesor, "idx_fk_tblSolicitudCotizacion_AspNetUsers_IDAsesor");

            entity.HasIndex(e => e.IDUsuario, "idx_fk_tblSolicitudCotizacion_AspNetUsers_IDUsuario");

            entity.Property(e => e.EstadoSolicitud).HasMaxLength(20);
            entity.Property(e => e.FechaSolicitud).HasPrecision(0);
            entity.Property(e => e.Observaciones).HasMaxLength(500);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
