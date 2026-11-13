namespace MEMS.Data.Services;

// What the services report. The Api turns them into 404, 409, 403 and 400 problem responses;
// the Admin app shows the message on the form.
public class NotFoundException(string message) : Exception(message);

public class ConflictException(string message) : Exception(message);

public class ForbiddenException(string message) : Exception(message);

public class RuleViolationException(string message) : Exception(message);
