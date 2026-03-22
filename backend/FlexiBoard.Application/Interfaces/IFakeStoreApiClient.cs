using FlexiBoard.Domain.Entities;

namespace FlexiBoard.Application.Interfaces;

public interface IFakeStoreApiClient
{
    Task<List<Product>> GetProductsAsync();
    Task<List<User>> GetUsersAsync();
}
