-- Autorizado por el usuario el 12/11/2026: IVA académico 12 %, precios sin IVA.
-- Solo inserta cuando la tabla está vacía. No modifica una configuración existente.
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF DB_NAME() <> N'EditorialDB' THROW 50001, 'Seleccione EditorialDB.', 1;
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
BEGIN TRY
    BEGIN TRANSACTION;
    IF NOT EXISTS(SELECT 1 FROM dbo.tblConfiguracionSistema WITH (UPDLOCK,HOLDLOCK))
        INSERT dbo.tblConfiguracionSistema(PorcentajeImpuesto) VALUES(12);
    IF (SELECT COUNT(*) FROM dbo.tblConfiguracionSistema)<>1
       OR EXISTS(SELECT 1 FROM dbo.tblConfiguracionSistema WHERE PorcentajeImpuesto<>12)
        THROW 50002, 'La configuracion existente requiere revision; no se ha sobrescrito.', 1;
    COMMIT;
    SELECT IDConfiguracion,PorcentajeImpuesto FROM dbo.tblConfiguracionSistema;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT>0 ROLLBACK;
    THROW;
END CATCH;
