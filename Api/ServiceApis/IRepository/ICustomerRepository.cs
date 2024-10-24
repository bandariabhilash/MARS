using ServiceApis.Models;

namespace ServiceApis.IRepository
{
    public interface ICustomerRepository
    {
        CustomerModel GetCustomerDetails(int CustomerId);
    }
}
