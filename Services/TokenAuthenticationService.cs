using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NetcoreVuePwa.Models;

namespace NetcoreVuePwa {
  public interface ITokenAuthenticationService {
    Task<Tuple<string, DateTime>> Authenticate(TokenRequestModel model);
  }

  public class TokenAuthenticationService : ITokenAuthenticationService
  {
    private TokenConfigurationModel _tokenConfig;
    private IUserRepository _usersRepository;

    public TokenAuthenticationService(IOptions<TokenConfigurationModel> tokenConfig, IUserRepository userRepository)
    {
      _tokenConfig = tokenConfig.Value;
      _usersRepository = userRepository;
    }

    public async Task<Tuple<string, DateTime>> Authenticate(TokenRequestModel model)
    {
      // Instantiate the return object
      Tuple<string, DateTime> returnObject = new Tuple<string, DateTime>(null, DateTime.MinValue);
      // Fetch user information
      var user = await _usersRepository.GetUserAndVerify(model.Email, model.Password);
      if (user == null) return returnObject;
      // Create a claims list
      var claims = new [] {
        new Claim("uid", user.Id.ToString())
      };
      // Generate a key, same as startup.cs
      var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_tokenConfig.Secret));
      // Generate new signing credentials
      var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

      // Finally, create the token
      DateTime expiration = DateTime.Now.AddMinutes(_tokenConfig.AccessExpiration);
      var jwtToken = new JwtSecurityToken(
        _tokenConfig.Issuer,
        _tokenConfig.Audience,
        claims,
        expires: expiration,
        signingCredentials: credentials
      );
      var token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
      returnObject = new Tuple<string, DateTime>(token, expiration);
      return returnObject;
    }
  }
}