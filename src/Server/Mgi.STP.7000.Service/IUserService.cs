using Mgi.STP7000.Model.Entity;
using Mgi.STP7000.Model.Request;

namespace Mgi.STP7000.Service
{
    public interface IUserService
    {
        UserInfo UserLogin(UserLoginRequest request);
    }
}
