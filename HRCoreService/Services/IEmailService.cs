using HRCoreDB.Entities;
using System.Threading.Tasks;

namespace HRCoreService.Services
{
    public interface IEmailService
    {
        Task SendContractNotificationAsync(Employee employee, Contract contract);
    }
}
