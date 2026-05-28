using HabitApp.Domain.Entities;
using HabitApp.Domain.Services.Interfaces;
using HabitApp.Infrastructure.Data.Interfaces;

namespace HabitApp.Domain.Services;

public class UserService(IUserRepository repository) : BaseService<User>(repository), IUserService
{
    private readonly new IUserRepository _repository = repository;

    public async Task<User?> AuthenticateAsync(string email, string password)
    {
        var user = await _repository.GetByEmailAsync(email);
        if (user == null || user.Password != password) return null;
        return user;
    }
}
