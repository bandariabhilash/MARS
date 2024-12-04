using DataAccess.Db;
using ServiceApis.Models;

namespace ServiceApis.IRepository
{
    public interface ICustomerRepository
    {
        CustomerModel GetCustomerDetails(int CustomerId);

        Contact ValidCustomerDetails(string customerId);
        bool IsTechUnAvailable(int techId, DateTime StartTime, out int replaceTech);
    }
}
