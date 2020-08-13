using Mgi.ALM.API.RestApi;
using System.Web.Http;

namespace Mgi.Instrument.ALM.API.Controller
{
    [RoutePrefix("api/alm")]
    public class AlmController : ApiController
    {
        IAutoLidMachine _machine;
        public AlmController(IAutoLidMachine machine)
        {
            _machine = machine;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("powerOn")]
        public ApiResponse PowerOn()
        {
            _machine.PowerOn();
            return ResponseCode.Ok;
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("powerOff")]
        public ApiResponse PowerOff()
        {
            _machine.PowerOff();
            return ResponseCode.Ok;
        }
    }
}
