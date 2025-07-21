USE CopCR_Dev;
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



CREATE PROCEDURE dbo.ValidarInicioSesion
    @CedulaIdentidad NVARCHAR(100)
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
        U.Contrasena,
        CASE 
            WHEN A.UsuarioID IS NOT NULL THEN 'ADMIN'
            WHEN UF.UsuarioID IS NOT NULL THEN 'USER'
            ELSE 'UNKNOWN'
        END AS TipoUsuario
    FROM Usuario U
    LEFT JOIN Administrador A ON U.UsuarioID = A.UsuarioID
    LEFT JOIN UsuarioFinal UF ON U.UsuarioID = UF.UsuarioID
    WHERE U.CedulaIdentidad = @CedulaIdentidad
      AND U.Activo = 1;
END


CREATE PROCEDURE dbo.ConsultarPerfilUsuario
    @UsuarioID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        U.CedulaIdentidad AS Identificacion,
        U.Nombre,
        U.PrimerApellido,
        U.SegundoApellido,
        U.Email,
        U.NombreUsuario,
        U.FotoPerfilUrl,
        U.FechaRegistro,
        UF.TelefonoContacto,
        UF.FechaNacimiento,
        UF.PuntosConfianza,
        UF.AceptaNotificacionesPush
    FROM Usuario U
    LEFT JOIN UsuarioFinal UF ON U.UsuarioID = UF.UsuarioID
    WHERE U.UsuarioID = @UsuarioID;
END;
GO

ALTER PROCEDURE dbo.ActualizarPerfilUsuario
    @UsuarioID                INT,
    @Nombre                   NVARCHAR(100),
    @PrimerApellido           NVARCHAR(100),
    @SegundoApellido          NVARCHAR(100),
    @Email                    NVARCHAR(200),
    @NombreUsuario            NVARCHAR(100),
    @TelefonoContacto         NVARCHAR(20),
    @FechaNacimiento          DATE,
    @FotoPerfilUrl            NVARCHAR(500),
    @AceptaNotificacionesPush BIT
AS
BEGIN
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRAN;
        UPDATE dbo.Usuario
        SET
            Nombre         = @Nombre,
            PrimerApellido = @PrimerApellido,
            SegundoApellido= @SegundoApellido,
            Email          = @Email,
            NombreUsuario  = @NombreUsuario,
            FotoPerfilUrl  = @FotoPerfilUrl
        WHERE UsuarioID = @UsuarioID;
        UPDATE dbo.UsuarioFinal
        SET
            TelefonoContacto        = @TelefonoContacto,
            FechaNacimiento         = @FechaNacimiento,
            AceptaNotificacionesPush= @AceptaNotificacionesPush
        WHERE UsuarioID = @UsuarioID;

        COMMIT;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK;
        THROW;
    END CATCH
END;
GO
    

select * from Usuario;