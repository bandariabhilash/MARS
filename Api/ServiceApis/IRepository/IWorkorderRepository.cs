using DataAccess.Db;
using ServiceApis.Models;

namespace ServiceApis.IRepository
{
    public interface IWorkorderRepository
    {
        ResultResponse<ERFResponseClass> SaveERFWorkorder(WorkorderRequestModel RequestData, int userId, string userName);
        ResultResponse<ERFResponseClass> SaveWorkorderData(WorkorderManagementModel workorderManagement, int userId, string userName, FBContext _context);
    }
}
