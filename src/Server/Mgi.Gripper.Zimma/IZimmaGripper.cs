namespace Mgi.Gripper.Zimma
{
    public interface IZimmaGripper
    {
        string Name { get; set; }
        void Initialize();

        void Close();

        /// <summary>
        /// 收紧夹爪(同步) 返回夹爪当前的位置
        /// </summary>
        int TightenGripper(byte gripForce, ushort teachPosition, byte positionTolerance, bool throwIfEmtpyGrasp);
        /// <summary>
        /// 释放夹爪(同步) 返回夹爪当前的位置
        /// </summary>
        int ReleaseGripper();
        /// <summary>
        /// 发送夹紧夹爪指令 立即返回，调用EndTighten阻塞
        /// </summary>
        /// <param name="gripForce"></param>
        /// <param name="teachPosition"></param>
        /// <param name="positionTolerance"></param>
        void BeginTighten(byte gripForce, ushort teachPosition);

        /// <summary>
        /// 返回夹爪当前的位置
        /// </summary>
        /// <param name="teachPosition"></param>
        /// <param name="positionTolerance"></param>
        /// <param name="throwIfEmptyGrasp"></param>
        /// <returns></returns>
        int EndTighten(ushort teachPosition, byte positionTolerance, bool throwIfEmptyGrasp);


        /// <summary>
        /// 发送松开夹爪指令 立即返回，调用EndRelease阻塞
        /// </summary>
        void BeginRelease();
        /// <summary>
        /// 返回夹爪当前的位置
        /// </summary>
        /// <returns></returns>
        int EndRelease();
    }
}
