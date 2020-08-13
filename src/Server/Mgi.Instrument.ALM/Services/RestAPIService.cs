using Mgi.ALM.API.RestApi;
using System.Collections.Generic;
using System.Reflection;
using Unity;
using Unity.WebApi;

namespace Mgi.Instrument.ALM.Services
{
    public class RestAPIService : IBackgroundService
    {
        bool started = false;
        RestApiService service;
        public RestAPIService(IUnityContainer container, int port)
        {
            service = new RestApiService(port, new UnityDependencyResolver(container), new List<Assembly>() { typeof(RestAPIService).Assembly });
        }

        public void Start()
        {
            if (started) return;
            service.StartAsync().Wait();
            started = true;
        }

        public void Stop()
        {
            if (!started) return;
            service.StopAsync().Wait();
            started = false;
        }
    }


}
