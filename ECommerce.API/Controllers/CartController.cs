using System.Security.Claims;
using ECommerce.Application.DTOs.Cart;
using ECommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[Authorize]
[ApiController]
[Route("api/cart")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var result = await _cartService.GetCartAsync(UserId);
        return Ok(result);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddCartItemDto dto)
    {
        var result = await _cartService.AddItemAsync(UserId, dto);
        return Ok(result);
    }

    [HttpPut("items/{itemId:guid}")]
    public async Task<IActionResult> UpdateItem(Guid itemId, [FromBody] UpdateCartItemDto dto)
    {
        var result = await _cartService.UpdateItemAsync(UserId, itemId, dto);
        return Ok(result);
    }

    [HttpDelete("items/{itemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid itemId)
    {
        await _cartService.RemoveItemAsync(UserId, itemId);
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        await _cartService.ClearCartAsync(UserId);
        return NoContent();
    }
}
