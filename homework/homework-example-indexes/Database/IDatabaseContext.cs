using OtusSocialNetwork.Database.Entities;

namespace OtusSocialNetwork.Database;


public interface IDatabaseContext
{
    Task<(bool isSuccess, string msg, AccountEntity? account)> GetLoginAsync(string id);
    Task<(bool isSuccess, string msg, string userId)> RegisterAsync(UserEntity user, string password);
    Task<(bool isSuccess, string msg, UserEntity? user)> GetUserAsync(string id);

    Task<(bool isSuccess, string msg, List<UserEntity> users)> SearchUserAsync(string firstName, string lastName);
}
