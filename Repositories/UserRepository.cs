using System;
using System.Threading.Tasks;

namespace NetcoreVuePwa {
  public interface IUserRepository {
    Task<ApplicationUser> GetUserAndVerify(string email, string password);
  }

  public class UserRepository : IUserRepository
  {
    public async Task<ApplicationUser> GetUserAndVerify(string email, string password)
    {
      return new ApplicationUser {
        Id = Guid.NewGuid(),
        Email = email
      };
    }
  }
}