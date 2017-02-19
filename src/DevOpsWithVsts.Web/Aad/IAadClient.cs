using Microsoft.Azure.ActiveDirectory.GraphClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevOpsWithVsts.Web.Aad
{
    public interface IAadClient
    {
        Task<IUser> GetUser(string userId);
        Task<IUser> GetCurrentUser();
        Task<IEnumerable<IGroup>> GetGroups();
        Task<IGroup> GetGroupByName(string groupName);
        Task AddGroup(string groupName);
        Task RemoveGroup(IGroup group);
        Task AddUserToGroup(string userId, string groupId);
        Task RemoveUserFromGroup(string userId, string groupId);
    }
}