namespace Mgi.STP7000.Infrastructure.ApiProtocol
{
    public class ResponseCode
    {
        public int Code { get; set; }
        public string Message { get; set; }


        public ResponseCode Format(params object[] args)
        {
            return new ResponseCode()
            {
                Code = Code,
                Message = string.Format(Message, args)
            };
        }
        protected ResponseCode()
        {

        }
        protected ResponseCode(int code, string message)
        {
            Code = code;
            Message = message;
        }

        public static readonly ResponseCode Ok = new ResponseCode(200, "OK");
        public static readonly ResponseCode UnAuthorized = new ResponseCode(401, "UnAuthorized");
        public static readonly ResponseCode NotFound = new ResponseCode(404, "Not Found");
        public static readonly ResponseCode InternalServerError = new ResponseCode(500, "Internal Server Error");
        public static readonly ResponseCode ModelValidateError = new ResponseCode(503, "{0}");
        public bool IsOK()
        {
            return Code == 200;
        }
    }
}
