using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Mgi.Instrument.ALM.API.Controller
{
    [RoutePrefix("api/tube")]
    public class TubeController : ApiController
    {
        public TubeController(IConfigProvider configProvider)
        {

        } 
    }
}
