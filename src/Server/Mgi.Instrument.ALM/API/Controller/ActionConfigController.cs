using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Mgi.Instrument.ALM.API.Controller
{
    [RoutePrefix("api/actionConfig")]
    public class ActionConfigController : ApiController
    {
        public ActionConfigController(IConfigProvider configProvider)
        {

        }
    }
}
