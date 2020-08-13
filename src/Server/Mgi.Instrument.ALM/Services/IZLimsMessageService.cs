using Mgi.ALM.ZLims.Protocol;

namespace Mgi.Instrument.ALM.Services
{
    public interface IZLimsMessageService : IBackgroundService
    {
        void Push(ZLimsMessage message);
    }
}
