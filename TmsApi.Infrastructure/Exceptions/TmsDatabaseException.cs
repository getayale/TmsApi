namespace TmsApi.Infrastructure.Exceptions;

public class TmsDatabaseException(string message)
    : Exception(message);