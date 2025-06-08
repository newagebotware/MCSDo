using ChatApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatApp.Services;

public interface IDatabaseService
{
    Task InitializeAsync();
    Task SaveMessageAsync(Message message);
    Task<List<Message>> GetRecentMessagesAsync(int limit);
}
