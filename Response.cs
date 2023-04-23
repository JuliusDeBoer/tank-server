namespace Tanks
{
    public class Response
    {
        public string Code { get; set; }
        public string Message { get; set; }

        public Response(string code, string message)
        {
            Code = code;
            Message = message;
        }

        public static IResult Ok(Response response)
        {
            Log.Info(response);
            return TypedResults.Ok(response);
        }

        public static IResult BadRequest(Response response)
        {
            Log.Error(response);
            return TypedResults.BadRequest(response);
        }

        // General errors
        public static readonly Response ERR_BAD_ARGUMENTS = new("ERR_BAD_ARGUMENTS", "Arguments where invalid");

        // Tanks errors
        public static readonly Response ERR_NO_SUCH_TANK = new("ERR_NO_SUCH_TANK", "Tank was not found");
        public static readonly Response ERR_NOT_ENOUGH_ACTION_POINTS = new("ERR_NOT_ENOUGH_ACTION_POINTS", "Tank does not have enough action points");
        public static readonly Response ERR_MAX_LEVEL_REACHED = new("ERR_MAX_LEVEL_REACHED", "Tank is already at the highest level");
        public static readonly Response ERR_MAX_TANKS_REACHED = new("ERR_MAX_TANKS_REACHED", "Maximum number of tanks was reaced");

        // Account errors
        public static readonly Response ERR_NO_CREDENTIALS = new("ERR_NO_CREDENTIALS", "No credentials where provided");
        public static readonly Response ERR_INVALID_CREDENTIALS = new("ERR_INVALID_CREDENTIALS", "Provided credentials where invalid");
        public static readonly Response ERR_ACCOUNT_EXISTS = new("ERR_ACCOUNT_EXISTS", "Account with that email already exists");

        // success
        public static readonly Response OK = new("OK", "Operation was successful");
    }
}