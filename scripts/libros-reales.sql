-- Fuentes bibliográficas: docs/FUENTES-LIBROS.csv. Verificadas el 13/11/2026.
-- Precios en quetzales e inventario FICTICIOS para el proyecto, siempre antes de IVA.
-- INSERT de datos faltantes y UPDATE exclusivamente de portadaURL por ISBN.
-- No repone stock al repetir ni sobrescribe precios, titulos u otros datos.
-- Portadas locales aportadas por el usuario: docs/PORTADAS-LIBROS.csv.
-- Incluir wwwroot/images/portadas completo en la entrega; no se descargan imagenes.
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF DB_NAME()<>N'EditorialDB' THROW 50001,'Seleccione EditorialDB.',1;
BEGIN TRY
BEGIN TRANSACTION;
DECLARE @Libros TABLE(Titulo nvarchar(200),Autor nvarchar(150),Editorial nvarchar(150),Anio smallint,ISBN nvarchar(20),Categoria nvarchar(100),Sinopsis nvarchar(max),Precio decimal(10,2),Portada nvarchar(500));
INSERT @Libros VALUES
(N'Cien años de soledad',N'Gabriel García Márquez',N'Debolsillo',2003,N'9788497592208',N'Narrativa latinoamericana',N'La historia de la familia Buendía recorre varias generaciones en Macondo y entrelaza memoria, soledad y transformaciones de la comunidad.',90.00,N'/images/portadas/9788497592208.jpg'),
(N'Fahrenheit 451',N'Ray Bradbury',N'Minotauro',2019,N'9788445006771',N'Ciencia ficción',N'En una sociedad que destruye los libros, un bombero empieza a cuestionar su trabajo y el lugar que ocupan la lectura y el pensamiento.',95.00,N'/images/portadas/9788445006771.jpg'),
(N'Crónicas marcianas',N'Ray Bradbury',N'Minotauro',2019,N'9788445006740',N'Ciencia ficción',N'Relatos sobre la llegada humana a Marte exploran el encuentro con otras culturas, la nostalgia y los conflictos trasladados desde la Tierra.',100.00,N'/images/portadas/9788445006740.jpg'),
(N'El hombre ilustrado',N'Ray Bradbury',N'Minotauro',2020,N'9788445006801',N'Ciencia ficción',N'Los tatuajes de un viajero sirven de marco a historias sobre futuros posibles y las contradicciones de la conducta humana.',105.00,N'/images/portadas/9788445006801.jpg'),
(N'Soy leyenda',N'Richard Matheson',N'Minotauro',2020,N'9788445006764',N'Terror',N'Un superviviente intenta comprender la epidemia que ha transformado a la población, mientras su aislamiento pone en duda sus certezas.',110.00,N'/images/portadas/9788445006764.jpg'),
(N'El hobbit',N'J. R. R. Tolkien',N'Minotauro',2022,N'9788445012802',N'Fantasía',N'Bilbo Bolsón abandona su hogar para acompañar a un grupo de enanos en una expedición que lo enfrenta a peligros y decisiones inesperadas.',115.00,N'/images/portadas/9788445012802.jpg'),
(N'La Comunidad del Anillo',N'J. R. R. Tolkien',N'Minotauro',2023,N'9788445013557',N'Fantasía',N'Frodo emprende un viaje para alejar de la Comarca un anillo peligroso y forma una comunidad con compañeros de distintos pueblos.',120.00,N'/images/portadas/9788445013557.jpg'),
(N'Las Dos Torres',N'J. R. R. Tolkien',N'Minotauro',2023,N'9788445013564',N'Fantasía',N'La comunidad se separa y sus integrantes continúan sus caminos mientras la guerra y las amenazas de Mordor se extienden por la Tierra Media.',125.00,N'/images/portadas/9788445013564.jpg'),
(N'El Silmarillion',N'J. R. R. Tolkien',N'Minotauro',2023,N'9788445014493',N'Fantasía',N'Un conjunto de relatos presenta los orígenes y las primeras edades de la Tierra Media, con sus pueblos, creaciones y luchas por el poder.',130.00,N'/images/portadas/9788445014493.jpg'),
(N'El fin de la eternidad',N'Isaac Asimov',N'Debolsillo',2004,N'9788497933537',N'Ciencia ficción',N'Una organización modifica la historia mediante viajes temporales. Uno de sus miembros descubre conflictos entre su misión y sus vínculos personales.',135.00,N'/images/portadas/9788497933537.jpg'),
(N'La metamorfosis',N'Franz Kafka',N'Alianza Editorial',2011,N'9788420651361',N'Narrativa clásica',N'La transformación de Gregor Samsa altera la vida familiar y revela tensiones alrededor del trabajo, la dependencia y el reconocimiento de los demás.',140.00,N'/images/portadas/9788420651361.jpg'),
(N'El retrato de Dorian Gray',N'Oscar Wilde',N'Alianza Editorial',2011,N'9788420654935',N'Narrativa clásica',N'Un joven conserva su apariencia mientras un retrato refleja las consecuencias de sus actos, en una reflexión sobre belleza y responsabilidad.',145.00,N'/images/portadas/9788420654935.jpg'),
(N'Drácula',N'Bram Stoker',N'Alianza Editorial',2020,N'9788491819103',N'Terror',N'Diarios y cartas reconstruyen la investigación de varias personas que intentan detener la amenaza del conde Drácula.',150.00,N'/images/portadas/9788491819103.jpg'),
(N'La Isla del Tesoro',N'Robert Louis Stevenson',N'Alianza Editorial',2011,N'9788420651385',N'Aventuras',N'Un mapa lleva a Jim Hawkins a una expedición marítima donde la búsqueda de un tesoro se cruza con la desconfianza y la ambición.',155.00,N'/images/portadas/9788420651385.jpg'),
(N'Frankenstein',N'Mary W. Shelley',N'Alianza Editorial',2021,N'9788413623689',N'Terror',N'El experimento de Victor Frankenstein produce una criatura cuya búsqueda de compañía y rechazo social enfrenta al creador con sus responsabilidades.',160.00,N'/images/portadas/9788413623689.jpg'),
(N'Orgullo y prejuicio',N'Jane Austen',N'Alianza Editorial',2026,N'9791370093068',N'Narrativa clásica',N'Elizabeth Bennet y Fitzwilliam Darcy deben revisar sus primeras impresiones en una sociedad marcada por las expectativas familiares y las diferencias económicas.',165.00,N'/images/portadas/9791370093068.jpg'),
(N'Alicia en el País de las Maravillas',N'Lewis Carroll',N'Alianza Editorial',2010,N'9788420664286',N'Literatura juvenil',N'Alicia recorre un mundo de transformaciones y conversaciones insólitas que juegan con el lenguaje, la lógica y las convenciones de la vida cotidiana.',170.00,N'/images/portadas/9788420664286.jpg'),
(N'Mujercitas',N'Louisa May Alcott',N'Alianza Editorial',2022,N'9788413628448',N'Narrativa clásica',N'Las hermanas March afrontan cambios personales y dificultades familiares mientras descubren sus intereses y buscan un lugar propio en la sociedad.',175.00,N'/images/portadas/9788413628448.jpg'),
(N'Odisea',N'Homero',N'Alianza Editorial',2013,N'9788420674629',N'Clásicos grecolatinos',N'El regreso de Odiseo a Ítaca enlaza viajes, pruebas y encuentros extraordinarios con la espera de su familia y la recuperación de su hogar.',180.00,N'/images/portadas/9788420674629.jpg'),
(N'Ilíada',N'Homero',N'Alianza Editorial',2013,N'9788420678344',N'Clásicos grecolatinos',N'El conflicto entre Aquiles y Agamenón guía un relato de la guerra de Troya centrado en el honor, la pérdida y las consecuencias de la violencia.',185.00,N'/images/portadas/9788420678344.jpg');
IF NOT EXISTS(SELECT 1 FROM tblFormatoLibro WHERE NombreFormato=N'Impreso') INSERT tblFormatoLibro(NombreFormato,EsDigital) VALUES(N'Impreso',0);
IF NOT EXISTS(SELECT 1 FROM tblFormatoLibro WHERE NombreFormato=N'Digital') INSERT tblFormatoLibro(NombreFormato,EsDigital) VALUES(N'Digital',1);
DECLARE @Impreso int=(SELECT IDFormato FROM tblFormatoLibro WHERE NombreFormato=N'Impreso' AND EsDigital=0);
DECLARE @Digital int=(SELECT IDFormato FROM tblFormatoLibro WHERE NombreFormato=N'Digital' AND EsDigital=1);
IF @Impreso IS NULL OR @Digital IS NULL THROW 50002,'Revise los formatos existentes; sus indicadores no corresponden.',1;
DECLARE @Titulo nvarchar(200),@Autor nvarchar(150),@Editorial nvarchar(150),@Anio smallint,@ISBN nvarchar(20),@Categoria nvarchar(100),@Sinopsis nvarchar(max),@Precio decimal(10,2),@Portada nvarchar(500);
DECLARE libros CURSOR LOCAL FAST_FORWARD FOR SELECT * FROM @Libros;
OPEN libros;
FETCH NEXT FROM libros INTO @Titulo,@Autor,@Editorial,@Anio,@ISBN,@Categoria,@Sinopsis,@Precio,@Portada;
WHILE @@FETCH_STATUS=0
BEGIN
    IF NOT EXISTS(SELECT 1 FROM tblAutor WHERE NombreAutor=@Autor) INSERT tblAutor(NombreAutor,Activo) VALUES(@Autor,1);
    IF NOT EXISTS(SELECT 1 FROM tblEditorial WHERE NombreEditorial=@Editorial) INSERT tblEditorial(NombreEditorial) VALUES(@Editorial);
    IF NOT EXISTS(SELECT 1 FROM tblCategoria WHERE NombreCategoria=@Categoria) INSERT tblCategoria(NombreCategoria,Activo) VALUES(@Categoria,1);
    DECLARE @A int=(SELECT TOP(1) IDAutor FROM tblAutor WHERE NombreAutor=@Autor ORDER BY IDAutor);
    DECLARE @E int=(SELECT IDEditorial FROM tblEditorial WHERE NombreEditorial=@Editorial);
    DECLARE @C int=(SELECT IDCategoria FROM tblCategoria WHERE NombreCategoria=@Categoria);
    DECLARE @L int=(SELECT IDLibro FROM tblPresentacionLibro WHERE isbn=@ISBN);
    IF @L IS NOT NULL AND NOT EXISTS(SELECT 1 FROM tblLibro WHERE IDLibro=@L AND Titulo=@Titulo AND IDEditorial=@E)
        THROW 50003,'Un ISBN ya pertenece a otro libro o editorial. Revise sin sobrescribir datos.',1;
    IF @L IS NULL SELECT TOP(1) @L=IDLibro FROM tblLibro WHERE Titulo=@Titulo AND IDEditorial=@E ORDER BY IDLibro;
    IF @L IS NULL
    BEGIN
        INSERT tblLibro(Titulo,Sinopsis,AnioEdicion,portadaURL,fechaRegistro,Destacado,Activo,IDEditorial)
        VALUES(@Titulo,@Sinopsis,@Anio,@Portada,SYSDATETIME(),CASE WHEN @ISBN IN(N'9788497592208',N'9788445006771',N'9788445012802') THEN 1 ELSE 0 END,1,@E);
        SET @L=CONVERT(int,SCOPE_IDENTITY());
    END;
    -- Asociacion explicita por ISBN validado; no modifica otros campos del libro.
    UPDATE tblLibro SET portadaURL=@Portada
    WHERE IDLibro=@L AND ISNULL(portadaURL,N'')<>@Portada;
    IF NOT EXISTS(SELECT 1 FROM tblLibro_Autor WHERE IDLibro=@L AND IDAutor=@A) INSERT tblLibro_Autor(IDLibro,IDAutor) VALUES(@L,@A);
    IF NOT EXISTS(SELECT 1 FROM tblLibro_Categoria WHERE IDLibro=@L AND IDCategoria=@C) INSERT tblLibro_Categoria(IDLibro,IDCategoria) VALUES(@L,@C);
    IF NOT EXISTS(SELECT 1 FROM tblPresentacionLibro WHERE isbn=@ISBN)
    BEGIN
        IF EXISTS(SELECT 1 FROM tblPresentacionLibro WHERE IDLibro=@L AND IDFormato=@Impreso) THROW 50004,'Ya hay otra edicion impresa para ese libro. Revise la restriccion libro/formato.',1;
        INSERT tblPresentacionLibro(isbn,PrecioBase,Existencias,IDLibro,IDFormato) VALUES(@ISBN,@Precio,120,@L,@Impreso);
    END;
    FETCH NEXT FROM libros INTO @Titulo,@Autor,@Editorial,@Anio,@ISBN,@Categoria,@Sinopsis,@Precio,@Portada;
