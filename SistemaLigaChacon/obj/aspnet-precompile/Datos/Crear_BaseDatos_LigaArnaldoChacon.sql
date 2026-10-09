/*
    Base de datos inicial para Liga Arnaldo Chacon Soto.
    Compatible con SQL Server / SQL Server Express.
    Ejecutar completo desde SQL Server Management Studio (SSMS).
    El script conserva datos existentes: crea tablas solo si no existen.
*/

IF DB_ID(N'LigaArnaldoChacon') IS NULL
BEGIN
    CREATE DATABASE [LigaArnaldoChacon];
END
GO

USE [LigaArnaldoChacon];
GO

IF OBJECT_ID(N'dbo.Temporadas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Temporadas
    (
        TemporadaId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Temporadas PRIMARY KEY,
        Nombre NVARCHAR(40) NOT NULL,
        FechaInicio DATE NULL,
        FechaFin DATE NULL,
        Estado VARCHAR(12) NOT NULL CONSTRAINT DF_Temporadas_Estado DEFAULT ('PREVIA'),
        CreadaEn DATETIME2(0) NOT NULL CONSTRAINT DF_Temporadas_CreadaEn DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_Temporadas_Nombre UNIQUE (Nombre),
        CONSTRAINT CK_Temporadas_Estado CHECK (Estado IN ('PREVIA', 'EN_CURSO', 'FINALIZADA')),
        CONSTRAINT CK_Temporadas_Fechas CHECK (FechaFin IS NULL OR FechaInicio IS NULL OR FechaFin >= FechaInicio)
    );
END
GO

IF OBJECT_ID(N'dbo.Categorias', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categorias
    (
        CategoriaId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Categorias PRIMARY KEY,
        Nombre NVARCHAR(30) NOT NULL,
        CONSTRAINT UQ_Categorias_Nombre UNIQUE (Nombre)
    );
END
GO

IF OBJECT_ID(N'dbo.Equipos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Equipos
    (
        EquipoId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Equipos PRIMARY KEY,
        Codigo VARCHAR(10) NOT NULL,
        Nombre NVARCHAR(120) NOT NULL,
        Representante NVARCHAR(120) NULL,
        Telefono NVARCHAR(30) NULL,
        LogoUrl NVARCHAR(260) NULL,
        Estado VARCHAR(10) NOT NULL CONSTRAINT DF_Equipos_Estado DEFAULT ('ACTIVO'),
        CreadoEn DATETIME2(0) NOT NULL CONSTRAINT DF_Equipos_CreadoEn DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_Equipos_Codigo UNIQUE (Codigo),
        CONSTRAINT UQ_Equipos_Nombre UNIQUE (Nombre),
        CONSTRAINT CK_Equipos_Estado CHECK (Estado IN ('ACTIVO', 'INACTIVO'))
    );
END
GO

IF OBJECT_ID(N'dbo.Competencias', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Competencias
    (
        CompetenciaId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Competencias PRIMARY KEY,
        TemporadaId INT NOT NULL,
        CategoriaId INT NOT NULL,
        Estado VARCHAR(12) NOT NULL CONSTRAINT DF_Competencias_Estado DEFAULT ('PREVIA'),
        LimiteEdicionesEquipo SMALLINT NOT NULL CONSTRAINT DF_Competencias_LimiteEdiciones DEFAULT (3),
        BloqueoPlantillaEn DATETIME2(0) NULL,
        CONSTRAINT UQ_Competencias_TemporadaCategoria UNIQUE (TemporadaId, CategoriaId),
        CONSTRAINT UQ_Competencias_IdTemporada UNIQUE (CompetenciaId, TemporadaId),
        CONSTRAINT FK_Competencias_Temporadas FOREIGN KEY (TemporadaId) REFERENCES dbo.Temporadas(TemporadaId),
        CONSTRAINT FK_Competencias_Categorias FOREIGN KEY (CategoriaId) REFERENCES dbo.Categorias(CategoriaId),
        CONSTRAINT CK_Competencias_Estado CHECK (Estado IN ('PREVIA', 'EN_CURSO', 'FINALIZADA')),
        CONSTRAINT CK_Competencias_LimiteEdiciones CHECK (LimiteEdicionesEquipo >= 0)
    );
END
GO

IF OBJECT_ID(N'dbo.EquipoInscripciones', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EquipoInscripciones
    (
        InscripcionEquipoId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EquipoInscripciones PRIMARY KEY,
        CompetenciaId INT NOT NULL,
        EquipoId INT NOT NULL,
        Estado VARCHAR(10) NOT NULL CONSTRAINT DF_EquipoInscripciones_Estado DEFAULT ('ACTIVO'),
        EdicionesUsadas SMALLINT NOT NULL CONSTRAINT DF_EquipoInscripciones_Ediciones DEFAULT (0),
        InscritoEn DATETIME2(0) NOT NULL CONSTRAINT DF_EquipoInscripciones_Fecha DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_EquipoInscripciones_CompetenciaEquipo UNIQUE (CompetenciaId, EquipoId),
        CONSTRAINT UQ_EquipoInscripciones_IdCompetencia UNIQUE (InscripcionEquipoId, CompetenciaId),
        CONSTRAINT FK_EquipoInscripciones_Competencias FOREIGN KEY (CompetenciaId) REFERENCES dbo.Competencias(CompetenciaId),
        CONSTRAINT FK_EquipoInscripciones_Equipos FOREIGN KEY (EquipoId) REFERENCES dbo.Equipos(EquipoId),
        CONSTRAINT CK_EquipoInscripciones_Estado CHECK (Estado IN ('ACTIVO', 'INACTIVO')),
        CONSTRAINT CK_EquipoInscripciones_Ediciones CHECK (EdicionesUsadas >= 0)
    );
END
GO

IF OBJECT_ID(N'dbo.Jugadores', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Jugadores
    (
        JugadorId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Jugadores PRIMARY KEY,
        Nombres NVARCHAR(80) NOT NULL,
        Apellidos NVARCHAR(100) NOT NULL,
        FechaNacimiento DATE NULL,
        Telefono NVARCHAR(30) NULL,
        Estado VARCHAR(10) NOT NULL CONSTRAINT DF_Jugadores_Estado DEFAULT ('ACTIVO'),
        CreadoEn DATETIME2(0) NOT NULL CONSTRAINT DF_Jugadores_CreadoEn DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT CK_Jugadores_Estado CHECK (Estado IN ('ACTIVO', 'INACTIVO'))
    );
END
GO

IF OBJECT_ID(N'dbo.InscripcionesJugadores', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.InscripcionesJugadores
    (
        InscripcionJugadorId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InscripcionesJugadores PRIMARY KEY,
        InscripcionEquipoId INT NOT NULL,
        JugadorId INT NOT NULL,
        Dorsal SMALLINT NULL,
        Estado VARCHAR(10) NOT NULL CONSTRAINT DF_InscripcionesJugadores_Estado DEFAULT ('ACTIVO'),
        CONSTRAINT UQ_InscripcionesJugadores_EquipoJugador UNIQUE (InscripcionEquipoId, JugadorId),
        CONSTRAINT UQ_InscripcionesJugadores_IdEquipo UNIQUE (InscripcionJugadorId, InscripcionEquipoId),
        CONSTRAINT FK_InscripcionesJugadores_Equipos FOREIGN KEY (InscripcionEquipoId) REFERENCES dbo.EquipoInscripciones(InscripcionEquipoId),
        CONSTRAINT FK_InscripcionesJugadores_Jugadores FOREIGN KEY (JugadorId) REFERENCES dbo.Jugadores(JugadorId),
        CONSTRAINT CK_InscripcionesJugadores_Dorsal CHECK (Dorsal IS NULL OR Dorsal BETWEEN 1 AND 99),
        CONSTRAINT CK_InscripcionesJugadores_Estado CHECK (Estado IN ('ACTIVO', 'INACTIVO'))
    );
END
GO

IF OBJECT_ID(N'dbo.Jornadas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Jornadas
    (
        JornadaId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Jornadas PRIMARY KEY,
        CompetenciaId INT NOT NULL,
        Numero SMALLINT NOT NULL,
        Fecha DATE NULL,
        Estado VARCHAR(12) NOT NULL CONSTRAINT DF_Jornadas_Estado DEFAULT ('PENDIENTE'),
        CONSTRAINT UQ_Jornadas_CompetenciaNumero UNIQUE (CompetenciaId, Numero),
        CONSTRAINT UQ_Jornadas_IdCompetencia UNIQUE (JornadaId, CompetenciaId),
        CONSTRAINT FK_Jornadas_Competencias FOREIGN KEY (CompetenciaId) REFERENCES dbo.Competencias(CompetenciaId),
        CONSTRAINT CK_Jornadas_Numero CHECK (Numero BETWEEN 1 AND 9),
        CONSTRAINT CK_Jornadas_Estado CHECK (Estado IN ('PENDIENTE', 'EN_CURSO', 'FINALIZADA'))
    );
END
GO

IF OBJECT_ID(N'dbo.Partidos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Partidos
    (
        PartidoId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Partidos PRIMARY KEY,
        CompetenciaId INT NOT NULL,
        JornadaId INT NOT NULL,
        EquipoLocalInscripcionId INT NOT NULL,
        EquipoVisitaInscripcionId INT NOT NULL,
        FechaHora DATETIME2(0) NULL,
        Cancha NVARCHAR(120) NULL,
        GolesLocal SMALLINT NULL,
        GolesVisita SMALLINT NULL,
        Estado VARCHAR(12) NOT NULL CONSTRAINT DF_Partidos_Estado DEFAULT ('PENDIENTE'),
        Observaciones NVARCHAR(1000) NULL,
        ActualizadoEn DATETIME2(0) NOT NULL CONSTRAINT DF_Partidos_Actualizado DEFAULT (SYSUTCDATETIME()),
        VersionFila ROWVERSION NOT NULL,
        CONSTRAINT UQ_Partidos_IdCompetencia UNIQUE (PartidoId, CompetenciaId),
        CONSTRAINT FK_Partidos_Jornadas FOREIGN KEY (JornadaId, CompetenciaId) REFERENCES dbo.Jornadas(JornadaId, CompetenciaId),
        CONSTRAINT FK_Partidos_EquipoLocal FOREIGN KEY (EquipoLocalInscripcionId, CompetenciaId) REFERENCES dbo.EquipoInscripciones(InscripcionEquipoId, CompetenciaId),
        CONSTRAINT FK_Partidos_EquipoVisita FOREIGN KEY (EquipoVisitaInscripcionId, CompetenciaId) REFERENCES dbo.EquipoInscripciones(InscripcionEquipoId, CompetenciaId),
        CONSTRAINT CK_Partidos_EquiposDistintos CHECK (EquipoLocalInscripcionId <> EquipoVisitaInscripcionId),
        CONSTRAINT CK_Partidos_Goles CHECK ((GolesLocal IS NULL OR GolesLocal >= 0) AND (GolesVisita IS NULL OR GolesVisita >= 0)),
        CONSTRAINT CK_Partidos_Estado CHECK (Estado IN ('PENDIENTE', 'EN_CURSO', 'FINALIZADO', 'CANCELADO')),
        CONSTRAINT CK_Partidos_MarcadorFinal CHECK (Estado <> 'FINALIZADO' OR (GolesLocal IS NOT NULL AND GolesVisita IS NOT NULL))
    );
END
GO

IF OBJECT_ID(N'dbo.PartidoAlineaciones', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PartidoAlineaciones
    (
        AlineacionId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PartidoAlineaciones PRIMARY KEY,
        PartidoId INT NOT NULL,
        InscripcionEquipoId INT NOT NULL,
        InscripcionJugadorId INT NOT NULL,
        Tipo VARCHAR(12) NOT NULL,
        Posicion NVARCHAR(30) NULL,
        EsPortero BIT NOT NULL CONSTRAINT DF_Alineaciones_EsPortero DEFAULT (0),
        MinutoEntrada SMALLINT NULL,
        MinutoSalida SMALLINT NULL,
        CONSTRAINT UQ_Alineaciones_PartidoJugador UNIQUE (PartidoId, InscripcionJugadorId),
        CONSTRAINT UQ_Alineaciones_IdPartidoEquipo UNIQUE (AlineacionId, PartidoId, InscripcionEquipoId),
        CONSTRAINT FK_Alineaciones_Partidos FOREIGN KEY (PartidoId) REFERENCES dbo.Partidos(PartidoId),
        CONSTRAINT FK_Alineaciones_JugadorEquipo FOREIGN KEY (InscripcionJugadorId, InscripcionEquipoId) REFERENCES dbo.InscripcionesJugadores(InscripcionJugadorId, InscripcionEquipoId),
        CONSTRAINT CK_Alineaciones_Tipo CHECK (Tipo IN ('TITULAR', 'SUPLENTE')),
        CONSTRAINT CK_Alineaciones_Minutos CHECK ((MinutoEntrada IS NULL OR MinutoEntrada BETWEEN 0 AND 120) AND (MinutoSalida IS NULL OR MinutoSalida BETWEEN 0 AND 120))
    );
END
GO

IF OBJECT_ID(N'dbo.Sustituciones', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Sustituciones
    (
        SustitucionId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Sustituciones PRIMARY KEY,
        PartidoId INT NOT NULL,
        InscripcionEquipoId INT NOT NULL,
        JugadorSaleId INT NOT NULL,
        JugadorEntraId INT NOT NULL,
        Minuto SMALLINT NULL,
        CONSTRAINT FK_Sustituciones_Partidos FOREIGN KEY (PartidoId) REFERENCES dbo.Partidos(PartidoId),
        CONSTRAINT FK_Sustituciones_Sale FOREIGN KEY (JugadorSaleId, InscripcionEquipoId) REFERENCES dbo.InscripcionesJugadores(InscripcionJugadorId, InscripcionEquipoId),
        CONSTRAINT FK_Sustituciones_Entra FOREIGN KEY (JugadorEntraId, InscripcionEquipoId) REFERENCES dbo.InscripcionesJugadores(InscripcionJugadorId, InscripcionEquipoId),
        CONSTRAINT CK_Sustituciones_Distintos CHECK (JugadorSaleId <> JugadorEntraId),
        CONSTRAINT CK_Sustituciones_Minuto CHECK (Minuto IS NULL OR Minuto BETWEEN 0 AND 120)
    );
END
GO

IF OBJECT_ID(N'dbo.EventosPartido', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EventosPartido
    (
        EventoId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EventosPartido PRIMARY KEY,
        PartidoId INT NOT NULL,
        InscripcionEquipoId INT NOT NULL,
        InscripcionJugadorId INT NOT NULL,
        Tipo VARCHAR(15) NOT NULL,
        Minuto SMALLINT NULL,
        Descripcion NVARCHAR(500) NULL,
        RegistradoEn DATETIME2(0) NOT NULL CONSTRAINT DF_EventosPartido_Fecha DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_EventosPartido_Partidos FOREIGN KEY (PartidoId) REFERENCES dbo.Partidos(PartidoId),
        CONSTRAINT FK_EventosPartido_JugadorEquipo FOREIGN KEY (InscripcionJugadorId, InscripcionEquipoId) REFERENCES dbo.InscripcionesJugadores(InscripcionJugadorId, InscripcionEquipoId),
        CONSTRAINT CK_EventosPartido_Tipo CHECK (Tipo IN ('GOL', 'AUTOGOL', 'AMARILLA', 'ROJA', 'LESION')),
        CONSTRAINT CK_EventosPartido_Minuto CHECK (Minuto IS NULL OR Minuto BETWEEN 0 AND 120)
    );
END
GO

IF OBJECT_ID(N'dbo.EstadisticasPorteroPartido', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EstadisticasPorteroPartido
    (
        EstadisticaPorteroId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EstadisticasPorteroPartido PRIMARY KEY,
        AlineacionId INT NOT NULL,
        PartidoId INT NOT NULL,
        InscripcionEquipoId INT NOT NULL,
        MinutosJugados SMALLINT NOT NULL,
        GolesRecibidos SMALLINT NOT NULL,
        RegistradoEn DATETIME2(0) NOT NULL CONSTRAINT DF_EstadisticasPortero_Fecha DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_EstadisticasPortero_Alineacion UNIQUE (AlineacionId),
        CONSTRAINT FK_EstadisticasPortero_Alineacion FOREIGN KEY (AlineacionId, PartidoId, InscripcionEquipoId) REFERENCES dbo.PartidoAlineaciones(AlineacionId, PartidoId, InscripcionEquipoId),
        CONSTRAINT CK_EstadisticasPortero_Minutos CHECK (MinutosJugados BETWEEN 0 AND 120),
        CONSTRAINT CK_EstadisticasPortero_Goles CHECK (GolesRecibidos >= 0)
    );
END
GO

IF OBJECT_ID(N'dbo.CheckIns', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CheckIns
    (
        CheckInId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CheckIns PRIMARY KEY,
        PartidoId INT NOT NULL,
        InscripcionJugadorId INT NOT NULL,
        MarcadoEn DATETIME2(0) NOT NULL CONSTRAINT DF_CheckIns_Marcado DEFAULT (SYSUTCDATETIME()),
        MarcadoPorUsuarioId INT NULL,
        CONSTRAINT UQ_CheckIns_PartidoJugador UNIQUE (PartidoId, InscripcionJugadorId),
        CONSTRAINT FK_CheckIns_Partidos FOREIGN KEY (PartidoId) REFERENCES dbo.Partidos(PartidoId),
        CONSTRAINT FK_CheckIns_Jugadores FOREIGN KEY (InscripcionJugadorId) REFERENCES dbo.InscripcionesJugadores(InscripcionJugadorId)
    );
END
GO

IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Usuarios
    (
        UsuarioId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Usuarios PRIMARY KEY,
        NombreUsuario NVARCHAR(60) NOT NULL,
        NombreMostrar NVARCHAR(120) NOT NULL,
        PasswordHash NVARCHAR(255) NOT NULL,
        Rol VARCHAR(15) NOT NULL,
        Activo BIT NOT NULL CONSTRAINT DF_Usuarios_Activo DEFAULT (1),
        CreadoEn DATETIME2(0) NOT NULL CONSTRAINT DF_Usuarios_CreadoEn DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_Usuarios_NombreUsuario UNIQUE (NombreUsuario),
        CONSTRAINT CK_Usuarios_Rol CHECK (Rol IN ('ADMINISTRADOR', 'USUARIO', 'JUGADOR'))
    );
END
GO

/*
    Roles actuales: SUPERADMINISTRADOR, ADMINISTRADOR y USUARIO.
    Se amplia la columna porque SUPERADMINISTRADOR supera 15 caracteres.
    Si aun no hay superadministrador, el administrador mas antiguo pasa a serlo.
*/
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

IF COL_LENGTH(N'dbo.CheckIns', N'MarcadoPorUsuarioId') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CheckIns_Usuarios')
BEGIN
    ALTER TABLE dbo.CheckIns ADD CONSTRAINT FK_CheckIns_Usuarios FOREIGN KEY (MarcadoPorUsuarioId) REFERENCES dbo.Usuarios(UsuarioId);
END
GO

IF OBJECT_ID(N'dbo.Campeones', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Campeones
    (
        CampeonId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Campeones PRIMARY KEY,
        CompetenciaId INT NOT NULL,
        InscripcionEquipoId INT NOT NULL,
        InscripcionPorteroPremiadoId INT NULL,
        ArchivadoEn DATETIME2(0) NOT NULL CONSTRAINT DF_Campeones_Archivado DEFAULT (SYSUTCDATETIME()),
        Observaciones NVARCHAR(500) NULL,
        CONSTRAINT UQ_Campeones_Competencia UNIQUE (CompetenciaId),
        CONSTRAINT FK_Campeones_Competencias FOREIGN KEY (CompetenciaId) REFERENCES dbo.Competencias(CompetenciaId),
        CONSTRAINT FK_Campeones_Equipo FOREIGN KEY (InscripcionEquipoId, CompetenciaId) REFERENCES dbo.EquipoInscripciones(InscripcionEquipoId, CompetenciaId),
        CONSTRAINT FK_Campeones_Portero FOREIGN KEY (InscripcionPorteroPremiadoId) REFERENCES dbo.InscripcionesJugadores(InscripcionJugadorId)
    );
END
GO

IF OBJECT_ID(N'dbo.PremiosTemporada', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PremiosTemporada
    (
        PremioId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PremiosTemporada PRIMARY KEY,
        CompetenciaId INT NOT NULL,
        Tipo VARCHAR(20) NOT NULL,
        InscripcionJugadorId INT NOT NULL,
        RegistradoEn DATETIME2(0) NOT NULL CONSTRAINT DF_Premios_Fecha DEFAULT (SYSUTCDATETIME()),
        Observaciones NVARCHAR(500) NULL,
        CONSTRAINT UQ_Premios_CompetenciaTipo UNIQUE (CompetenciaId, Tipo),
        CONSTRAINT FK_Premios_Competencias FOREIGN KEY (CompetenciaId) REFERENCES dbo.Competencias(CompetenciaId),
        CONSTRAINT FK_Premios_Jugador FOREIGN KEY (InscripcionJugadorId) REFERENCES dbo.InscripcionesJugadores(InscripcionJugadorId),
        CONSTRAINT CK_Premios_Tipo CHECK (Tipo IN ('GOLEADOR', 'MEJOR_PORTERO'))
    );
END
GO

IF OBJECT_ID(N'dbo.Tesoreria', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tesoreria
    (
        MovimientoId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Tesoreria PRIMARY KEY,
        CompetenciaId INT NOT NULL,
        Tipo VARCHAR(10) NOT NULL,
        Monto DECIMAL(12,2) NOT NULL,
        Fecha DATE NOT NULL,
        Concepto NVARCHAR(200) NOT NULL,
        RegistradoPor INT NULL,
        RegistradoEn DATETIME2(0) NOT NULL CONSTRAINT DF_Tesoreria_Creado DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_Tesoreria_Competencias FOREIGN KEY (CompetenciaId) REFERENCES dbo.Competencias(CompetenciaId),
        CONSTRAINT FK_Tesoreria_Usuarios FOREIGN KEY (RegistradoPor) REFERENCES dbo.Usuarios(UsuarioId),
        CONSTRAINT CK_Tesoreria_Tipo CHECK (Tipo IN ('INGRESO', 'GASTO')),
        CONSTRAINT CK_Tesoreria_Monto CHECK (Monto > 0)
    );
END
GO

IF OBJECT_ID(N'dbo.Auditoria', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Auditoria
    (
        AuditoriaId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Auditoria PRIMARY KEY,
        UsuarioId INT NULL,
        Entidad NVARCHAR(80) NOT NULL,
        EntidadId NVARCHAR(80) NOT NULL,
        Accion VARCHAR(20) NOT NULL,
        Cambios NVARCHAR(MAX) NULL,
        CreadoEn DATETIME2(0) NOT NULL CONSTRAINT DF_Auditoria_Creado DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_Auditoria_Usuarios FOREIGN KEY (UsuarioId) REFERENCES dbo.Usuarios(UsuarioId),
        CONSTRAINT CK_Auditoria_Accion CHECK (Accion IN ('INSERTAR', 'ACTUALIZAR', 'DESACTIVAR', 'FINALIZAR'))
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Categorias WHERE Nombre = N'Mayor')
    INSERT dbo.Categorias (Nombre) VALUES (N'Mayor');
IF NOT EXISTS (SELECT 1 FROM dbo.Categorias WHERE Nombre = N'Intermedia')
    INSERT dbo.Categorias (Nombre) VALUES (N'Intermedia');
GO

MERGE dbo.Equipos AS destino
USING (VALUES
    ('RDY', N'Real Deportivo Yojoa', N'~/Images/Logo RDY.jpeg'),
    ('RG', N'Club Deportivo Racing', N'~/Images/LogoRG.jpeg'),
    ('FCR', N'FC Reparto', N'~/Images/Logo FCR.jpeg'),
    ('MFC', N'Municipal FC', N'~/Images/Logo MFC.jpeg'),
    ('ASC', N'Atletico Santa Cruz', N'~/Images/Logo ASC.jpeg'),
    ('PSGC', N'Club Deportivo PSG', N'~/Images/Logo PSGC.jpeg'),
    ('ASA', N'Atletico San Antonio', N'~/Images/Logo ASA.jpeg'),
    ('CDI', N'Club Deportivo Independiente', N'~/Images/LogoCDI.jpeg'),
    ('CDEC', N'Real Deportivo El Cajon', N'~/Images/Logo CDEC.jpeg'),
    ('SPFC', N'Club Deportivo Sao Paulo', N'~/Images/LogoSPFC.jpeg')
) AS origen (Codigo, Nombre, LogoUrl)
ON destino.Codigo = origen.Codigo
WHEN MATCHED THEN UPDATE SET destino.Nombre = origen.Nombre, destino.LogoUrl = origen.LogoUrl
WHEN NOT MATCHED THEN INSERT (Codigo, Nombre, LogoUrl) VALUES (origen.Codigo, origen.Nombre, origen.LogoUrl);
GO

CREATE OR ALTER VIEW dbo.vw_TablaPosiciones
AS
    WITH Resultados AS
    (
        SELECT p.CompetenciaId, p.EquipoLocalInscripcionId AS InscripcionEquipoId,
               1 AS PJ,
               CASE WHEN p.GolesLocal > p.GolesVisita THEN 1 ELSE 0 END AS PG,
               CASE WHEN p.GolesLocal = p.GolesVisita THEN 1 ELSE 0 END AS PE,
               CASE WHEN p.GolesLocal < p.GolesVisita THEN 1 ELSE 0 END AS PP,
               p.GolesLocal AS GF, p.GolesVisita AS GC,
               CASE WHEN p.GolesLocal > p.GolesVisita THEN 3 WHEN p.GolesLocal = p.GolesVisita THEN 1 ELSE 0 END AS PTS
        FROM dbo.Partidos p
        WHERE p.Estado = 'FINALIZADO'

        UNION ALL

        SELECT p.CompetenciaId, p.EquipoVisitaInscripcionId,
               1,
               CASE WHEN p.GolesVisita > p.GolesLocal THEN 1 ELSE 0 END,
               CASE WHEN p.GolesVisita = p.GolesLocal THEN 1 ELSE 0 END,
               CASE WHEN p.GolesVisita < p.GolesLocal THEN 1 ELSE 0 END,
               p.GolesVisita, p.GolesLocal,
               CASE WHEN p.GolesVisita > p.GolesLocal THEN 3 WHEN p.GolesVisita = p.GolesLocal THEN 1 ELSE 0 END
        FROM dbo.Partidos p
        WHERE p.Estado = 'FINALIZADO'
    )
    SELECT c.CompetenciaId, t.TemporadaId, ca.Nombre AS Categoria,
           ei.InscripcionEquipoId, e.EquipoId, e.Codigo, e.Nombre AS Equipo, e.LogoUrl,
           COALESCE(SUM(r.PJ), 0) AS PJ,
           COALESCE(SUM(r.PG), 0) AS PG,
           COALESCE(SUM(r.PE), 0) AS PE,
           COALESCE(SUM(r.PP), 0) AS PP,
           COALESCE(SUM(r.GF), 0) AS GF,
           COALESCE(SUM(r.GC), 0) AS GC,
           COALESCE(SUM(r.GF - r.GC), 0) AS DG,
           COALESCE(SUM(r.PTS), 0) AS PTS
    FROM dbo.Competencias c
    INNER JOIN dbo.Temporadas t ON t.TemporadaId = c.TemporadaId
    INNER JOIN dbo.Categorias ca ON ca.CategoriaId = c.CategoriaId
    INNER JOIN dbo.EquipoInscripciones ei ON ei.CompetenciaId = c.CompetenciaId
    INNER JOIN dbo.Equipos e ON e.EquipoId = ei.EquipoId
    LEFT JOIN Resultados r ON r.CompetenciaId = c.CompetenciaId AND r.InscripcionEquipoId = ei.InscripcionEquipoId
    WHERE ei.Estado = 'ACTIVO'
    GROUP BY c.CompetenciaId, t.TemporadaId, ca.Nombre, ei.InscripcionEquipoId, e.EquipoId, e.Codigo, e.Nombre, e.LogoUrl;
GO

CREATE OR ALTER VIEW dbo.vw_Goleadores
AS
    SELECT c.CompetenciaId, c.TemporadaId, ca.Nombre AS Categoria,
           j.JugadorId, CONCAT(j.Nombres, N' ', j.Apellidos) AS Jugador,
           e.EquipoId, e.Nombre AS Equipo,
           COUNT_BIG(*) AS Goles
    FROM dbo.EventosPartido ev
    INNER JOIN dbo.Partidos p ON p.PartidoId = ev.PartidoId AND p.Estado = 'FINALIZADO'
    INNER JOIN dbo.Competencias c ON c.CompetenciaId = p.CompetenciaId
    INNER JOIN dbo.Categorias ca ON ca.CategoriaId = c.CategoriaId
    INNER JOIN dbo.InscripcionesJugadores ij ON ij.InscripcionJugadorId = ev.InscripcionJugadorId
    INNER JOIN dbo.Jugadores j ON j.JugadorId = ij.JugadorId
    INNER JOIN dbo.EquipoInscripciones ei ON ei.InscripcionEquipoId = ij.InscripcionEquipoId
    INNER JOIN dbo.Equipos e ON e.EquipoId = ei.EquipoId
    WHERE ev.Tipo = 'GOL'
    GROUP BY c.CompetenciaId, c.TemporadaId, ca.Nombre, j.JugadorId, j.Nombres, j.Apellidos, e.EquipoId, e.Nombre;
GO

CREATE OR ALTER VIEW dbo.vw_ClasificacionPorteros
AS
    SELECT c.CompetenciaId, c.TemporadaId, ca.Nombre AS Categoria,
           j.JugadorId, CONCAT(j.Nombres, N' ', j.Apellidos) AS Portero,
           e.EquipoId, e.Nombre AS Equipo,
           COUNT_BIG(*) AS PartidosComoPortero,
           SUM(CASE WHEN ep.GolesRecibidos = 0 THEN 1 ELSE 0 END) AS PorteriasACero,
           SUM(ep.GolesRecibidos) AS GolesRecibidos,
           SUM(ep.MinutosJugados) AS MinutosJugados
    FROM dbo.EstadisticasPorteroPartido ep
    INNER JOIN dbo.Partidos p ON p.PartidoId = ep.PartidoId AND p.Estado = 'FINALIZADO'
    INNER JOIN dbo.Competencias c ON c.CompetenciaId = p.CompetenciaId
    INNER JOIN dbo.Categorias ca ON ca.CategoriaId = c.CategoriaId
    INNER JOIN dbo.PartidoAlineaciones pa ON pa.AlineacionId = ep.AlineacionId
    INNER JOIN dbo.InscripcionesJugadores ij ON ij.InscripcionJugadorId = pa.InscripcionJugadorId
    INNER JOIN dbo.Jugadores j ON j.JugadorId = ij.JugadorId
    INNER JOIN dbo.EquipoInscripciones ei ON ei.InscripcionEquipoId = pa.InscripcionEquipoId
    INNER JOIN dbo.Equipos e ON e.EquipoId = ei.EquipoId
    WHERE pa.EsPortero = 1
    GROUP BY c.CompetenciaId, c.TemporadaId, ca.Nombre, j.JugadorId, j.Nombres, j.Apellidos, e.EquipoId, e.Nombre;
GO

/*
    Al terminar el torneo, crear un registro en dbo.Temporadas y una fila en
    dbo.Competencias por cada categoria. Inscribir clubes en dbo.EquipoInscripciones,
    generar 9 jornadas y agregar los 18 partidos de cada equipo en dbo.Partidos.
    Los clubes quedan cargados, pero no inscritos en una categoria porque esa
    distribucion debe confirmarse primero.
*/
