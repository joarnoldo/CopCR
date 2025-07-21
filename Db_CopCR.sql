CREATE DATABASE CopCR_Dev;
GO

-- ##########################
-- 1. Catálogos geográficos
-- ##########################

CREATE TABLE Provincia (
  ProvinciaID   TINYINT      IDENTITY PRIMARY KEY,
  Nombre        NVARCHAR(50) NOT NULL UNIQUE
);
GO

CREATE TABLE Canton (
  CantonID      SMALLINT     IDENTITY PRIMARY KEY,
  ProvinciaID   TINYINT      NOT NULL,
  Nombre        NVARCHAR(50) NOT NULL,
  CONSTRAINT FK_Canton_Provincia FOREIGN KEY (ProvinciaID)
    REFERENCES Provincia(ProvinciaID),
  CONSTRAINT UQ_Canton UNIQUE (ProvinciaID, Nombre)
);
GO

CREATE TABLE Distrito (
  DistritoID    SMALLINT     IDENTITY PRIMARY KEY,
  CantonID      SMALLINT     NOT NULL,
  Nombre        NVARCHAR(50) NOT NULL,
  CONSTRAINT FK_Distrito_Canton FOREIGN KEY (CantonID)
    REFERENCES Canton(CantonID),
  CONSTRAINT UQ_Distrito UNIQUE (CantonID, Nombre)
);
GO

-- ##########################
-- 2.Usuario
-- ##########################

CREATE TABLE Usuario (
  UsuarioID     INT           IDENTITY PRIMARY KEY,
  CedulaIdentidad NVARCHAR(100) NOT NULL,
  Nombre        NVARCHAR(100) NOT NULL,
  PrimerApellido        NVARCHAR(100) NOT NULL,
  SegundoApellido       NVARCHAR(100) NOT NULL,
  Email         NVARCHAR(200) NOT NULL UNIQUE,
  NombreUsuario        NVARCHAR(100) NOT NULL,
  Contrasena    NVARCHAR(200) NOT NULL,      -- Hash
  FechaRegistro DATETIME      NOT NULL DEFAULT GETDATE(),
  FotoPerfilUrl             NVARCHAR(500)  NULL,
  Activo            BIT            NOT NULL DEFAULT 1
);
GO

-- ##########################
-- 3. Subtipo Administrador
-- ##########################

CREATE TABLE Administrador (
  UsuarioID         INT PRIMARY KEY,  
  TelefonoInterno   NVARCHAR(20)   NULL,

  CONSTRAINT FK_Admin_Usuario 
    FOREIGN KEY (UsuarioID) REFERENCES Usuario(UsuarioID) ON DELETE CASCADE
);
GO

-- ##########################
-- 4. Subtipo UsuarioFinal
-- ##########################

CREATE TABLE UsuarioFinal (
  UsuarioID             INT PRIMARY KEY,
  FechaNacimiento       DATE           NULL,
  TelefonoContacto      NVARCHAR(20)   NULL,
  PuntosConfianza       INT            NOT NULL DEFAULT 0,
  AceptaNotificacionesPush    BIT            NOT NULL DEFAULT 1,

  CONSTRAINT FK_UsuarioFinal_Usuario 
    FOREIGN KEY (UsuarioID) REFERENCES Usuario(UsuarioID) ON DELETE CASCADE
);
GO

-- ##########################
-- 5. Direcciones de usuario
-- ##########################

CREATE TABLE Direccion (
  DireccionID   INT           IDENTITY PRIMARY KEY,
  UsuarioID     INT           NOT NULL,
  DistritoID    SMALLINT      NOT NULL,
  Barrio        NVARCHAR(100) NULL,
  Linea         NVARCHAR(200) NULL,
  CodigoPostal  CHAR(5)       NULL,
  Latitud       DECIMAL(9,6)  NULL,
  Longitud      DECIMAL(9,6)  NULL,
  GeoLocation   AS (
    CASE 
      WHEN Latitud IS NOT NULL AND Longitud IS NOT NULL 
      THEN geography::Point(Latitud, Longitud, 4326) 
      ELSE NULL 
    END
  ) PERSISTED,
  CONSTRAINT FK_Direccion_Usuario FOREIGN KEY (UsuarioID)
    REFERENCES Usuario(UsuarioID) ON DELETE CASCADE,
  CONSTRAINT FK_Direccion_Distrito FOREIGN KEY (DistritoID)
    REFERENCES Distrito(DistritoID)
);
GO

