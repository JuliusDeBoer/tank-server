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

        public static readonly Response ERR_NO_SUCH_TANK = new("ERR_NO_SUCH_TANK", "Tank was not found");
        public static readonly Response ERR_BAD_ARGUMENTS = new("ERR_BAD_ARGUMENTS", "Arguments where invalid");
        public static readonly Response ERR_NOT_ENOUGH_ACTION_POINTS = new("ERR_NOT_ENOUGH_ACTION_POINTS", "Tank does not have enough action points");

        public static readonly Response OK = new("OK", "Operation was successful");
    }
}