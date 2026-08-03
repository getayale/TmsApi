namespace TmsApi.Exceptions;

public class TmsDatabaseException(string message)
    : Exception(message);