using Mgi.ALM.API.RestApi;
using Mgi.Instrument.ALM.API.Model;
using Mgi.Instrument.ALM.Device;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace Mgi.Instrument.ALM.API.Controller
{
    [RoutePrefix("api/lidUncover")]
    public class LidUncoverController : ApiController
    {
        IALMLidUnCover _lidUncover;
        public LidUncoverController(IALMLidUnCover lidUnCover)
        {
            _lidUncover = lidUnCover;
        }

        /// <summary>
        /// 拔盖 并扫码 返回扫到的条码  一个通道可以配两个条码枪，同时扫两个
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("uncoverAndScan")]
        public ApiResponse<IEnumerable<string>> UnCoverAndScan(LidUncoverAndScanRequest request)
        {
            var barcodes = _lidUncover.UnCoverAndScan(request.TubeId, request.LidUncover, request.Gripper);
            return new ApiResponse<IEnumerable<string>>(ResponseCode.Ok, barcodes);
        }

        /// <summary>
        /// 盖回去
        /// </summary>
        /// <param name="request"></param>
        [HttpPost]
        [Route("cover")]
        public ApiResponse Cover(LidCoverConverRequest request)
        {
            _lidUncover.Cover(request.TubeId, request.LidUncover);
            return ResponseCode.Ok;
        }

        /// <summary>
        /// 导轨移动
        /// </summary>
        /// <param name="request"></param>
        [HttpPost]
        [Route("sliderMove")]
        public ApiResponse SliderMoveTo(LidUncoverMoveRequest request)
        {
            _lidUncover.SliderMoveTo(request.LidUncover, request.SliderPosition);
            return ResponseCode.Ok;
        }

        /// <summary>
        /// 夹紧C
        /// </summary>
        [HttpPost]
        [Route("tightenCAxis")]
        public ApiResponse TightenCAxis(LidUncoverTightenCRequest request)
        {
            _lidUncover.TightenCAxis(request.TubeId, request.LidUncover);
            return ResponseCode.Ok;
        }

        /// <summary>
        /// 松开C
        /// </summary>
        [HttpPost]
        [Route("releaseCAxis")]
        public ApiResponse ReleaseCAxis(LidUncoverReleaseCRequest request)
        {
            _lidUncover.ReleaseCAxis(request.TubeId, request.LidUncover, request.Level);
            return ResponseCode.Ok;
        }

        /// <summary>
        /// 夹爪夹紧
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("tightGripper")]
        public ApiResponse TightGripper(TightenRequest request)
        {
            if (request.Gripper == "A")
            {
                _lidUncover.WorkUnits[request.LidUncover].GripperA.TightenGripper(request.GripForce, request.TeachPosition, request.PositionTolerance, request.ThrowIfEmptyGrasp);
            }
            else if (request.Gripper == "B")
            {
                _lidUncover.WorkUnits[request.LidUncover].GripperB.TightenGripper(request.GripForce, request.TeachPosition, request.PositionTolerance, request.ThrowIfEmptyGrasp);
            }
            else
            {
                return new ApiResponse(550, $"Unknow gripper :{request.Gripper}");
            }
            return ResponseCode.Ok;
        }
        /// <summary>
        /// 夹爪松开
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("releaseGripper")]
        public ApiResponse ReleaseGripper(GripperRequest request)
        {
            if (request.Gripper == "A")
            {
                _lidUncover.WorkUnits[request.LidUncover].GripperA.ReleaseGripper();
            }
            else if (request.Gripper == "B")
            {
                _lidUncover.WorkUnits[request.LidUncover].GripperB.ReleaseGripper();
            }
            else
            {
                return new ApiResponse(550, $"Unknow gripper :{request.Gripper}");
            }
            return ResponseCode.Ok;
        }
        /// <summary>
        /// 打开扫码枪
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("openScanner")]
        public ApiResponse OpenScanner(ScannerRequest request)
        {
            if (request.Scanner == "A")
            {
                _lidUncover.WorkUnits[request.LidUncover].BarcodeA.BeginConsequentTrigger();
            }
            else if (request.Scanner == "B")
            {
                _lidUncover.WorkUnits[request.LidUncover].BarcodeB.BeginConsequentTrigger();
            }
            else
            {
                return new ApiResponse(551, $"Unknow Scanner :{request.Scanner}");
            }
            return ResponseCode.Ok;
        }

        /// <summary>
        /// 关闭扫码枪
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("closeScanner")]
        public ApiResponse<string> CloseScanner(ScannerRequest request)
        {
            string barcode;
            if (request.Scanner == "A")
            {
                barcode = _lidUncover.WorkUnits[request.LidUncover].BarcodeA.EndConsequentTrigger();
            }
            else if (request.Scanner == "B")
            {
                barcode = _lidUncover.WorkUnits[request.LidUncover].BarcodeB.EndConsequentTrigger();
            }
            else
            {
                return new ResponseCode(552, $"Unknow Scanner :{request.Scanner}");
            }
            return new ApiResponse<string>(barcode);
        }

        /// <summary>
        /// 断言滑块试管位置的状态
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("assertSamplePosState")]
        public ApiResponse AssertSamplePosState(AssertSamplePosStateRequest request)
        {
            _lidUncover.AssertSamplePosState(request.LidUncover, request.Pos, request.State);
            return ResponseCode.Ok;
        }
    }
}
