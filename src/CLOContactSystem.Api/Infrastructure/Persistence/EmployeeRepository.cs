using CLOContactSystem.Api.Domain.Entities;
using CLOContactSystem.Api.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CLOContactSystem.Api.Infrastructure.Persistence;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _context;

    public EmployeeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Employee> Items, int TotalCount)> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var totalCount = await _context.Employees.CountAsync(ct);
        var items = await _context.Employees
            .OrderBy(e => e.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<Employee?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return await _context.Employees
            .FirstOrDefaultAsync(e => e.Name == name, ct);
    }

    public async Task AddRangeAsync(IEnumerable<Employee> employees, CancellationToken ct = default)
    {
        await _context.Employees.AddRangeAsync(employees, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
    {
        return await _context.Employees.AnyAsync(e => e.Name == name, ct);
    }

    public async Task<List<string>> GetExistingNamesAsync(IEnumerable<string> names, CancellationToken ct = default)
    {
        var nameList = names.ToList();
        return await _context.Employees
            .Where(e => nameList.Contains(e.Name))
            .Select(e => e.Name)
            .ToListAsync(ct);
    }
}
