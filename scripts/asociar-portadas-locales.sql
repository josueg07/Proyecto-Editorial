-- Portadas aportadas por el usuario en wwwroot/images/portadas; no descarga imagenes.
-- Ejecutar despues de libros-reales.sql en una instalacion nueva.
-- Solo modifica tblLibro.portadaURL para estos 20 ISBN; se puede repetir.
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF DB_NAME() <> N'EditorialDB' THROW 50010, 'Seleccione EditorialDB.', 1;
BEGIN TRY
    BEGIN TRANSACTION;
    DECLARE @Libros TABLE (ISBN nvarchar(20) PRIMARY KEY, Titulo nvarchar(200), Ruta nvarchar(500));
    INSERT @Libros VALUES
(N'9788497592208',N'Cien años de soledad',N'/images/portadas/9788497592208.jpg'),
(N'9788445006771',N'Fahrenheit 451',N'/images/portadas/9788445006771.jpg'),
(N'9788445006740',N'Crónicas marcianas',N'/images/portadas/9788445006740.jpg'),
(N'9788445006801',N'El hombre ilustrado',N'/images/portadas/9788445006801.jpg'),
(N'9788445006764',N'Soy leyenda',N'/images/portadas/9788445006764.jpg'),
(N'9788445012802',N'El hobbit',N'/images/portadas/9788445012802.jpg'),
(N'9788445013557',N'La Comunidad del Anillo',N'/images/portadas/9788445013557.jpg'),
(N'9788445013564',N'Las Dos Torres',N'/images/portadas/9788445013564.jpg'),
(N'9788445014493',N'El Silmarillion',N'/images/portadas/9788445014493.jpg'),
(N'9788497933537',N'El fin de la eternidad',N'/images/portadas/9788497933537.jpg'),
(N'9788420651361',N'La metamorfosis',N'/images/portadas/9788420651361.jpg'),
(N'9788420654935',N'El retrato de Dorian Gray',N'/images/portadas/9788420654935.jpg'),
(N'9788491819103',N'Drácula',N'/images/portadas/9788491819103.jpg'),
(N'9788420651385',N'La Isla del Tesoro',N'/images/portadas/9788420651385.jpg'),
(N'9788413623689',N'Frankenstein',N'/images/portadas/9788413623689.jpg'),
(N'9791370093068',N'Orgullo y prejuicio',N'/images/portadas/9791370093068.jpg'),
(N'9788420664286',N'Alicia en el País de las Maravillas',N'/images/portadas/9788420664286.jpg'),
(N'9788413628448',N'Mujercitas',N'/images/portadas/9788413628448.jpg'),
(N'9788420674629',N'Odisea',N'/images/portadas/9788420674629.jpg'),
(N'9788420678344',N'Ilíada',N'/images/portadas/9788420678344.jpg');
    IF (SELECT COUNT(*) FROM @Libros m
        JOIN dbo.tblPresentacionLibro p ON p.isbn = m.ISBN
        JOIN dbo.tblLibro l ON l.IDLibro = p.IDLibro AND l.Titulo = m.Titulo) <> 20
        THROW 50011, 'Faltan ISBN o no coinciden los titulos. Revise libros-reales.sql antes de asociar.', 1;

    UPDATE l SET portadaURL = m.Ruta
    FROM dbo.tblLibro l
    JOIN dbo.tblPresentacionLibro p ON p.IDLibro = l.IDLibro
    JOIN @Libros m ON m.ISBN = p.isbn AND m.Titulo = l.Titulo
    WHERE ISNULL(l.portadaURL,N'') <> m.Ruta;
    SELECT @@ROWCOUNT AS PortadasActualizadas;

    SELECT l.IDLibro,l.Titulo,p.isbn,l.portadaURL
    FROM @Libros m
    JOIN dbo.tblPresentacionLibro p ON p.isbn=m.ISBN
    JOIN dbo.tblLibro l ON l.IDLibro=p.IDLibro
    ORDER BY l.Titulo;
    COMMIT;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;
    THROW;
END CATCH;
