namespace MEMS.Data;

public enum ResultStatus { Ok, Created, NotFound, Forbidden, Invalid, Conflict }

// Outcome of a service call. The API maps it to HTTP status codes; the Admin app shows the messages in the form.
public class ServiceResult
{
    public ResultStatus Status { get; init; }
    public string? Error { get; init; }
    // field name -> messages (for Invalid / Conflict)
    public Dictionary<string, string[]>? Errors { get; init; }
    public bool Succeeded => Status is ResultStatus.Ok or ResultStatus.Created;

    public static ServiceResult Success() => new() { Status = ResultStatus.Ok };
    public static ServiceResult NotFound(string error = "The requested item was not found.") => new() { Status = ResultStatus.NotFound, Error = error };
    public static ServiceResult Forbidden(string error = "You do not have access to this municipality.") => new() { Status = ResultStatus.Forbidden, Error = error };
    public static ServiceResult Invalid(Dictionary<string, string[]> errors) => new() { Status = ResultStatus.Invalid, Error = "Validation failed.", Errors = errors };
    public static ServiceResult Invalid(string field, string message) => Invalid(new Dictionary<string, string[]> { [field] = [message] });
    public static ServiceResult Conflict(string field, string message) => new() { Status = ResultStatus.Conflict, Error = message, Errors = new() { [field] = [message] } };

    // First message to show a person when there is no field to attach it to.
    public string Message => Errors?.Values.SelectMany(v => v).FirstOrDefault() ?? Error ?? "The operation failed.";
}

public class ServiceResult<T> : ServiceResult
{
    public T? Value { get; init; }

    public static ServiceResult<T> Success(T value, ResultStatus status = ResultStatus.Ok) => new() { Status = status, Value = value };

    public static ServiceResult<T> From(ServiceResult failure) =>
        new() { Status = failure.Status, Error = failure.Error, Errors = failure.Errors };
}
