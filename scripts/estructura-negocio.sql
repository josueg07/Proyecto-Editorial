-- Esquema de las 19 tablas del negocio, leído de EditorialDB el 13/11/2026.
-- Instalación en otra computadora: aplicar primero las migraciones de Identity.
-- No elimina tablas ni datos y no altera columnas existentes.
USE [EditorialDB];
GO
SET XACT_ABORT ON;
IF OBJECT_ID(N'dbo.AspNetUsers',N'U') IS NULL THROW 50001,'Aplique primero las migraciones de ApplicationDbContext.',1;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.tblAutor',N'U') IS NULL
CREATE TABLE [dbo].[tblAutor] (
    [IDAutor] int IDENTITY(1,1) NOT NULL,
    [NombreAutor] nvarchar(150) NOT NULL,
    [Activo] bit NOT NULL CONSTRAINT [DF_tblAutor_Activo] DEFAULT ((1)),
    CONSTRAINT [PK_tblAutor] PRIMARY KEY ([IDAutor])
);
IF OBJECT_ID(N'dbo.tblBanner',N'U') IS NULL
CREATE TABLE [dbo].[tblBanner] (
    [IDBanner] int IDENTITY(1,1) NOT NULL,
    [Titulo] nvarchar(150) NOT NULL,
    [ImagenURL] nvarchar(500) NOT NULL,
    [EnlaceURL] nvarchar(500) NULL,
    [FechaInicio] date NULL,
    [FechaFin] date NULL,
    [OrdenVisualizacion] int NOT NULL,
    CONSTRAINT [PK_tblBanner] PRIMARY KEY ([IDBanner])
);
IF OBJECT_ID(N'dbo.tblCarrito',N'U') IS NULL
CREATE TABLE [dbo].[tblCarrito] (
    [IDCarrito] int IDENTITY(1,1) NOT NULL,
    [FechaCreacion] datetime2(6) NOT NULL,
    [IDUsuario] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_tblCarrito] PRIMARY KEY ([IDCarrito])
);
IF OBJECT_ID(N'dbo.tblCategoria',N'U') IS NULL
CREATE TABLE [dbo].[tblCategoria] (
    [IDCategoria] int IDENTITY(1,1) NOT NULL,
    [NombreCategoria] nvarchar(100) NOT NULL,
    [Activo] bit NOT NULL CONSTRAINT [DF_tblCategoria_Activo] DEFAULT ((1)),
    CONSTRAINT [PK_tblCategoria] PRIMARY KEY ([IDCategoria])
);
IF OBJECT_ID(N'dbo.tblConfiguracionSistema',N'U') IS NULL
CREATE TABLE [dbo].[tblConfiguracionSistema] (
    [IDConfiguracion] int IDENTITY(1,1) NOT NULL,
    [PorcentajeImpuesto] decimal(5,2) NOT NULL,
    CONSTRAINT [PK_tblConfiguracionSistema] PRIMARY KEY ([IDConfiguracion])
);
IF OBJECT_ID(N'dbo.tblCotizacion',N'U') IS NULL
CREATE TABLE [dbo].[tblCotizacion] (
    [IDCotizacion] int IDENTITY(1,1) NOT NULL,
    [FechaEmision] datetime2(0) NOT NULL,
    [FechaVencimiento] date NOT NULL,
    [EstadoCotizacion] nvarchar(20) NOT NULL,
    [SubtotalBase] decimal(12,2) NOT NULL,
    [Descuento] decimal(12,2) NOT NULL,
    [Impuestos] decimal(12,2) NOT NULL,
    [TotalFinal] decimal(12,2) NOT NULL,
    [Observaciones] nvarchar(500) NULL,
    [IDSolicitud] int NOT NULL,
    CONSTRAINT [PK_tblCotizacion] PRIMARY KEY ([IDCotizacion])
);
IF OBJECT_ID(N'dbo.tblDetalleCarrito',N'U') IS NULL
CREATE TABLE [dbo].[tblDetalleCarrito] (
    [IDDetalleCarrito] int IDENTITY(1,1) NOT NULL,
    [Cantidad] int NOT NULL,
    [IDPresentacion] int NOT NULL,
    [IDCarrito] int NOT NULL,
    CONSTRAINT [PK_tblDetalleCarrito] PRIMARY KEY ([IDDetalleCarrito])
);
IF OBJECT_ID(N'dbo.tblDetalleCotizacion',N'U') IS NULL
CREATE TABLE [dbo].[tblDetalleCotizacion] (
    [IDDetalleCotizacion] int IDENTITY(1,1) NOT NULL,
    [Descripcion] nvarchar(255) NOT NULL,
    [Cantidad] int NOT NULL,
    [PrecioUnitario] decimal(10,2) NOT NULL,
    [IDCotizacion] int NOT NULL,
    [IDDetalleSolicitud] int NULL,
    CONSTRAINT [PK_tblDetalleCotizacion] PRIMARY KEY ([IDDetalleCotizacion])
);
IF OBJECT_ID(N'dbo.tblDetallePedido',N'U') IS NULL
CREATE TABLE [dbo].[tblDetallePedido] (
    [IDDetallePedido] int IDENTITY(1,1) NOT NULL,
    [Cantidad] int NOT NULL,
    [PrecioUnitario] decimal(10,2) NOT NULL,
    [IDPedido] int NOT NULL,
    [IDPresentacion] int NOT NULL,
    CONSTRAINT [PK_tblDetallePedido] PRIMARY KEY ([IDDetallePedido])
);
IF OBJECT_ID(N'dbo.tblDetalleSolicitudCotizacion',N'U') IS NULL
CREATE TABLE [dbo].[tblDetalleSolicitudCotizacion] (
    [IDDetalleSolicitud] int IDENTITY(1,1) NOT NULL,
    [TituloSolicitado] nvarchar(200) NOT NULL,
    [CantidadEstimada] int NOT NULL,
    [AcabadoPortada] nvarchar(100) NOT NULL,
    [TipoPapel] nvarchar(100) NOT NULL,
    [IDSolicitud] int NOT NULL,
    [IDLibro] int NULL,
    CONSTRAINT [PK_tblDetalleSolicitudCotizacion] PRIMARY KEY ([IDDetalleSolicitud])
);
IF OBJECT_ID(N'dbo.tblEditorial',N'U') IS NULL
CREATE TABLE [dbo].[tblEditorial] (
    [IDEditorial] int IDENTITY(1,1) NOT NULL,
    [NombreEditorial] nvarchar(150) NOT NULL,
    CONSTRAINT [PK_tblEditorial] PRIMARY KEY ([IDEditorial])
);
IF OBJECT_ID(N'dbo.tblFormatoLibro',N'U') IS NULL
CREATE TABLE [dbo].[tblFormatoLibro] (
    [IDFormato] int IDENTITY(1,1) NOT NULL,
    [NombreFormato] nvarchar(50) NOT NULL,
    [EsDigital] bit NOT NULL CONSTRAINT [DF_tblFormatoLibro_EsDigital] DEFAULT ((0)),
    CONSTRAINT [PK_tblFormatoLibro] PRIMARY KEY ([IDFormato])
);
IF OBJECT_ID(N'dbo.tblLibro',N'U') IS NULL
CREATE TABLE [dbo].[tblLibro] (
    [IDLibro] int IDENTITY(1,1) NOT NULL,
    [Titulo] nvarchar(200) NOT NULL,
    [Sinopsis] nvarchar(max) NOT NULL,
    [AnioEdicion] smallint NOT NULL,
    [portadaURL] nvarchar(500) NOT NULL,
    [fechaRegistro] datetime2(6) NOT NULL,
    [Destacado] bit NOT NULL CONSTRAINT [DF_tblLibro_Destacado] DEFAULT ((0)),
    [Activo] bit NOT NULL CONSTRAINT [DF_tblLibro_Activo] DEFAULT ((1)),
    [IDEditorial] int NOT NULL,
    CONSTRAINT [PK_tblLibro] PRIMARY KEY ([IDLibro])
);
IF OBJECT_ID(N'dbo.tblLibro_Autor',N'U') IS NULL
CREATE TABLE [dbo].[tblLibro_Autor] (
    [IDLibro] int NOT NULL,
    [IDAutor] int NOT NULL,
    CONSTRAINT [PK_tblLibro_Autor] PRIMARY KEY ([IDLibro],[IDAutor])
);
IF OBJECT_ID(N'dbo.tblLibro_Categoria',N'U') IS NULL
CREATE TABLE [dbo].[tblLibro_Categoria] (
    [IDLibro] int NOT NULL,
    [IDCategoria] int NOT NULL,
    CONSTRAINT [PK_tblLibro_Categoria] PRIMARY KEY ([IDLibro],[IDCategoria])
);
IF OBJECT_ID(N'dbo.tblPedido',N'U') IS NULL
CREATE TABLE [dbo].[tblPedido] (
    [IDPedido] int IDENTITY(1,1) NOT NULL,
    [FechaPedido] datetime2(6) NOT NULL,
    [EstadoPedido] nvarchar(20) NOT NULL,
    [DireccionEntrega] nvarchar(255) NULL,
    [SubtotalBase] decimal(12,2) NOT NULL,
    [DescuentoVolumen] decimal(12,2) NOT NULL,
    [DescuentoInstitucional] decimal(12,2) NOT NULL,
    [PorcentajeImpuesto] decimal(5,2) NOT NULL,
    [Impuestos] decimal(12,2) NOT NULL,
    [TotalFinal] decimal(12,2) NOT NULL,
    [IDUsuario] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_tblPedido] PRIMARY KEY ([IDPedido])
);
IF OBJECT_ID(N'dbo.tblPerfilInstitucional',N'U') IS NULL
CREATE TABLE [dbo].[tblPerfilInstitucional] (
    [IDUsuario] nvarchar(450) NOT NULL,
    [NombreInstitucion] nvarchar(200) NOT NULL,
    [NIT] nvarchar(20) NOT NULL,
    [Direccion] nvarchar(255) NOT NULL,
    [EstadoValidacion] nvarchar(20) NOT NULL,
    [FechaValidacion] datetime2(6) NULL,
    CONSTRAINT [PK_tblPerfilInstitucional] PRIMARY KEY ([IDUsuario])
);
IF OBJECT_ID(N'dbo.tblPresentacionLibro',N'U') IS NULL
CREATE TABLE [dbo].[tblPresentacionLibro] (
    [IDPresentacion] int IDENTITY(1,1) NOT NULL,
    [isbn] nvarchar(20) NOT NULL,
    [PrecioBase] decimal(10,2) NOT NULL,
    [Existencias] int NULL,
    [IDLibro] int NOT NULL,
    [IDFormato] int NOT NULL,
    CONSTRAINT [PK_tblPresentacionLibro] PRIMARY KEY ([IDPresentacion])
);
IF OBJECT_ID(N'dbo.tblSolicitudCotizacion',N'U') IS NULL
CREATE TABLE [dbo].[tblSolicitudCotizacion] (
    [IDSolicitud] int IDENTITY(1,1) NOT NULL,
    [FechaSolicitud] datetime2(0) NOT NULL,
    [FechaEntregaRequerida] date NOT NULL,
    [EstadoSolicitud] nvarchar(20) NOT NULL,
    [Observaciones] nvarchar(500) NULL,
    [IDUsuario] nvarchar(450) NOT NULL,
    [IDAsesor] nvarchar(450) NULL,
    CONSTRAINT [PK_tblSolicitudCotizacion] PRIMARY KEY ([IDSolicitud])
);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.tblCarrito') AND name=N'idx_fk_tblCarrito_AspNetUsers_IDUsuario')
CREATE UNIQUE INDEX [idx_fk_tblCarrito_AspNetUsers_IDUsuario] ON [dbo].[tblCarrito]([IDUsuario]);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.tblCategoria') AND name=N'uq_Categoria_Nombre')
CREATE UNIQUE INDEX [uq_Categoria_Nombre] ON [dbo].[tblCategoria]([NombreCategoria]);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.tblCotizacion') AND name=N'uq_Cotizacion_Solicitud')
CREATE UNIQUE INDEX [uq_Cotizacion_Solicitud] ON [dbo].[tblCotizacion]([IDSolicitud]);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.tblDetalleCarrito') AND name=N'fk_tblDetalleCarrito_tblPresentacionLibro1_idx')
CREATE INDEX [fk_tblDetalleCarrito_tblPresentacionLibro1_idx] ON [dbo].[tblDetalleCarrito]([IDPresentacion]);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.tblDetalleCarrito') AND name=N'uq_DetalleCarrito_CarritoPresentacion')
CREATE UNIQUE INDEX [uq_DetalleCarrito_CarritoPresentacion] ON [dbo].[tblDetalleCarrito]([IDCarrito],[IDPresentacion]);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.tblDetalleCotizacion') AND name=N'fk_tblDetalleCotizacion_tblCotizacion1_idx')
CREATE INDEX [fk_tblDetalleCotizacion_tblCotizacion1_idx] ON [dbo].[tblDetalleCotizacion]([IDCotizacion]);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.tblDetalleCotizacion') AND name=N'fk_tblDetalleCotizacion_tblDetalleSolicitudCotizacion1_idx')
CREATE INDEX [fk_tblDetalleCotizacion_tblDetalleSolicitudCotizacion1_idx] ON [dbo].[tblDetalleCotizacion]([IDDetalleSolicitud]);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.tblDetallePedido') AND name=N'fk_DetallePedido_tblPedido1_idx')
CREATE INDEX [fk_DetallePedido_tblPedido1_idx] ON [dbo].[tblDetallePedido]([IDPedido]);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.tblDetallePedido') AND name=N'fk_DetallePedido_tblPresentacionLibro1_idx')
CREATE INDEX [fk_DetallePedido_tblPresentacionLibro1_idx] ON [dbo].[tblDetallePedido]([IDPresentacion]);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.tblDetalleSolicitudCotizacion') AND name=N'fk_tblDetalleSolicitudCotizacion_tblLibro1_idx')
CREATE INDEX [fk_tblDetalleSolicitudCotizacion_tblLibro1_idx] ON [dbo].[tblDetalleSolicitudCotizacion]([IDLibro]);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.tblDetalleSolicitudCotizacion') AND name=N'fk_tblDetalleSolicitudCotizacion_tblSolicitudCotizacion1_idx')
CREATE INDEX [fk_tblDetalleSolicitudCotizacion_tblSolicitudCotizacion1_idx] ON [dbo].[tblDetalleSolicitudCotizacion]([IDSolicitud]);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.tblEditorial') AND name=N'uq_Editorial_Nombre')
CREATE UNIQUE INDEX [uq_Editorial_Nombre] ON [dbo].[tblEditorial]([NombreEditorial]);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.tblFormatoLibro') AND name=N'uq_Formato_Nombre')
CREATE UNIQUE INDEX [uq_Formato_Nombre] ON [dbo].[tblFormatoLibro]([NombreFormato]);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.tblLibro') AND name=N'fk_tblLibro_tblEditorial1_idx')
CREATE INDEX [fk_tblLibro_tblEditorial1_idx] ON [dbo].[tblLibro]([IDEditorial]);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.tblLibro_Autor') AND name=N'fk_tblLibro_has_tblAutor_tblAutor1_idx')
CREATE INDEX [fk_tblLibro_has_tblAutor_tblAutor1_idx] ON [dbo].[tblLibro_Autor]([IDAutor]);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.tblLibro_Categoria') AND name=N'fk_tblLibro_has_tblCategoria_tblCategoria1_idx')
CREATE INDEX [fk_tblLibro_has_tblCategoria_tblCategoria1_idx] ON [dbo].[tblLibro_Categoria]([IDCategoria]);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.tblPedido') AND name=N'idx_fk_tblPedido_AspNetUsers_IDUsuario')
CREATE INDEX [idx_fk_tblPedido_AspNetUsers_IDUsuario] ON [dbo].[tblPedido]([IDUsuario]);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.tblPerfilInstitucional') AND name=N'idx_fk_tblPerfilInstitucional_AspNetUsers_IDUsuario')
CREATE UNIQUE INDEX [idx_fk_tblPerfilInstitucional_AspNetUsers_IDUsuario] ON [dbo].[tblPerfilInstitucional]([IDUsuario]);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.tblPresentacionLibro') AND name=N'fk_tblPresentacionLibro_tblFormatoLibro1_idx')
CREATE INDEX [fk_tblPresentacionLibro_tblFormatoLibro1_idx] ON [dbo].[tblPresentacionLibro]([IDFormato]);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.tblPresentacionLibro') AND name=N'uq_Presentacion_ISBN')
CREATE UNIQUE INDEX [uq_Presentacion_ISBN] ON [dbo].[tblPresentacionLibro]([isbn]);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.tblPresentacionLibro') AND name=N'uq_Presentacion_LibroFormato')
CREATE UNIQUE INDEX [uq_Presentacion_LibroFormato] ON [dbo].[tblPresentacionLibro]([IDLibro],[IDFormato]);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.tblSolicitudCotizacion') AND name=N'idx_fk_tblSolicitudCotizacion_AspNetUsers_IDAsesor')
CREATE INDEX [idx_fk_tblSolicitudCotizacion_AspNetUsers_IDAsesor] ON [dbo].[tblSolicitudCotizacion]([IDAsesor]);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.tblSolicitudCotizacion') AND name=N'idx_fk_tblSolicitudCotizacion_AspNetUsers_IDUsuario')
CREATE INDEX [idx_fk_tblSolicitudCotizacion_AspNetUsers_IDUsuario] ON [dbo].[tblSolicitudCotizacion]([IDUsuario]);
IF OBJECT_ID(N'dbo.CK_tblDetalleCotizacion_PrecioUnitario',N'C') IS NULL
ALTER TABLE [dbo].[tblDetalleCotizacion] ADD CONSTRAINT [CK_tblDetalleCotizacion_PrecioUnitario] CHECK ([PrecioUnitario]>=(0));
IF OBJECT_ID(N'dbo.CK_tblConfiguracionSistema_PorcentajeImpuesto',N'C') IS NULL
ALTER TABLE [dbo].[tblConfiguracionSistema] ADD CONSTRAINT [CK_tblConfiguracionSistema_PorcentajeImpuesto] CHECK ([PorcentajeImpuesto]>=(0) AND [PorcentajeImpuesto]<=(100));
IF OBJECT_ID(N'dbo.CK_tblPresentacionLibro_PrecioBase',N'C') IS NULL
ALTER TABLE [dbo].[tblPresentacionLibro] ADD CONSTRAINT [CK_tblPresentacionLibro_PrecioBase] CHECK ([PrecioBase]>=(0));
IF OBJECT_ID(N'dbo.CK_tblPresentacionLibro_Existencias',N'C') IS NULL
ALTER TABLE [dbo].[tblPresentacionLibro] ADD CONSTRAINT [CK_tblPresentacionLibro_Existencias] CHECK ([Existencias] IS NULL OR [Existencias]>=(0));
IF OBJECT_ID(N'dbo.CK_tblBanner_RangoFechas',N'C') IS NULL
ALTER TABLE [dbo].[tblBanner] ADD CONSTRAINT [CK_tblBanner_RangoFechas] CHECK ([FechaInicio] IS NULL OR [FechaFin] IS NULL OR [FechaFin]>=[FechaInicio]);
IF OBJECT_ID(N'dbo.CK_tblDetalleCarrito_Cantidad',N'C') IS NULL
ALTER TABLE [dbo].[tblDetalleCarrito] ADD CONSTRAINT [CK_tblDetalleCarrito_Cantidad] CHECK ([Cantidad]>(0));
IF OBJECT_ID(N'dbo.CK_tblPedido_SubtotalBase',N'C') IS NULL
ALTER TABLE [dbo].[tblPedido] ADD CONSTRAINT [CK_tblPedido_SubtotalBase] CHECK ([SubtotalBase]>=(0));
IF OBJECT_ID(N'dbo.CK_tblPedido_DescuentoVolumen',N'C') IS NULL
ALTER TABLE [dbo].[tblPedido] ADD CONSTRAINT [CK_tblPedido_DescuentoVolumen] CHECK ([DescuentoVolumen]>=(0));
IF OBJECT_ID(N'dbo.CK_tblPedido_DescuentoInstitucional',N'C') IS NULL
ALTER TABLE [dbo].[tblPedido] ADD CONSTRAINT [CK_tblPedido_DescuentoInstitucional] CHECK ([DescuentoInstitucional]>=(0));
IF OBJECT_ID(N'dbo.CK_tblPedido_PorcentajeImpuesto',N'C') IS NULL
ALTER TABLE [dbo].[tblPedido] ADD CONSTRAINT [CK_tblPedido_PorcentajeImpuesto] CHECK ([PorcentajeImpuesto]>=(0) AND [PorcentajeImpuesto]<=(100));
IF OBJECT_ID(N'dbo.CK_tblPedido_Impuestos',N'C') IS NULL
ALTER TABLE [dbo].[tblPedido] ADD CONSTRAINT [CK_tblPedido_Impuestos] CHECK ([Impuestos]>=(0));
IF OBJECT_ID(N'dbo.CK_tblPedido_TotalFinal',N'C') IS NULL
ALTER TABLE [dbo].[tblPedido] ADD CONSTRAINT [CK_tblPedido_TotalFinal] CHECK ([TotalFinal]>=(0));
IF OBJECT_ID(N'dbo.CK_tblDetallePedido_Cantidad',N'C') IS NULL
ALTER TABLE [dbo].[tblDetallePedido] ADD CONSTRAINT [CK_tblDetallePedido_Cantidad] CHECK ([Cantidad]>(0));
IF OBJECT_ID(N'dbo.CK_tblDetallePedido_PrecioUnitario',N'C') IS NULL
ALTER TABLE [dbo].[tblDetallePedido] ADD CONSTRAINT [CK_tblDetallePedido_PrecioUnitario] CHECK ([PrecioUnitario]>=(0));
IF OBJECT_ID(N'dbo.CK_tblDetalleSolicitudCotizacion_CantidadEstimada',N'C') IS NULL
ALTER TABLE [dbo].[tblDetalleSolicitudCotizacion] ADD CONSTRAINT [CK_tblDetalleSolicitudCotizacion_CantidadEstimada] CHECK ([CantidadEstimada]>(0));
IF OBJECT_ID(N'dbo.CK_tblCotizacion_SubtotalBase',N'C') IS NULL
ALTER TABLE [dbo].[tblCotizacion] ADD CONSTRAINT [CK_tblCotizacion_SubtotalBase] CHECK ([SubtotalBase]>=(0));
IF OBJECT_ID(N'dbo.CK_tblCotizacion_Descuento',N'C') IS NULL
ALTER TABLE [dbo].[tblCotizacion] ADD CONSTRAINT [CK_tblCotizacion_Descuento] CHECK ([Descuento]>=(0));
IF OBJECT_ID(N'dbo.CK_tblCotizacion_Impuestos',N'C') IS NULL
ALTER TABLE [dbo].[tblCotizacion] ADD CONSTRAINT [CK_tblCotizacion_Impuestos] CHECK ([Impuestos]>=(0));
IF OBJECT_ID(N'dbo.CK_tblCotizacion_TotalFinal',N'C') IS NULL
ALTER TABLE [dbo].[tblCotizacion] ADD CONSTRAINT [CK_tblCotizacion_TotalFinal] CHECK ([TotalFinal]>=(0));
IF OBJECT_ID(N'dbo.CK_tblDetalleCotizacion_Cantidad',N'C') IS NULL
ALTER TABLE [dbo].[tblDetalleCotizacion] ADD CONSTRAINT [CK_tblDetalleCotizacion_Cantidad] CHECK ([Cantidad]>(0));
IF OBJECT_ID(N'dbo.FK_tblPerfilInstitucional_AspNetUsers_IDUsuario',N'F') IS NULL
ALTER TABLE [dbo].[tblPerfilInstitucional] ADD CONSTRAINT [FK_tblPerfilInstitucional_AspNetUsers_IDUsuario] FOREIGN KEY([IDUsuario]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE NO ACTION;
IF OBJECT_ID(N'dbo.FK_tblCarrito_AspNetUsers_IDUsuario',N'F') IS NULL
ALTER TABLE [dbo].[tblCarrito] ADD CONSTRAINT [FK_tblCarrito_AspNetUsers_IDUsuario] FOREIGN KEY([IDUsuario]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE NO ACTION;
IF OBJECT_ID(N'dbo.FK_tblPedido_AspNetUsers_IDUsuario',N'F') IS NULL
ALTER TABLE [dbo].[tblPedido] ADD CONSTRAINT [FK_tblPedido_AspNetUsers_IDUsuario] FOREIGN KEY([IDUsuario]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE NO ACTION;
IF OBJECT_ID(N'dbo.FK_tblSolicitudCotizacion_AspNetUsers_IDUsuario',N'F') IS NULL
ALTER TABLE [dbo].[tblSolicitudCotizacion] ADD CONSTRAINT [FK_tblSolicitudCotizacion_AspNetUsers_IDUsuario] FOREIGN KEY([IDUsuario]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE NO ACTION;
IF OBJECT_ID(N'dbo.FK_tblSolicitudCotizacion_AspNetUsers_IDAsesor',N'F') IS NULL
ALTER TABLE [dbo].[tblSolicitudCotizacion] ADD CONSTRAINT [FK_tblSolicitudCotizacion_AspNetUsers_IDAsesor] FOREIGN KEY([IDAsesor]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE NO ACTION;
IF OBJECT_ID(N'dbo.FK_tblLibro_tblEditorial_IDEditorial',N'F') IS NULL
ALTER TABLE [dbo].[tblLibro] ADD CONSTRAINT [FK_tblLibro_tblEditorial_IDEditorial] FOREIGN KEY([IDEditorial]) REFERENCES [dbo].[tblEditorial]([IDEditorial]) ON DELETE NO ACTION;
IF OBJECT_ID(N'dbo.FK_tblLibro_Autor_tblAutor_IDAutor',N'F') IS NULL
ALTER TABLE [dbo].[tblLibro_Autor] ADD CONSTRAINT [FK_tblLibro_Autor_tblAutor_IDAutor] FOREIGN KEY([IDAutor]) REFERENCES [dbo].[tblAutor]([IDAutor]) ON DELETE NO ACTION;
IF OBJECT_ID(N'dbo.FK_tblLibro_Categoria_tblCategoria_IDCategoria',N'F') IS NULL
ALTER TABLE [dbo].[tblLibro_Categoria] ADD CONSTRAINT [FK_tblLibro_Categoria_tblCategoria_IDCategoria] FOREIGN KEY([IDCategoria]) REFERENCES [dbo].[tblCategoria]([IDCategoria]) ON DELETE NO ACTION;
IF OBJECT_ID(N'dbo.FK_tblLibro_Autor_tblLibro_IDLibro',N'F') IS NULL
ALTER TABLE [dbo].[tblLibro_Autor] ADD CONSTRAINT [FK_tblLibro_Autor_tblLibro_IDLibro] FOREIGN KEY([IDLibro]) REFERENCES [dbo].[tblLibro]([IDLibro]) ON DELETE NO ACTION;
IF OBJECT_ID(N'dbo.FK_tblLibro_Categoria_tblLibro_IDLibro',N'F') IS NULL
ALTER TABLE [dbo].[tblLibro_Categoria] ADD CONSTRAINT [FK_tblLibro_Categoria_tblLibro_IDLibro] FOREIGN KEY([IDLibro]) REFERENCES [dbo].[tblLibro]([IDLibro]) ON DELETE NO ACTION;
IF OBJECT_ID(N'dbo.FK_tblPresentacionLibro_tblLibro_IDLibro',N'F') IS NULL
ALTER TABLE [dbo].[tblPresentacionLibro] ADD CONSTRAINT [FK_tblPresentacionLibro_tblLibro_IDLibro] FOREIGN KEY([IDLibro]) REFERENCES [dbo].[tblLibro]([IDLibro]) ON DELETE NO ACTION;
IF OBJECT_ID(N'dbo.FK_tblDetalleSolicitudCotizacion_tblLibro_IDLibro',N'F') IS NULL
ALTER TABLE [dbo].[tblDetalleSolicitudCotizacion] ADD CONSTRAINT [FK_tblDetalleSolicitudCotizacion_tblLibro_IDLibro] FOREIGN KEY([IDLibro]) REFERENCES [dbo].[tblLibro]([IDLibro]) ON DELETE NO ACTION;
IF OBJECT_ID(N'dbo.FK_tblPresentacionLibro_tblFormatoLibro_IDFormato',N'F') IS NULL
ALTER TABLE [dbo].[tblPresentacionLibro] ADD CONSTRAINT [FK_tblPresentacionLibro_tblFormatoLibro_IDFormato] FOREIGN KEY([IDFormato]) REFERENCES [dbo].[tblFormatoLibro]([IDFormato]) ON DELETE NO ACTION;
IF OBJECT_ID(N'dbo.FK_tblDetalleCarrito_tblPresentacionLibro_IDPresentacion',N'F') IS NULL
ALTER TABLE [dbo].[tblDetalleCarrito] ADD CONSTRAINT [FK_tblDetalleCarrito_tblPresentacionLibro_IDPresentacion] FOREIGN KEY([IDPresentacion]) REFERENCES [dbo].[tblPresentacionLibro]([IDPresentacion]) ON DELETE NO ACTION;
IF OBJECT_ID(N'dbo.FK_tblDetallePedido_tblPresentacionLibro_IDPresentacion',N'F') IS NULL
ALTER TABLE [dbo].[tblDetallePedido] ADD CONSTRAINT [FK_tblDetallePedido_tblPresentacionLibro_IDPresentacion] FOREIGN KEY([IDPresentacion]) REFERENCES [dbo].[tblPresentacionLibro]([IDPresentacion]) ON DELETE NO ACTION;
IF OBJECT_ID(N'dbo.FK_tblDetalleCarrito_tblCarrito_IDCarrito',N'F') IS NULL
ALTER TABLE [dbo].[tblDetalleCarrito] ADD CONSTRAINT [FK_tblDetalleCarrito_tblCarrito_IDCarrito] FOREIGN KEY([IDCarrito]) REFERENCES [dbo].[tblCarrito]([IDCarrito]) ON DELETE NO ACTION;
IF OBJECT_ID(N'dbo.FK_tblDetallePedido_tblPedido_IDPedido',N'F') IS NULL
ALTER TABLE [dbo].[tblDetallePedido] ADD CONSTRAINT [FK_tblDetallePedido_tblPedido_IDPedido] FOREIGN KEY([IDPedido]) REFERENCES [dbo].[tblPedido]([IDPedido]) ON DELETE NO ACTION;
IF OBJECT_ID(N'dbo.FK_tblDetalleSolicitudCotizacion_tblSolicitudCotizacion_IDSolicitud',N'F') IS NULL
ALTER TABLE [dbo].[tblDetalleSolicitudCotizacion] ADD CONSTRAINT [FK_tblDetalleSolicitudCotizacion_tblSolicitudCotizacion_IDSolicitud] FOREIGN KEY([IDSolicitud]) REFERENCES [dbo].[tblSolicitudCotizacion]([IDSolicitud]) ON DELETE NO ACTION;
IF OBJECT_ID(N'dbo.FK_tblCotizacion_tblSolicitudCotizacion_IDSolicitud',N'F') IS NULL
ALTER TABLE [dbo].[tblCotizacion] ADD CONSTRAINT [FK_tblCotizacion_tblSolicitudCotizacion_IDSolicitud] FOREIGN KEY([IDSolicitud]) REFERENCES [dbo].[tblSolicitudCotizacion]([IDSolicitud]) ON DELETE NO ACTION;
IF OBJECT_ID(N'dbo.FK_tblDetalleCotizacion_tblDetalleSolicitudCotizacion_IDDetalleSolicitud',N'F') IS NULL
ALTER TABLE [dbo].[tblDetalleCotizacion] ADD CONSTRAINT [FK_tblDetalleCotizacion_tblDetalleSolicitudCotizacion_IDDetalleSolicitud] FOREIGN KEY([IDDetalleSolicitud]) REFERENCES [dbo].[tblDetalleSolicitudCotizacion]([IDDetalleSolicitud]) ON DELETE NO ACTION;
IF OBJECT_ID(N'dbo.FK_tblDetalleCotizacion_tblCotizacion_IDCotizacion',N'F') IS NULL
ALTER TABLE [dbo].[tblDetalleCotizacion] ADD CONSTRAINT [FK_tblDetalleCotizacion_tblCotizacion_IDCotizacion] FOREIGN KEY([IDCotizacion]) REFERENCES [dbo].[tblCotizacion]([IDCotizacion]) ON DELETE NO ACTION;
COMMIT;
GO
