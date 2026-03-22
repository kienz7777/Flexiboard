using FlexiBoard.Domain.Entities;

namespace FlexiBoard.Application.Interfaces;

public interface IOrderGenerator
{
    Task<List<Order>> GenerateOrdersAsync(List<Product> products, List<User> users);
}
