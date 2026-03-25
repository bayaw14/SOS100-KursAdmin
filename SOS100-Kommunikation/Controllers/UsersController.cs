using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using SOS100_Kommunikation.Models; // User model here acts as DTO

namespace SOS100_Kommunikation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;

    public UsersController(IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _config = config;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<User>>> SearchUsers(string? q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Ok(new List<User>());
        }

        try
        {
            var baseUrl = _config["AuthApiUrl"] ?? "https://app-sos100-inloggning.azurewebsites.net";
            var apiKey = _config["AuthApiKey"] ?? "MIN_HEMLIGA_API_NYCKEL_12345";
            
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            
            var url = $"{baseUrl.TrimEnd('/')}/api/auth/search-users?q={Uri.EscapeDataString(q)}";
            
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<List<User>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return Ok(users ?? new List<User>());
            }
            else
            {
                var err = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, new { Error = $"Inloggning-API returnerade fel: {err}" });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                Message = "Kunde inte ansluta till Inloggning-API (Autentiseringsdatabasen).", 
                Error = ex.Message
            });
        }
    }
}
