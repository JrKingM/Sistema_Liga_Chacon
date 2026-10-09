/*
    Ejecutar una vez en SQL Server Management Studio para actualizar
    una base LigaArnaldoChacon que ya existia.
    Conserva las cuentas actuales y convierte la cuenta ADMINISTRADOR
    mas antigua en SUPERADMINISTRADOR si todavia no hay uno.
*/
USE [LigaArnaldoChacon];
GO

IF COL_LENGTH(N'dbo.Usuarios', N'Rol') IS NULL
    THROW 50020, 'No existe dbo.Usuarios. Ejecuta primero el script inicial de la liga.', 1;
GO

IF COL_LENGTH(N'dbo.Usuarios', N'Rol') < 25
    ALTER TABLE dbo.Usuarios ALTER COLUMN Rol VARCHAR(25) NOT NULL;
GO

IF EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE parent_object_id = OBJECT_ID(N'dbo.Usuarios')
      AND name = N'CK_Usuarios_Rol')
    ALTER TABLE dbo.Usuarios DROP CONSTRAINT CK_Usuarios_Rol;
GO

UPDATE dbo.Usuarios
SET Rol = 'USUARIO'
WHERE Rol = 'JUGADOR';

IF NOT EXISTS (
    SELECT 1
    FROM dbo.Usuarios
    WHERE Rol = 'SUPERADMINISTRADOR')
BEGIN
    DECLARE @PrimerAdmin INT;
    SELECT @PrimerAdmin = MIN(UsuarioId)
    FROM dbo.Usuarios
    WHERE Rol = 'ADMINISTRADOR';

    IF @PrimerAdmin IS NOT NULL
        UPDATE dbo.Usuarios
        SET Rol = 'SUPERADMINISTRADOR'
        WHERE UsuarioId = @PrimerAdmin;
END;
GO

ALTER TABLE dbo.Usuarios
ADD CONSTRAINT CK_Usuarios_Rol
CHECK (Rol IN ('SUPERADMINISTRADOR', 'ADMINISTRADOR', 'USUARIO'));
GO
