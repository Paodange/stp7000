using Mgi.Instrument.ALM.Device;
using System.Web.Http;

namespace Mgi.Instrument.ALM.API.Controller
{
    [RoutePrefix("api/ioboard")]
    public class IOBoardController : ApiController
    {
        IALMIOBoard _ioboard;
        public IOBoardController(IALMIOBoard ioboard)
        {
            _ioboard = ioboard;
        }



    }
}
