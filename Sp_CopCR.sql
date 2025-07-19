USE CopCR_Dev;
GO


CREATE PROCEDURE dbo.RegistroUsuario
    @CedulaIdentidad   NVARCHAR(20),
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
    BEGIN TRY
        BEGIN TRAN;

        -- 1) Insertar en Usuario
        INSERT INTO dbo.Usuario
            (CedulaIdentidad, Nombre, PrimerApellido, SegundoApellido,
             Email, NombreUsuario, Contrasena, FechaRegistro, FotoPerfilUrl, Activo)
        VALUES
            (@CedulaIdentidad, @Nombre, @PrimerApellido, @SegundoApellido,
             @Email, @NombreUsuario, @Contrasena, GETDATE(), @FotoPerfilUrl, 1);

        -- 2) Obtener el ID recién generado
        DECLARE @NewUserID INT = SCOPE_IDENTITY();

        -- 3) Insertar en UsuarioFinal
        INSERT INTO dbo.UsuarioFinal
            (UsuarioID, FechaNacimiento, TelefonoContacto)
        VALUES
            (@NewUserID, @FechaNacimiento, @TelefonoContacto);

        COMMIT;
        RETURN 1;
    END TRY
    BEGIN CATCH
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


