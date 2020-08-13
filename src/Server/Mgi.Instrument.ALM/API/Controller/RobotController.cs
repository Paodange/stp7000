using Mgi.ALM.API.RestApi;
using Mgi.Instrument.ALM.API.Model;
using Mgi.Instrument.ALM.Device;
using System.Web.Http;

namespace Mgi.Instrument.ALM.API.Controller
{
    [RoutePrefix("api/robot")]
    public class RobotController : ApiController
    {
        IALMRobot _robot;
        public RobotController(IALMRobot robot)
        {
            _robot = robot;
        }

        /// <summary>
        /// 夹爪夹紧
        /// </summary>
        /// <param name="request"></param>
        [HttpPost]
        [Route("tightenGripper")]
        public ApiResponse TightenGripper(RobotGripperRequest request)
        {
            _robot.TightenGripper(request.TubeId);
            return ResponseCode.Ok;
        }
        /// <summary>
        /// 松开夹爪
        /// </summary>
        [HttpPost]
        [Route("releaseGripper")]
        public ApiResponse ReleaseGripper()
        {
            _robot.ReleaseGripper();
            return ResponseCode.Ok;
        }
        /// <summary>
        /// 复位
        /// </summary>
        [HttpPost]
        [Route("home")]
        public ApiResponse Home()
        {
            _robot.Home();
            return ResponseCode.Ok;
        }


        /// <summary>
        /// 抓取并返回
        /// </summary>
        /// <param name="request"></param>
        [HttpPost]
        [Route("grasp")]
        public ApiResponse Grasp(RobotRequest request)
        {
            _robot.Grasp(request.TubeId, request.Location, request.Row, request.Column);
            return ResponseCode.Ok;
        }

        /// <summary>
        /// 释放并返回
        /// </summary>
        /// <param name="request"></param>
        [HttpPost]
        [Route("loosen")]
        public ApiResponse Loosen(RobotRequest request)
        {
            _robot.Loosen(request.TubeId, request.Location, request.Row, request.Column);
            return ResponseCode.Ok;
        }

        /// <summary>
        /// 仅抓取  抓取后保持抓取位置
        /// </summary>
        /// <param name="request"></param>
        [HttpPost]
        [Route("graspOnly")]
        public ApiResponse GraspOnly(RobotRequest request)
        {
            _robot.GraspOnly(request.TubeId, request.Location, request.Row, request.Column);
            return ResponseCode.Ok;
        }

        /// <summary>
        /// 仅释放 释放后保持在释放的位置
        /// </summary>
        /// <param name="request"></param>
        [HttpPost]
        [Route("loosenOnly")]
        public ApiResponse LoosenOnly(RobotRequest request)
        {
            _robot.LoosenOnly(request.TubeId, request.Location, request.Row, request.Column);
            return ResponseCode.Ok;
        }
        /// <summary>
        /// 释放后 回安全位置
        /// </summary>
        /// <param name="request"></param>
        [HttpPost]
        [Route("goBack")]
        public ApiResponse GoBack(RobotRequest request)
        {
            _robot.GraspGoBack(request.Location, request.Row, request.Column);
            return ResponseCode.Ok;
        }
    }
}
