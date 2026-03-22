using FlexiBoard.Domain.Entities;
using FlexiBoard.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace FlexiBoard.Infrastructure.DataGenerators;

public class OrderGenerator : IOrderGenerator
{
    private readonly Random _random = new();
    private readonly ILogger<OrderGenerator> _logger;

    public OrderGenerator(ILogger<OrderGenerator> logger)
    {
        _logger = logger;
    }

    public async Task<List<Order>> GenerateOrdersAsync(List<Product> products, List<User> users)
    {
        return await Task.Run(() =>
        {
            _logger.LogInformation("Generating orders");
            var orders = new List<Order>();
            var ordersCount = _random.Next(50, 150);

            for (int i = 0; i < ordersCount; i++)
            {
                var userId = users[_random.Next(users.Count)].Id;
                var orderItemsCount = _random.Next(1, 5);
                var orderItems = new List<OrderItem>();
                decimal total = 0;

                for (int j = 0; j < orderItemsCount; j++)
                {
                    var product = products[_random.Next(products.Count)];
                    var quantity = _random.Next(1, 5);
                    var orderItem = new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = quantity,
                        Price = product.Price
                    };
                    orderItems.Add(orderItem);
                    total += orderItem.Total;
                }

                var daysAgo = _random.Next(0, 30);
                var order = new Order
                {
                    Id = i + 1,
                    UserId = userId,
                    Date = DateTime.UtcNow.AddDays(-daysAgo).AddHours(_random.Next(0, 24)),
                    Products = orderItems,
                    Total = total
                };

                orders.Add(order);
            }

            _logger.LogInformation("Generated {Count} orders", orders.Count);
            return orders;
        });
    }
}
