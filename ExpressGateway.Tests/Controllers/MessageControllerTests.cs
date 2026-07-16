using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ExpressGateway.Controllers;
using ExpressGateway.Models;
using ExpressGateway.Services;

namespace ExpressGateway.Tests.Controllers;

public class MessengerControllerTests
{
    private readonly Mock<IMessengerService> _mockService;
    private readonly Mock<ILogger<MessengerController>> _mockLogger;
    private readonly MessengerController _controller;

    public MessengerControllerTests()
    {
        _mockService = new Mock<IMessengerService>();
        _mockLogger = new Mock<ILogger<MessengerController>>();
        _controller = new MessengerController(_mockService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task SendMessage_WithValidRequest_ShouldReturnOk()
    {
        var request = new SendMessageRequest
        {
            ChatId = "chat_123",
            Text = "Test message"
        };

        var expectedResponse = new SendMessageResponse
        {
            Success = true,
            MessageId = "msg_456",
            SentAt = DateTime.UtcNow
        };

        _mockService
            .Setup(s => s.SendMessageAsync(It.IsAny<SendMessageRequest>()))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.SendMessage(request);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().BeEquivalentTo(expectedResponse);
        
        _mockService.Verify(s => s.SendMessageAsync(request), Times.Once);
    }

    [Fact]
    public async Task SendMessage_WithInvalidRequest_ShouldReturnBadRequest()
    {
        var request = new SendMessageRequest(); 
        _controller.ModelState.AddModelError("ChatId", "ChatId is required");

        var result = await _controller.SendMessage(request);
        result.Should().BeOfType<BadRequestObjectResult>();
        _mockService.Verify(s => s.SendMessageAsync(It.IsAny<SendMessageRequest>()), Times.Never);
    }

    [Fact]
    public async Task GetChats_ShouldReturnChatList()
    {
        var expectedChats = new ChatListResponse
        {
            Chats = new List<ChatInfo>
            {
                new() { Id = "chat1", Name = "Team Chat", MembersCount = 5 },
                new() { Id = "chat2", Name = "Project Group", MembersCount = 3 }
            },
            Total = 2
        };

        _mockService
            .Setup(s => s.GetChatsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(expectedChats);

        var result = await _controller.GetChats(50, 0);
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().BeEquivalentTo(expectedChats);
        
        _mockService.Verify(s => s.GetChatsAsync(50, 0), Times.Once);
    }

    [Fact]
    public async Task SetWebhook_WithValidUrl_ShouldReturnOk()
    {
        var request = new WebhookRequest
        {
            Url = "https://my-service.com/webhook",
            Secret = "test-secret"
        };

        _mockService
            .Setup(s => s.SetWebhookAsync(It.IsAny<WebhookRequest>()))
            .ReturnsAsync(new { status = "ok" });
	
        var result = await _controller.SetWebhook(request);

        result.Should().BeOfType<OkObjectResult>();
        _mockService.Verify(s => s.SetWebhookAsync(request), Times.Once);
    }
}
