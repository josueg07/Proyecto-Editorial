using Microsoft.AspNetCore.Identity;
namespace PlataformaEditorial.Web.Localizacion;
public class ErroresIdentityEspanol : IdentityErrorDescriber
{
    private static IdentityError Error(string codigo,string descripcion)=>new(){Code=codigo,Description=descripcion};
    public override IdentityError DefaultError()=>Error(nameof(DefaultError),"No se pudo completar la operación.");
    public override IdentityError ConcurrencyFailure()=>Error(nameof(ConcurrencyFailure),"La cuenta cambió. Recarga la página.");
    public override IdentityError PasswordMismatch()=>Error(nameof(PasswordMismatch),"La contraseña actual no es correcta.");
    public override IdentityError InvalidToken()=>Error(nameof(InvalidToken),"El enlace o código no es válido o ya venció.");
    public override IdentityError LoginAlreadyAssociated()=>Error(nameof(LoginAlreadyAssociated),"Ese acceso externo ya está asociado a otra cuenta.");
    public override IdentityError InvalidUserName(string? userName)=>Error(nameof(InvalidUserName),"El nombre de usuario no es válido.");
    public override IdentityError InvalidEmail(string? email)=>Error(nameof(InvalidEmail),"El correo electrónico no es válido.");
    public override IdentityError DuplicateUserName(string userName)=>Error(nameof(DuplicateUserName),"Ya existe una cuenta con ese nombre de usuario.");
    public override IdentityError DuplicateEmail(string email)=>Error(nameof(DuplicateEmail),"Ya existe una cuenta con ese correo electrónico.");
    public override IdentityError InvalidRoleName(string? role)=>Error(nameof(InvalidRoleName),"El rol no es válido.");
    public override IdentityError DuplicateRoleName(string role)=>Error(nameof(DuplicateRoleName),"El rol ya existe.");
    public override IdentityError UserAlreadyHasPassword()=>Error(nameof(UserAlreadyHasPassword),"La cuenta ya tiene una contraseña.");
    public override IdentityError UserLockoutNotEnabled()=>Error(nameof(UserLockoutNotEnabled),"Esta cuenta no admite bloqueo.");
    public override IdentityError UserAlreadyInRole(string role)=>Error(nameof(UserAlreadyInRole),"La cuenta ya tiene ese rol.");
    public override IdentityError UserNotInRole(string role)=>Error(nameof(UserNotInRole),"La cuenta no tiene ese rol.");
    public override IdentityError PasswordTooShort(int length)=>Error(nameof(PasswordTooShort),$"La contraseña debe tener al menos {length} caracteres.");
    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars)=>Error(nameof(PasswordRequiresUniqueChars),$"La contraseña debe contener al menos {uniqueChars} caracteres diferentes.");
    public override IdentityError PasswordRequiresNonAlphanumeric()=>Error(nameof(PasswordRequiresNonAlphanumeric),"La contraseña debe incluir un símbolo.");
    public override IdentityError PasswordRequiresDigit()=>Error(nameof(PasswordRequiresDigit),"La contraseña debe incluir un número.");
    public override IdentityError PasswordRequiresLower()=>Error(nameof(PasswordRequiresLower),"La contraseña debe incluir una letra minúscula.");
    public override IdentityError PasswordRequiresUpper()=>Error(nameof(PasswordRequiresUpper),"La contraseña debe incluir una letra mayúscula.");
    public override IdentityError RecoveryCodeRedemptionFailed()=>Error(nameof(RecoveryCodeRedemptionFailed),"El código de recuperación no es válido.");
}
