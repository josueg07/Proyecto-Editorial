-- OPCIONAL Y MANUAL. No se ejecutó durante la implementación.
-- Elegir una cuenta previamente registrada y confirmada. No concede rol institucional.
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF DB_NAME() <> N'EditorialDB' THROW 50001, 'Seleccione EditorialDB.', 1;
DECLARE @Correo nvarchar(256)=N'REEMPLAZAR_CON_CORREO_DE_LA_CUENTA';
IF @Correo=N'REEMPLAZAR_CON_CORREO_DE_LA_CUENTA'
    THROW 50002, 'Escriba el correo de la cuenta de demostracion elegida.', 1;
BEGIN TRY
    BEGIN TRANSACTION;
    DECLARE @Usuario nvarchar(450)=(SELECT Id FROM dbo.AspNetUsers WHERE NormalizedEmail=UPPER(@Correo) AND EmailConfirmed=1);
    DECLARE @Rol nvarchar(450)=(SELECT Id FROM dbo.AspNetRoles WHERE NormalizedName=N'ASESORVENTAS');
    IF @Usuario IS NULL OR @Rol IS NULL THROW 50003, 'No existe esa cuenta confirmada o el rol AsesorVentas.', 1;
    IF NOT EXISTS(SELECT 1 FROM dbo.AspNetUserRoles WHERE UserId=@Usuario AND RoleId=@Rol)
        INSERT dbo.AspNetUserRoles(UserId,RoleId) VALUES(@Usuario,@Rol);
    COMMIT;
    SELECT N'Asesor habilitado. Cierre sesion y vuelva a entrar para cargar el rol.' Resultado;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT>0 ROLLBACK;
    THROW;
END CATCH;
