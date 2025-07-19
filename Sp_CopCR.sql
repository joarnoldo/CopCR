USE CopCR_Pruebas;
GO

drop PROCEDURE dbo.RegistroUsuario;
go

CREATE PROCEDURE dbo.RegistroUsuario
    @CedulaIdentidad   NVARCHAR(100),
    @Nombre            NVARCHAR(100),
    @PrimerApellido    NVARCHAR(100),
    @SegundoApellido   NVARCHAR(100),
    @Email             NVARCHAR(200),
    @NombreUsuario     NVARCHAR(100),
    @Contrasena        NVARCHAR(200),
    @FechaNacimiento   DATE,
    @TelefonoContacto  NVARCHAR(20),
    @FotoPerfilUrl     NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF EXISTS (SELECT 1 FROM dbo.Usuario WHERE Email = @Email)
        RETURN -2;

    IF EXISTS (SELECT 1 FROM dbo.Usuario WHERE CedulaIdentidad = @CedulaIdentidad)
        RETURN -3;

    BEGIN TRY
        BEGIN TRAN;

        INSERT INTO dbo.Usuario
            (CedulaIdentidad, Nombre, PrimerApellido, SegundoApellido,
             Email, NombreUsuario, Contrasena, FechaRegistro, FotoPerfilUrl, Activo)
        VALUES
            (@CedulaIdentidad, @Nombre, @PrimerApellido, @SegundoApellido,
             @Email, @NombreUsuario, @Contrasena, GETDATE(), @FotoPerfilUrl, 1);

        DECLARE @NewUserID INT = SCOPE_IDENTITY();

        INSERT INTO dbo.UsuarioFinal
            (UsuarioID, FechaNacimiento, TelefonoContacto, PuntosConfianza, AceptaNotificacionesPush)
        VALUES
            (@NewUserID, @FechaNacimiento, @TelefonoContacto, 0, 1);

        COMMIT;
        RETURN 0;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK;
        RETURN -1;
    END CATCH
END
GO





EXEC dbo.RegistroUsuario
    @CedulaIdentidad  = '1-2345-6789',
    @Nombre           = 'Ana',
    @PrimerApellido   = 'Gómez',
    @SegundoApellido  = 'Pérez',
    @Email            = 'ana.gomez@ejemplo.com',
    @NombreUsuario    = 'anagomez',
    @Contrasena       = 'MiPass123',
    @FechaNacimiento  = '1990-05-15',
    @TelefonoContacto = '88881234',
    @FotoPerfilUrl    = 'https://example.com/fotos/ana.jpg';


select * from UsuarioFinal;

DECLARE @rc INT;

EXEC @rc = dbo.RegistroUsuario
    @CedulaIdentidad   = '12345678',
    @Nombre            = 'Ricardo',
    @PrimerApellido    = 'Montaner',
    @SegundoApellido   = 'Santos',
    @Email             = 'montaner02@example.com',
    @NombreUsuario     = 'prueba1',
    @Contrasena        = '$2a$11$7dHuiwT3IKcg7W4n7ErtqeL2Q52EdfXsTHHUjmMwFGr9sXg8xU',
    @FechaNacimiento   = '1980-01-01',
    @TelefonoContacto  = '8888-8888',
    @FotoPerfilUrl     = NULL;

SELECT @rc AS ReturnCode;













CREATE PROCEDURE dbo.ValidarInicioSesion
    @CedulaIdentidad NVARCHAR(100),
    @Contrasena NVARCHAR(200)
AS
BEGIN
    SELECT 
        U.UsuarioID,
        U.CedulaIdentidad,
        U.Nombre,
        U.PrimerApellido,
        U.SegundoApellido,
        U.Email,
        U.NombreUsuario,
        U.FotoPerfilUrl,
        CASE 
            WHEN A.UsuarioID IS NOT NULL THEN 'ADMIN'
            WHEN UF.UsuarioID IS NOT NULL THEN 'USER'
            ELSE 'UNKNOWN'
        END AS TipoUsuario
    FROM Usuario U
    LEFT JOIN Administrador A ON U.UsuarioID = A.UsuarioID
    LEFT JOIN UsuarioFinal UF ON U.UsuarioID = UF.UsuarioID
    WHERE U.CedulaIdentidad = @CedulaIdentidad
      AND U.Contrasena = @Contrasena
      AND U.Activo = 1;
END

