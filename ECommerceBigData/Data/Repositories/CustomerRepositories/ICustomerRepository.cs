namespace ECommerceBigData.Data.Repositories.CustomerRepositories
{
    public interface ICustomerRepository
    {
        Task<int> GetTotalCustomerCountAsync();
    }
}
