using ServiceApis.Models;

namespace ServiceApis.IRepository
{
    public interface IERFRepository
    {
        ResultResponse<ERFResponseClass> SaveERFData(ERFRequestModel ErfData, int userId, string userName);
    }
}
