namespace NetcoreVuePwa.Models
{
  public class TokenConfigurationModel
  {
    public string Secret { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int AccessExpiration { get; set; } = 3600;
  }
}