namespace FlexiBoard.Domain.Entities;

using System.Text.Json.Serialization;

public class Product
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
    
    [JsonPropertyName("price")]
    public decimal Price { get; set; }
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;
    
    [JsonPropertyName("image")]
    public string Image { get; set; } = string.Empty;
    
    [JsonPropertyName("rating")]
    public Rating? Rating { get; set; }
}

public class Rating
{
    [JsonPropertyName("rate")]
    public double Rate { get; set; }
    
    [JsonPropertyName("count")]
    public int Count { get; set; }
}
