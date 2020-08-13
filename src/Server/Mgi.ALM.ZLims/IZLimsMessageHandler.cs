using System.Threading.Tasks;
using Mgi.ALM.ZLims.Protocol;

namespace Mgi.ALM.ZLims
{
    public interface IZLimsMessageHandler
    {
        void SendMessage(ZLimsMessage message);
        Task SendMessageAsync(ZLimsMessage message);

    }
}
