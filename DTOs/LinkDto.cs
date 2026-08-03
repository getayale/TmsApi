namespace TmsApi.DTOs;

public record LinkDto(
    string Href,
    string Rel,
    string Method);