using Chat.App.API.Database;
using Chat.App.API.Models;
using Microsoft.AspNetCore.Mvc;
using ConversationModel = Chat.App.API.Models.Conversation;
using ConversationDetailModel = Chat.App.API.Models.ConversationDetail;
using MessageModel = Chat.App.API.Models.Message;
using RenameConversationRequestModel = Chat.App.API.Models.RenameConversationRequest;

namespace Chat.App.API.Controllers;

[ApiController]
[Route("api/conversations")]
public class ConversationsController : ControllerBase
{
    private readonly IConversationRepository _conversationRepository;
    private readonly ILogger<ConversationsController> _logger;

    public ConversationsController(
        IConversationRepository conversationRepository,
        ILogger<ConversationsController> logger)
    {
        _conversationRepository = conversationRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<ConversationModel>>> List([FromQuery] int skip = 0, [FromQuery] int limit = 100, CancellationToken ct = default)
    {
        var conversations = await _conversationRepository.ListAsync(skip, limit, ct);
        var result = conversations.Select(c => new ConversationModel(c.Id, c.Title, c.CreatedAt, c.UpdatedAt)).ToList();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ConversationDetailModel>> Get(Guid id, CancellationToken ct)
    {
        var conversation = await _conversationRepository.GetAsync(id, ct);
        if (conversation is null)
        {
            return NotFound(new { detail = "Conversation not found" });
        }

        var result = new ConversationDetailModel(
            conversation.Id,
            conversation.Title,
            conversation.CreatedAt,
            conversation.UpdatedAt,
            new List<MessageModel>());
        return Ok(result);
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<ConversationModel>> Rename(Guid id, [FromBody] RenameConversationRequestModel request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new { detail = "Title cannot be empty" });
        }

        var conversation = await _conversationRepository.RenameAsync(id, request.Title, ct);
        if (conversation is null)
        {
            return NotFound(new { detail = "Conversation not found" });
        }

        return Ok(new ConversationModel(conversation.Id, conversation.Title, conversation.CreatedAt, conversation.UpdatedAt));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _conversationRepository.DeleteAsync(id, ct);
        if (!deleted)
        {
            return NotFound(new { detail = "Conversation not found" });
        }

        return Ok(new { deleted = true });
    }
}