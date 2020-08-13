using Mgi.ALM.API.RestApi;
using Mgi.Instrument.ALM.API.Model;
using Mgi.Instrument.ALM.Device;
using System.Collections.Generic;
using System.Web.Http;

namespace Mgi.Instrument.ALM.API.Controller
{
    /// <summary>
    /// 移液器API
    /// </summary>
    [RoutePrefix("api/pipette")]
    public class PipetteController : ApiController
    {
        IALMPipettes _pipettes;
        public PipetteController(IALMPipettes pipettes)
        {
            _pipettes = pipettes;
        }

        /// <summary>
        /// 通道集合
        /// </summary>
        [HttpGet]
        [Route("channel")]
        public ApiResponse<IEnumerable<PipetteChannel>> GetChannels()
        {
            var channels = _pipettes.Channels.Values;
            return new ApiResponse<IEnumerable<PipetteChannel>>(channels);
        }
        /// <summary>
        /// 单通道吸液
        /// </summary>
        /// <param name="request">吸液通道</param>
        [HttpPost]
        [Route("aspirate")]
        public ApiResponse Aspirate(PipetteChannelAspirateRequest request)
        {
            _pipettes.Aspirate(request.TubeId, request.Channel, request.Position, request.Volume);
            return ResponseCode.Ok;
        }
        /// <summary>
        /// 两个通道同时洗液
        /// </summary>
        /// <param name="request"></param>
        [HttpPost]
        [Route("aspirateBoth")]
        public ApiResponse AspirateBoth(PipetteAspirateRequest request)
        {
            _pipettes.AspirateBoth(request.TubeId, request.Position, request.Volume);
            return ResponseCode.Ok;
        }
        /// <summary>
        /// 单通道排液
        /// </summary>
        /// <param name="request">排液通道</param>
        [HttpPost]
        [Route("unAspirate")]
        public ApiResponse UnAspirate(PipetteChannelRequest request)
        {
            _pipettes.UnAspirate(request.Channel, request.Position);
            return ResponseCode.Ok;
        }

        /// <summary>
        /// 两个通道同时排液
        /// </summary>
        [HttpPost]
        [Route("unAspirateBoth")]
        public ApiResponse UnAspirateBoth(PipetteRequest request)
        {
            _pipettes.UnAspirateBoth(request.Position);
            return ResponseCode.Ok;
        }
        /// <summary>
        /// 退单个吸头
        /// </summary>
        /// <param name="request"></param>
        [HttpPost]
        [Route("unLoadTip")]
        public ApiResponse UnLoadTip(UnloadTipRequest request)
        {
            _pipettes.UnLoadTip(request.Channel);
            return ResponseCode.Ok;
        }
        /// <summary>
        /// 扎单个吸头
        /// </summary>
        /// <param name="request"></param>
        [HttpPost]
        [Route("loadTip")]
        public ApiResponse LoadTip(PipetteChannelRequest request)
        {
            _pipettes.LoadTip(request.Channel, request.Position);
            return ResponseCode.Ok;
        }

        /// <summary>
        /// 扎两个吸头
        /// </summary>
        [HttpPost]
        [Route("loadTips")]
        public ApiResponse LoadTips(PipetteRequest request)
        {
            _pipettes.LoadTips(request.Position);
            return ResponseCode.Ok;
        }
        /// <summary>
        /// 退两个吸头
        /// </summary>
        [HttpPost]
        [Route("unloadTips")]
        public ApiResponse UnloadTips()
        {
            _pipettes.UnloadTips();
            return ResponseCode.Ok;
        }
        /// <summary>
        /// 两个通道同时运动
        /// </summary>
        /// <param name="request">位置名称枚举</param>
        [HttpPost]
        [Route("move")]
        public ApiResponse MoveTo(PipetteMoveRequest request)
        {
            _pipettes.MoveTo(request.Position, request.Row, request.Column);
            return ResponseCode.Ok;
        }
    }
}
