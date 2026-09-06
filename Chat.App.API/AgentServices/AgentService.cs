
using System.Runtime.CompilerServices;
using System.Text;
using Google.GenAI;
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

    public AgentService(string apiKey, string model, string name, string systemPrompt)
    {
        _agent = new(
            new Client(vertexAI: false, apiKey: apiKey).AsIChatClient(model),
            name: name.Trim(),
            instructions: systemPrompt.Trim());
    }

    public async IAsyncEnumerable<AgentResponseUpdate> StreamAsync(
        string conversationId, ChatMessage message,
        [EnumeratorCancellation]
     CancellationToken ct = default)
    {
        AgentSession session = await _agent.CreateSessionAsync(conversationId, ct);
        StringBuilder accumulatedText = new();
        await foreach (var update in _agent.RunStreamingAsync(message, session))
        {
            if (!string.IsNullOrEmpty(update.Text))
            {
                accumulatedText.Append(update.Text);
                yield return update;
            }
        }
        yield return new AgentResponseUpdate(ChatRole.Assistant, accumulatedText.ToString());

    }

}