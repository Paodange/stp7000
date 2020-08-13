using Newtonsoft.Json;

namespace Mgi.STP7000.Infrastructure.ApiProtocol
{
    public class ApiResponse<T>
    {
        [JsonProperty(Order = 0)]
        public int Code { get; set; } = -1;
        [JsonProperty(Order = 10)]
        public string Message { get; set; }
        [JsonProperty(Order = 20)]
        public T Data { get; set; }

        public ApiResponse(ResponseCode responseCode, T data = default(T)) : this(responseCode)
        {
            Data = data;
        }

        public ApiResponse(T data = default(T)) : this(ResponseCode.Ok, data)
        {
        }
        public ApiResponse(ResponseCode responseCode)
        {
            Code = responseCode.Code;
            Message = responseCode.Message;
            Data = default(T);
        }

        public bool IsOK()
        {
            return Code == 200;
        }
    }

    public class ApiResponse : ApiResponse<object>
    {
        public ApiResponse(ResponseCode responseCode) : this(responseCode, null)
        {

        }
        public ApiResponse(ResponseCode responseCode, object data)
        {
            Code = responseCode.Code;
            Message = responseCode.Message;
            Data = data;
        }
        public static implicit operator ApiResponse(ResponseCode responseCode)
        {
            return new ApiResponse(responseCode);
        }
    }
}