END;
CLOSE libros; DEALLOCATE libros;
-- ISBN digitales independientes, publicados en las mismas fichas de Alianza.
DECLARE @Digitales TABLE(Papel nvarchar(20),Electronico nvarchar(20),Precio decimal(10,2));
INSERT @Digitales VALUES(N'9788420651361',N'9788413622552',65),(N'9788420654935',N'9788411480703',75),(N'9791370093068',N'9791370093396',85);
INSERT tblPresentacionLibro(isbn,PrecioBase,Existencias,IDLibro,IDFormato)
SELECT d.Electronico,d.Precio,NULL,p.IDLibro,@Digital FROM @Digitales d JOIN tblPresentacionLibro p ON p.isbn=d.Papel
WHERE NOT EXISTS(SELECT 1 FROM tblPresentacionLibro x WHERE x.isbn=d.Electronico OR x.IDLibro=p.IDLibro AND x.IDFormato=@Digital);
COMMIT;
SELECT COUNT(*) EdicionesImpresasVerificadas FROM tblPresentacionLibro p JOIN @Libros l ON l.ISBN=p.isbn;
-- Mostrar las asociaciones efectivas de las 20 portadas locales.
SELECT b.Titulo,p.isbn,b.portadaURL FROM tblLibro b
JOIN tblPresentacionLibro p ON p.IDLibro=b.IDLibro JOIN @Libros l ON l.ISBN=p.isbn
ORDER BY b.Titulo;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT>0 ROLLBACK;
    THROW;
END CATCH;
