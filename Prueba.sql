USE [GAC_Desarrollo];
GO

SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @TipoInformeId uniqueidentifier = 'A3F6D650-341E-438A-BD53-5644D599EBDC';
DECLARE @InformeId uniqueidentifier = '0A83E5C0-AD4B-4345-B799-5ABFB6CB52AA';

IF NOT EXISTS (SELECT 1 FROM dbo.TiposInforme WHERE Id = @TipoInformeId)
BEGIN
    INSERT INTO dbo.TiposInforme (Id, Nombre)
    VALUES (@TipoInformeId, N'Investigación');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Articulos WHERE Id = @InformeId)
BEGIN
    INSERT INTO dbo.Articulos
        (Id, Titulo, TipoPublicacion, NumeroInforme, CentroPublicacion,
         MesPublicacion, AnioInforme, TipoInformeId)
    VALUES
        (@InformeId, N'Calidad del agua', 1, N'IT-DEMO-001', N'UNICDA',
         11, 2026, @TipoInformeId);
END;

COMMIT TRANSACTION;

SELECT Titulo, NumeroInforme, TipoPublicacion
FROM dbo.Articulos
WHERE Id = @InformeId;