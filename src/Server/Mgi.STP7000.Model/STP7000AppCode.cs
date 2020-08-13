using Mgi.STP7000.Infrastructure.ApiProtocol;

namespace Mgi.STP7000.Model
{
    public class STP7000AppCode : ResponseCode
    {
        private STP7000AppCode(int code, string message) : base(code, message)
        {
            Code = code;
            Message = message;
        }


        public static readonly STP7000AppCode UserNameOrPasswordError = new STP7000AppCode(1001, "User name or password error");
    }
}
