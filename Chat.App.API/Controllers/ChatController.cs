using System.Text.Json;
using Chat.App.API.Database;
using Chat.App.API.Helpers;
using Chat.App.API.Services;
using ChatRequestModel = Chat.App.API.Models.ChatRequest;
using ChatResponseModel = Chat.App.API.Models.ChatResponse;
using MessageModel = Chat.App.API.Models.Message;
using ConversationEntity = Chat.App.API.Database.Entities.Conversation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using Chat.App.API.Models;

namespace Chat.App.API.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly IAgentService _agentService;
    private readonly IConversationRepository _conversationRepository;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger<ChatController> _logger;

    public ChatController(
        IAgentService agentService,
        IConversationRepository conversationRepository,
        JsonSerializerOptions jsonOptions,
        ILogger<ChatController> logger)
    {
        _agentService = agentService;
        _conversationRepository = conversationRepository;
        _jsonOptions = jsonOptions;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IResult> Chat([FromBody] ChatRequestModel request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return Results.BadRequest(new { detail = "Message cannot be empty" });
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
                return Results.NotFound(new { detail = "Conversation not found" });
            }
            conversation = existing;
        }

        var userMessage = new ChatMessage(ChatRole.User, request.Message.Trim());
        var conversationIdHeader = conversation.Id.ToString();

        _logger.LogInformation("Streaming chat response for conversation {ConversationId}", conversation.Id);

        var sseResult = new SseResult(
            async (stream, streamCt) =>
            {
                var accumulator = new System.Text.StringBuilder();
                await foreach (var update in _agentService.StreamAsync(conversation.Id.ToString(), userMessage, streamCt))
                {
                    if (string.IsNullOrEmpty(update.Text))
                    {
                        continue;
                    }

                    accumulator.Append(update.Text);

                    var chunkPayload = new ChatResponseModel(conversation.Id, new MessageModel("assistant", update.Text, null));
                    await SseStreamHelper.WriteAsync(stream, SseStreamHelper.FormatJson(chunkPayload, jsonOptions: _jsonOptions), streamCt);
                }

                var finalPayload = new ChatResponseModel(conversation.Id, new MessageModel("assistant", accumulator.ToString(), null));
                await SseStreamHelper.WriteAsync(stream, SseStreamHelper.Done(finalPayload, _jsonOptions), streamCt);
            },
            headers: new Dictionary<string, string> { ["X-Conversation-Id"] = conversationIdHeader });

        return sseResult;
    }

}