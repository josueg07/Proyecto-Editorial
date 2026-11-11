-- Datos ficticios para demostrar el catálogo. Solo INSERT; no elimina ni actualiza.
-- Ejecutar exclusivamente en EditorialDB. Puede repetirse sin duplicar estos ejemplos.
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF DB_NAME() <> N'EditorialDB' THROW 50001, 'Seleccione EditorialDB.', 1;
BEGIN TRY
    BEGIN TRANSACTION;
    IF NOT EXISTS (SELECT 1 FROM dbo.tblEditorial WHERE NombreEditorial=N'Editorial Académica (demostración)')
        INSERT dbo.tblEditorial(NombreEditorial) VALUES(N'Editorial Académica (demostración)');
    DECLARE @editorial int=(SELECT TOP(1) IDEditorial FROM dbo.tblEditorial WHERE NombreEditorial=N'Editorial Académica (demostración)' ORDER BY IDEditorial);
    IF NOT EXISTS (SELECT 1 FROM dbo.tblAutor WHERE NombreAutor=N'Equipo académico (demostración)')
        INSERT dbo.tblAutor(NombreAutor,Activo) VALUES(N'Equipo académico (demostración)',1);
    DECLARE @autor int=(SELECT TOP(1) IDAutor FROM dbo.tblAutor WHERE NombreAutor=N'Equipo académico (demostración)' ORDER BY IDAutor);
    IF NOT EXISTS (SELECT 1 FROM dbo.tblCategoria WHERE NombreCategoria=N'Tecnología (demostración)')
        INSERT dbo.tblCategoria(NombreCategoria,Activo) VALUES(N'Tecnología (demostración)',1);
    IF NOT EXISTS (SELECT 1 FROM dbo.tblCategoria WHERE NombreCategoria=N'Educación (demostración)')
        INSERT dbo.tblCategoria(NombreCategoria,Activo) VALUES(N'Educación (demostración)',1);
    IF NOT EXISTS (SELECT 1 FROM dbo.tblFormatoLibro WHERE NombreFormato=N'Impreso (demostración)')
        INSERT dbo.tblFormatoLibro(NombreFormato,EsDigital) VALUES(N'Impreso (demostración)',0);
    IF NOT EXISTS (SELECT 1 FROM dbo.tblFormatoLibro WHERE NombreFormato=N'Digital (demostración)')
        INSERT dbo.tblFormatoLibro(NombreFormato,EsDigital) VALUES(N'Digital (demostración)',1);
    DECLARE @impreso int=(SELECT IDFormato FROM dbo.tblFormatoLibro WHERE NombreFormato=N'Impreso (demostración)');
    DECLARE @digital int=(SELECT IDFormato FROM dbo.tblFormatoLibro WHERE NombreFormato=N'Digital (demostración)');
    DECLARE @tecnologia int=(SELECT IDCategoria FROM dbo.tblCategoria WHERE NombreCategoria=N'Tecnología (demostración)');
    DECLARE @educacion int=(SELECT IDCategoria FROM dbo.tblCategoria WHERE NombreCategoria=N'Educación (demostración)');
    DECLARE @ejemplos TABLE(Titulo nvarchar(200),Sinopsis nvarchar(max),Categoria int,Destacado bit,IsbnImpreso nvarchar(20),IsbnDigital nvarchar(20),PrecioImpreso decimal(10,2),PrecioDigital decimal(10,2));
    INSERT @ejemplos VALUES
      (N'Arquitectura de sistemas editoriales (demostración)',N'Libro ficticio de demostración. Introducción a la organización de un sistema editorial en modelos, vistas y controladores.',@tecnologia,1,N'9780000000102',N'9780000000119',120,80),
      (N'Fundamentos de bases de datos (demostración)',N'Libro ficticio de demostración. Conceptos de tablas, claves, relaciones e integridad referencial en una base de datos relacional.',@tecnologia,0,N'9780000000126',N'9780000000133',150,95),
      (N'Aprendizaje y diseño de aplicaciones (demostración)',N'Libro ficticio de demostración. Ejemplos académicos para aprender a diseñar aplicaciones y explicar sus decisiones de arquitectura.',@educacion,1,N'9780000000140',N'9780000000157',90,60);
    INSERT dbo.tblLibro(Titulo,Sinopsis,AnioEdicion,portadaURL,fechaRegistro,Destacado,Activo,IDEditorial)
      SELECT e.Titulo,e.Sinopsis,2026,N'/images/sin-portada.svg',SYSDATETIME(),e.Destacado,1,@editorial FROM @ejemplos e
      WHERE NOT EXISTS(SELECT 1 FROM dbo.tblLibro l WHERE l.Titulo=e.Titulo AND l.IDEditorial=@editorial);
    INSERT dbo.tblLibro_Autor(IDLibro,IDAutor)
      SELECT l.IDLibro,@autor FROM @ejemplos e JOIN dbo.tblLibro l ON l.Titulo=e.Titulo AND l.IDEditorial=@editorial
      WHERE NOT EXISTS(SELECT 1 FROM dbo.tblLibro_Autor a WHERE a.IDLibro=l.IDLibro AND a.IDAutor=@autor);
    INSERT dbo.tblLibro_Categoria(IDLibro,IDCategoria)
      SELECT l.IDLibro,e.Categoria FROM @ejemplos e JOIN dbo.tblLibro l ON l.Titulo=e.Titulo AND l.IDEditorial=@editorial
      WHERE NOT EXISTS(SELECT 1 FROM dbo.tblLibro_Categoria c WHERE c.IDLibro=l.IDLibro AND c.IDCategoria=e.Categoria);
    INSERT dbo.tblPresentacionLibro(isbn,PrecioBase,Existencias,IDLibro,IDFormato)
      SELECT e.IsbnImpreso,e.PrecioImpreso,12,l.IDLibro,@impreso FROM @ejemplos e JOIN dbo.tblLibro l ON l.Titulo=e.Titulo AND l.IDEditorial=@editorial
      WHERE NOT EXISTS(SELECT 1 FROM dbo.tblPresentacionLibro p WHERE p.isbn=e.IsbnImpreso OR (p.IDLibro=l.IDLibro AND p.IDFormato=@impreso));
    INSERT dbo.tblPresentacionLibro(isbn,PrecioBase,Existencias,IDLibro,IDFormato)
      SELECT e.IsbnDigital,e.PrecioDigital,NULL,l.IDLibro,@digital FROM @ejemplos e JOIN dbo.tblLibro l ON l.Titulo=e.Titulo AND l.IDEditorial=@editorial
      WHERE NOT EXISTS(SELECT 1 FROM dbo.tblPresentacionLibro p WHERE p.isbn=e.IsbnDigital OR (p.IDLibro=l.IDLibro AND p.IDFormato=@digital));
    COMMIT TRANSACTION;
    SELECT N'Datos de demostración disponibles' Resultado;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT>0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
