using Deepwell.Common.CommonModel;
using System.Collections.Generic;

namespace Deepwell.Data.Interfaces
{
    interface ICustomerRepository
    {
        ResponseResult AddCustomer(CustomerInformation c);
        IEnumerable<State> GetStates();
        IEnumerable<Customer> Search(CustomerSearchRequest request);
        string Edit(CustomerInformation c);
        Customer GetById(int id);
        IEnumerable<Customer> GetCustomers();
        IEnumerable<Customer> GetCustomersActive();
        Customer GetByCustomerNumber(string customerNumber);
    }
}
