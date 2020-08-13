using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mgi.STP7000.Infrastructure.ApiProtocol;
using Mgi.STP7000.Model;
using Mgi.STP7000.Model.Entity;
using Mgi.STP7000.Model.Request;

namespace Mgi.STP7000.Service.Impl
{
    public class UserService : IUserService
    {
        readonly List<UserInfo> users = new List<UserInfo>()
        {
            new UserInfo()
            {
                 Name = "管理员",
                 Password = "mgispx",
                 UserName = "admin",
                 Role = "admin",
            },
            new UserInfo()
            {
                 Name = "普通用户",
                 Password = "123456",
                 UserName = "user",
                 Role="user"
            }
        };
        public UserInfo UserLogin(UserLoginRequest request)
        {
            var user = users.FirstOrDefault(x => x.UserName == request.UserName && x.Password == request.Password);
            if (user == null)
            {
                throw new BusinessException(STP7000AppCode.UserNameOrPasswordError);
            }
            return user;
        }
    }
}
