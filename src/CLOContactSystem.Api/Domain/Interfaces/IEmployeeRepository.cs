using CLOContactSystem.Api.Domain.Entities;

namespace CLOContactSystem.Api.Domain.Interfaces;

public interface IEmployeeRepository
{
    Task<(List<Employee> Items, int TotalCount)> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<Employee?> GetByNameAsync(string name, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<Employee> employees, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
    Task<List<string>> GetExistingNamesAsync(IEnumerable<string> names, CancellationToken ct = default);
}
