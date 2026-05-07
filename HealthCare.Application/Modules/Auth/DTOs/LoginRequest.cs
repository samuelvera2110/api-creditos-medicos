namespace HealthCare.Application.Modules.Auth.DTOs;

public record LoginRequest(string Username, string Password, string? IpAddress = null);