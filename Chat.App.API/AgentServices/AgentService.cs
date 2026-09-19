
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace Chat.App.API.Services;

public interface IAgentService
{
    IAsyncEnumerable<AgentResponseUpdate> StreamAsync(
        string conversationId,
        ChatMessage message,
        CancellationToken ct = default);
}

public sealed class AgentService : IAgentService
{
    private readonly ChatClientAgent _agent;

    public AgentService(ChatClientAgent agent)
    {
        _agent = agent;
    }
    public async IAsyncEnumerable<AgentResponseUpdate> StreamAsync(
        string conversationId, ChatMessage message,
        [EnumeratorCancellation]
     CancellationToken ct = default)
    {

        AgentSession session = await _agent.CreateSessionAsync(ct);
        if (_agent.ChatHistoryProvider?.StateKeys.Count > 0)
        {
            foreach (var key in _agent.ChatHistoryProvider.StateKeys)
            {
                session.StateBag.SetValue(key, conversationId);
            }
        }

        await foreach (var update in _agent.RunStreamingAsync(message, session))
        {
            if (!string.IsNullOrEmpty(update.Text))
            {
                yield return update;
            }
        }

    }

}