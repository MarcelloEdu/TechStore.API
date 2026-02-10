using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStore.Application.DTOs.Orders;
using TechStore.Domain.Entities;
using TechStore.Infrastructure.Data;

namespace TechStore.Controllers;

[ApiController]
[Route("api/orders")]
public class OrderController : ControllerBase
{
    private readonly AppDbContext _context;

    public OrderController(AppDbContext context)
    {
        _context = context;
    }

    // ===== Registrar Venda =====
    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
    {
        if (request.Items == null || !request.Items.Any())
            return BadRequest("O pedido deve conter ao menos um item.");

        var productIds = request.Items
            .Select(i => i.ProductId)
            .ToList();

        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .ToListAsync();

        if (products.Count != productIds.Count)
            return BadRequest("Um ou mais produtos são inválidos ou estão inativos.");

        // ===== Validação de estoque (#12) =====
        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
                return BadRequest("A quantidade deve ser maior que zero.");

            var product = products.First(p => p.Id == item.ProductId);

            if (product.StockQuantity < item.Quantity)
            {
                return BadRequest(
                    $"Estoque insuficiente para o produto '{product.Name}'. " +
                    $"Disponível: {product.StockQuantity}, solicitado: {item.Quantity}"
                );
            }
        }

        // ===== Criar itens do pedido =====
        var orderItems = request.Items.Select(item =>
        {
            var product = products.First(p => p.Id == item.ProductId);

            return new OrderItem(
                product.Id,
                item.Quantity,
                product.Price
            );
        }).ToList();

        var order = new Order(orderItems);

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetOrderById),
            new { id = order.Id },
            new
            {
                order.Id,
                order.Status,
                order.TotalAmount,
                order.CreatedAt
            }
        );
    }

    // ===== Buscar Pedido por ID =====
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOrderById(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound();

        return Ok(new
        {
            order.Id,
            order.Status,
            order.TotalAmount,
            order.CreatedAt,
            Items = order.Items.Select(i => new
            {
                i.ProductId,
                ProductName = i.Product.Name,
                i.Quantity,
                i.UnitPrice,
                i.Subtotal
            })
        });
    }

    // ===== Confirmar Pedido =====
    //após uma compra ser finalizada, após sua confirmação o estoque do produto deve ser atualizado
    
    [HttpPut("{id:int}/confirm")]
    public async Task<IActionResult> ConfirmOrder(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound("Pedido não encontrado.");

        if (order.Status != Domain.Enums.OrderStatus.Pending)
            return BadRequest("Apenas pedidos pendentes podem ser confirmados.");

        // ===== Validação final de estoque =====
        foreach (var item in order.Items)
        {
            if (item.Product.StockQuantity < item.Quantity)
            {
                return BadRequest(
                    $"Estoque insuficiente para o produto '{item.Product.Name}'."
                );
            }
        }

        // ===== Subtração de estoque =====
        foreach (var item in order.Items)
        {
            item.Product.DecreaseStock(item.Quantity);
        }

        order.Confirm();

        await _context.SaveChangesAsync();

        return NoContent();
    }
}