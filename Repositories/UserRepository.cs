using System.Threading.Tasks;

namespace NetcoreVuePwa {
  public interface IUserRepository {
    Task<ApplicationUser> GetUserAndVerify(string email, string password);
  }

  public class UserRepository : IUserRepository
  {
    public Task<ApplicationUser> GetUserAndVerify(string email, string password)
    {
      throw new System.NotImplementedException();
    }
  }
}