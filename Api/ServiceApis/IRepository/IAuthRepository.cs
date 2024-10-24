using DataAccess.Db;

namespace ServiceApis.IRepository
{
    public interface IAuthRepository
    {
        FbUserMaster GetUser(string username);
        bool CheckUser(string username);
        bool CheckPassword(string username, string password);
        void UpdateUser(FbUserMaster user);

        void UpdateRefreshToken(UserRefreshtoken refreshtoken);

        UserRefreshtoken getWhuserRefreshtoken(string refreshtoken);


    }
}
