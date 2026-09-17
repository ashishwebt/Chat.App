using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Net.ServerSentEvents;

using Chat.App.API.Database;
using Chat.App.API.Services;

using ChatRequestModel = Chat.App.API.Models.ChatRequest;
using ChatResponseModel = Chat.App.API.Models.ChatResponse;
using MessageModel = Chat.App.API.Models.Message;
using ConversationEntity = Chat.App.API.Database.Entities.Conversation;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;

namespace Chat.App.API.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly IAgentService _agentService;
    private readonly IConversationRepository _conversationRepository;
    private readonly ILogger<ChatController> _logger;

    public ChatController(
        IAgentService agentService,
        IConversationRepository conversationRepository,
        ILogger<ChatController> logger)
    {
        _agentService = agentService;
        _conversationRepository = conversationRepository;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IResult> Chat(
        [FromBody] ChatRequestModel request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return Results.BadRequest(new
            {
                detail = "Message cannot be empty"
            });
        }

        var conversationId = request.ConversationId;

        ConversationEntity conversation;

        if (conversationId is null)
        {
            conversation = await _conversationRepository.CreateAsync(ct: ct);
        }
        else
        {
            var existing =
                await _conversationRepository.GetAsync(
                    conversationId.Value,
                    ct);

            if (existing is null)
            {
                return Results.NotFound(new
                {
                    detail = "Conversation not found"
                });
            }

            conversation = existing;
        }

        var userMessage =
            new ChatMessage(
                ChatRole.User,
                request.Message.Trim());

        _logger.LogInformation(
            "Streaming chat response for conversation {ConversationId}",
            conversation.Id);

        Response.Headers["X-Conversation-Id"] =
            conversation.Id.ToString();

        return TypedResults.ServerSentEvents(
            StreamResponse(
                conversation.Id,
                userMessage,
                ct));
    }

    private async IAsyncEnumerable<SseItem<ChatResponseModel>> StreamResponse(
        Guid conversationId,
        ChatMessage userMessage,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var accumulator = new System.Text.StringBuilder();

        await foreach (var update in _agentService.StreamAsync(
            conversationId.ToString(),
            userMessage,
            ct))
        {
            if (string.IsNullOrEmpty(update.Text))
            {
                continue;
            }

            accumulator.Append(update.Text);

            var payload = new ChatResponseModel(
                conversationId,
                new MessageModel(
                    "assistant",
                    update.Text,
                    null));

            yield return new SseItem<ChatResponseModel>(
                payload,
                eventType: "message");
        }

        var finalPayload = new ChatResponseModel(
            conversationId,
            new MessageModel(
                "assistant",
                accumulator.ToString(),
                null));

        yield return new SseItem<ChatResponseModel>(
            finalPayload,
            eventType: "done");
    }
}