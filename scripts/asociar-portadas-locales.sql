-- Asociaciones locales de respaldo; no contiene portadas editoriales autorizadas.
-- Inventario y condiciones: docs/REVISION-PORTADAS.md y docs/PORTADAS-LIBROS.csv.
-- Solo completa rutas vacias de los 20 libros. Conserva cualquier otra asociacion.
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF DB_NAME() <> N'EditorialDB' THROW 50010, 'Seleccione EditorialDB.', 1;
BEGIN TRY
    BEGIN TRANSACTION;
    DECLARE @Libros TABLE (ISBN nvarchar(20) PRIMARY KEY, Titulo nvarchar(200));
    INSERT @Libros VALUES
(N'9788497592208',N'Cien años de soledad'),
(N'9788445006771',N'Fahrenheit 451'),
(N'9788445006740',N'Crónicas marcianas'),
(N'9788445006801',N'El hombre ilustrado'),
(N'9788445006764',N'Soy leyenda'),
(N'9788445012802',N'El hobbit'),
(N'9788445013557',N'La Comunidad del Anillo'),
(N'9788445013564',N'Las Dos Torres'),
(N'9788445014493',N'El Silmarillion'),
(N'9788497933537',N'El fin de la eternidad'),
(N'9788420651361',N'La metamorfosis'),
(N'9788420654935',N'El retrato de Dorian Gray'),
(N'9788491819103',N'Drácula'),
(N'9788420651385',N'La Isla del Tesoro'),
(N'9788413623689',N'Frankenstein'),
(N'9791370093068',N'Orgullo y prejuicio'),
(N'9788420664286',N'Alicia en el País de las Maravillas'),
(N'9788413628448',N'Mujercitas'),
(N'9788420674629',N'Odisea'),
(N'9788420678344',N'Ilíada');
    IF (SELECT COUNT(*) FROM @Libros m
        JOIN dbo.tblPresentacionLibro p ON p.isbn = m.ISBN
        JOIN dbo.tblLibro l ON l.IDLibro = p.IDLibro AND l.Titulo = m.Titulo) <> 20
        THROW 50011, 'Faltan ISBN o no coinciden los titulos. Revise libros-reales.sql antes de asociar.', 1;

    UPDATE l SET portadaURL = N'/images/sin-portada.svg'
    FROM dbo.tblLibro l
    JOIN dbo.tblPresentacionLibro p ON p.IDLibro = l.IDLibro
    JOIN @Libros m ON m.ISBN = p.isbn AND m.Titulo = l.Titulo
    WHERE l.portadaURL IS NULL OR LTRIM(RTRIM(l.portadaURL)) = N'';
    SELECT @@ROWCOUNT AS RutasVaciasCompletadas;

    SELECT l.IDLibro, l.Titulo, p.isbn, l.portadaURL,
        CASE WHEN l.portadaURL = N'/images/sin-portada.svg'
             THEN N'Predeterminada local; portada editorial pendiente'
             ELSE N'Asociacion previa conservada; revisar archivo y permiso' END AS Estado
    FROM @Libros m
    JOIN dbo.tblPresentacionLibro p ON p.isbn = m.ISBN
    JOIN dbo.tblLibro l ON l.IDLibro = p.IDLibro
    ORDER BY l.Titulo;
    COMMIT;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;
    THROW;
END CATCH;
