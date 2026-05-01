namespace HealthCare.Application.Modules.Auth.DTOs;

public record AuthResponse(string Token, string Username, string FullName);