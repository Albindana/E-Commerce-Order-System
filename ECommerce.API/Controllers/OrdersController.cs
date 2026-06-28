using System.Security.Claims;
using ECommerce.Application.DTOs.Order;
using ECommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[Authorize]
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutDto dto)
    {
        var result = await _orderService.CheckoutAsync(UserId, dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetMyOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _orderService.GetUserOrdersAsync(UserId, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _orderService.GetByIdAsync(UserId, id);
        return Ok(result);
    }

    [HttpPut("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await _orderService.CancelOrderAsync(UserId, id);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _orderService.GetAllOrdersAsync(page, pageSize);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusDto dto)
    {
        var result = await _orderService.UpdateStatusAsync(id, dto);
        return Ok(result);
    }
}
