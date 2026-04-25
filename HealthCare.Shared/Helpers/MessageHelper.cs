namespace HealthCare.Shared.Helpers;


public static class MessageHelper
{
    public static string Created(string entity)    => $"{entity} creado exitosamente.";
    public static string Updated(string entity)    => $"{entity} actualizado exitosamente.";
    public static string Deleted(string entity)    => $"{entity} eliminado exitosamente.";
    public static string Deactivated(string entity)=> $"{entity} desactivado exitosamente.";
    public static string Found(string entity)      => $"{entity} encontrado exitosamente.";
    public static string NotFound(string entity)   => $"{entity} no encontrado.";
    public static string AlreadyExists(string entity) => $"{entity} ya existe.";
    public static string Custom(string message)       => message;

}