-- Índice espacial para búsquedas por radio
CREATE SPATIAL INDEX IX_Direccion_GeoLocation 
  ON Direccion(GeoLocation);
GO

-- ##########################
-- 6. Catálogo de Categorías de Incidente
-- ##########################

CREATE TABLE CategoriaIncidente (
  CategoriaIncidenteID TINYINT       IDENTITY PRIMARY KEY,
  Nombre               NVARCHAR(100) NOT NULL,
  Descripcion          NVARCHAR(255) NULL
);
GO

-- ##########################
-- 7. Incidentes
-- ##########################

CREATE TABLE Incidente (
  IncidenteID          INT           IDENTITY PRIMARY KEY,
  UsuarioID            INT           NOT NULL,    -- debe ser UsuarioFinal
  DireccionID          INT           NULL,        -- domicilio guardado, opcional
  Latitud              DECIMAL(9,6)  NOT NULL,
  Longitud             DECIMAL(9,6)  NOT NULL,
  GeoLocation          AS geography::Point(Latitud, Longitud, 4326) PERSISTED,
  Titulo               NVARCHAR(200) NOT NULL,
  Descripcion          NVARCHAR(MAX) NOT NULL,
  FechaReporte         DATETIME      NOT NULL DEFAULT GETDATE(),
  Estado               TINYINT       NOT NULL DEFAULT 1,  -- 1=Pendiente,2=EnProceso,3=Cerrado
  CategoriaIncidenteID TINYINT       NOT NULL,
  AdministradorID      INT           NULL,               -- debe ser Administrador
  FechaResolucion      DATETIME      NULL,
  CONSTRAINT FK_Incidente_UsuarioFinal 
    FOREIGN KEY (UsuarioID)  
    REFERENCES UsuarioFinal(UsuarioID),
  CONSTRAINT FK_Incidente_Direccion 
    FOREIGN KEY (DireccionID)       
    REFERENCES Direccion(DireccionID),
  CONSTRAINT FK_Incidente_Categoria 
    FOREIGN KEY (CategoriaIncidenteID) 
    REFERENCES CategoriaIncidente(CategoriaIncidenteID),
  CONSTRAINT FK_Incidente_Administrador 
    FOREIGN KEY (AdministradorID)    
    REFERENCES Administrador(UsuarioID),
  CONSTRAINT CHK_Incidente_Estado CHECK (Estado IN (1,2,3))
);
GO

-- Índice espacial para búsquedas por radio
CREATE SPATIAL INDEX IX_Incidente_GeoLocation 
  ON Incidente(GeoLocation);
GO

-- ##########################
-- 8. Adjuntos de incidente (URLs externas)
-- ##########################

CREATE TABLE IncidenteAdjunto (
  AdjuntoID    INT      IDENTITY PRIMARY KEY,
  IncidenteID  INT      NOT NULL,
  Url          NVARCHAR(500) NOT NULL,
  FechaSubida  DATETIME NOT NULL DEFAULT GETDATE(),
  UsuarioID    INT      NOT NULL,   -- quién sube el adjunto
  CONSTRAINT FK_Adjunto_Incidente FOREIGN KEY (IncidenteID)
    REFERENCES Incidente(IncidenteID) ON DELETE CASCADE,
  CONSTRAINT FK_Adjunto_Usuario    FOREIGN KEY (UsuarioID)
    REFERENCES Usuario(UsuarioID)
);
GO

-- ##########################
-- 9. Notificaciones de estado
-- ##########################

CREATE TABLE Notificacion (
  NotificacionID INT      IDENTITY PRIMARY KEY,
  UsuarioID      INT      NOT NULL,     -- destinatario
  IncidenteID    INT      NOT NULL,
  Mensaje        NVARCHAR(255) NOT NULL,
  IsLeido        BIT      NOT NULL DEFAULT 0,
  Fecha          DATETIME NOT NULL DEFAULT GETDATE(),
  CONSTRAINT FK_Notificacion_Usuario  FOREIGN KEY (UsuarioID)
    REFERENCES Usuario(UsuarioID),
  CONSTRAINT FK_Notificacion_Incidente FOREIGN KEY (IncidenteID)
    REFERENCES Incidente(IncidenteID) ON DELETE CASCADE
);
GO
