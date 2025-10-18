using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common.Messages
{
    public class MessageUsuario
    {
        public static readonly string EmailRecuperacion = "El correo electrónico de recuperación no es válido.";

        public static readonly string NombresInvalid = "Nombres no encontrados en los datos del usuario.";

        public static readonly string UpdateInvalid = "No se pudo actualizar el usuario.";


        public static readonly string UsuarioDTOInvalid = " El objeto usuarioDTO no puede ser nulo";

        public static readonly string InvalidID = "El identificador del usuario es invalido";

        public static readonly string InvalidUsuarioContratista = "No se ha proporcionado información de contratistas.";


        public static readonly string DatosUsuarioInvalid = "No se encontraron datos del usuario para el contratista.";


        public static readonly string ValidarClave = " La contraseña debe tener al menos una letra, un digito, y un  caracter no alfanúmerico.";

        public static readonly string InvalidToken = "No se pudo generar el token JWT.";

        public const string InvalidUserName = "El nombre de usuario es obligatorio.";

        public const string InvalidPassword = "La contraseña es obligatoria.";

    }
}
