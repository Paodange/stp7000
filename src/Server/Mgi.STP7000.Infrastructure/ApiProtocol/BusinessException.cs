using System;

namespace Mgi.STP7000.Infrastructure.ApiProtocol
{
    public class BusinessException : Exception
    {
        public ResponseCode ResponseCode { get; set; }
        public BusinessException(ResponseCode responseCode) : base(responseCode.Message)
        {
            ResponseCode = responseCode;
        }
    }
}
