CREATE DATABASE CopCR_Dev;
GO
------------------------------------------------------------
-- 1. Usuario y subtipos
------------------------------------------------------------
CREATE TABLE dbo.Usuario (
    UsuarioID INT IDENTITY PRIMARY KEY,
    CedulaIdentidad NVARCHAR(100) NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    PrimerApellido NVARCHAR(100) NOT NULL,
    SegundoApellido NVARCHAR(100) NOT NULL,
    Email NVARCHAR(200) NOT NULL UNIQUE,
    NombreUsuario NVARCHAR(100) NOT NULL,
    Contrasena NVARCHAR(200) NOT NULL,  -- Hash BCrypt
    FechaRegistro DATETIME NOT NULL DEFAULT(GETDATE()),
    FotoPerfilUrl    NVARCHAR(500) NULL,
    Activo BIT NOT NULL DEFAULT(1)
);
GO

CREATE TABLE dbo.Administrador (
    UsuarioID INT PRIMARY KEY,
    TelefonoInterno NVARCHAR(20) NULL,
    CONSTRAINT FK_Admin_Usuario
      FOREIGN KEY (UsuarioID) REFERENCES dbo.Usuario(UsuarioID) ON DELETE CASCADE
);
GO

CREATE TABLE dbo.UsuarioFinal (
    UsuarioID INT PRIMARY KEY,
    FechaNacimiento DATE NULL,
    TelefonoContacto NVARCHAR(20) NULL,
    PuntosConfianza INT NOT NULL DEFAULT(0),
    AceptaNotificacionesPush BIT NOT NULL DEFAULT(1),
    CONSTRAINT FK_UsuarioFinal_Usuario
      FOREIGN KEY (UsuarioID) REFERENCES dbo.Usuario(UsuarioID) ON DELETE CASCADE
);
GO

------------------------------------------------------------
-- 2. Direcciones
------------------------------------------------------------
CREATE TABLE dbo.Direccion (
    DireccionID INT IDENTITY PRIMARY KEY,
    UsuarioID INT NOT NULL,
    Alias NVARCHAR(50) NULL,
    TextoLibre NVARCHAR(255) NULL,
    Latitud DECIMAL(9,6) NOT NULL,
    Longitud DECIMAL(9,6) NOT NULL,
    GeoLocation AS geography::Point(Latitud, Longitud, 4326) PERSISTED,
    IsDomicilioPrincipal BIT NOT NULL DEFAULT(0),
    CONSTRAINT FK_Direccion_Usuario
      FOREIGN KEY (UsuarioID) REFERENCES dbo.Usuario(UsuarioID) ON DELETE CASCADE
);
GO

CREATE SPATIAL INDEX IX_Direccion_Geo
    ON dbo.Direccion(GeoLocation);
GO

------------------------------------------------------------
-- 3. Catálogo de categorías de incidente
------------------------------------------------------------
CREATE TABLE dbo.CategoriaIncidente (
    CategoriaIncidenteID TINYINT IDENTITY PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(255) NULL
);
GO

-- Insertar algunas categorías por defecto
INSERT INTO dbo.CategoriaIncidente (Nombre, Descripcion) VALUES
('Robo', 'Informe de robos o hurtos en curso o anteriores'),
('Vandalismo','Daños a la propiedad pública o privada'),
('Accidente de Tráncito','Choques, colisiones o atropellos en vías públicas'),
('Agresión','Incidentes de violencia física o amenazas'),
('Incendio','Fuegos o conatos de incendio en edificaciones o zonas verdes');
GO

------------------------------------------------------------
-- 4. Catálogo de estados de incidente
------------------------------------------------------------
CREATE TABLE dbo.Estado (
    EstadoId      TINYINT      IDENTITY PRIMARY KEY,
    NombreEstado  NVARCHAR(100) NOT NULL,
    Descripcion   NVARCHAR(255) NULL
);
GO

-- Insertar algunos estados por defecto
INSERT INTO dbo.Estado (NombreEstado, Descripcion) VALUES
('Pendiente', 'Reporte ingresado, aún no atendido'),
('EnProceso', 'Incidente en proceso de atención'),
('Cerrado', 'Incidente finalizado');
GO

------------------------------------------------------------
-- 5. Incidentes
------------------------------------------------------------
CREATE TABLE dbo.Incidente (
    IncidenteID INT IDENTITY PRIMARY KEY,
    UsuarioID INT NOT NULL,
    DireccionID INT NULL,
    Latitud DECIMAL(9,6) NOT NULL,
    Longitud DECIMAL(9,6) NOT NULL,
    GeoLocation AS geography::Point(Latitud, Longitud, 4326) PERSISTED,
    Titulo NVARCHAR(200) NOT NULL,
    Descripcion NVARCHAR(MAX) NOT NULL,
    FechaReporte DATETIME NOT NULL DEFAULT(GETDATE()),
    EstadoId TINYINT NOT NULL DEFAULT(1),
    CategoriaIncidenteID TINYINT NOT NULL,
    AdministradorID INT NULL,
    FechaResolucion DATETIME NULL,
    CONSTRAINT FK_Incidente_UsuarioFinal
      FOREIGN KEY (UsuarioID) REFERENCES dbo.UsuarioFinal(UsuarioID),
    CONSTRAINT FK_Incidente_Direccion
      FOREIGN KEY (DireccionID) REFERENCES dbo.Direccion(DireccionID),
    CONSTRAINT FK_Incidente_Categoria
      FOREIGN KEY (CategoriaIncidenteID) REFERENCES dbo.CategoriaIncidente(CategoriaIncidenteID),
    CONSTRAINT FK_Incidente_Administrador
      FOREIGN KEY (AdministradorID) REFERENCES dbo.Administrador(UsuarioID),
    CONSTRAINT FK_Incidente_Estado
      FOREIGN KEY (EstadoId) REFERENCES dbo.Estado(EstadoId)
);
GO

CREATE SPATIAL INDEX IX_Incidente_GeoLocation
    ON dbo.Incidente(GeoLocation);
GO

------------------------------------------------------------
-- 6. Adjuntos de incidente
------------------------------------------------------------
CREATE TABLE dbo.IncidenteAdjunto (
    AdjuntoID INT IDENTITY PRIMARY KEY,
    IncidenteID INT NOT NULL,
    Url NVARCHAR(500) NOT NULL,
    FechaSubida DATETIME NOT NULL DEFAULT(GETDATE()),
    UsuarioID INT NOT NULL,
    CONSTRAINT FK_Adjunto_Incidente
      FOREIGN KEY (IncidenteID) REFERENCES dbo.Incidente(IncidenteID) ON DELETE CASCADE,
    CONSTRAINT FK_Adjunto_Usuario
      FOREIGN KEY (UsuarioID) REFERENCES dbo.Usuario(UsuarioID)
);
GO

------------------------------------------------------------
-- 7. Notificaciones de estado
------------------------------------------------------------
CREATE TABLE dbo.Notificacion (
    NotificacionID INT IDENTITY PRIMARY KEY,
    UsuarioID INT NOT NULL,
    IncidenteID INT NOT NULL,
    Mensaje NVARCHAR(255) NOT NULL,
    IsLeido BIT NOT NULL DEFAULT(0),
    Fecha DATETIME NOT NULL DEFAULT(GETDATE()),
    CONSTRAINT FK_Notificacion_Usuario
      FOREIGN KEY (UsuarioID) REFERENCES dbo.Usuario(UsuarioID),
    CONSTRAINT FK_Notificacion_Incidente
      FOREIGN KEY (IncidenteID) REFERENCES dbo.Incidente(IncidenteID) ON DELETE CASCADE
);
GO
