namespace RpgWorkspace.Application.Interfaces;

public interface ITokenGenerator
{
    string GenerateToken(string userId, string email, IEnumerable<string> roles);
}
