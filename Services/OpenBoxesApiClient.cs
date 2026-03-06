using System.Net;
using System.Net.Http.Json;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using OpenBoxesMobile.Blazor.Models.Auth;
using OpenBoxesMobile.Blazor.Models.Location;
using OpenBoxesMobile.Blazor.Models.Picking;
using OpenBoxesMobile.Blazor.Models.Product;
using OpenBoxesMobile.Blazor.Options;

namespace OpenBoxesMobile.Blazor.Services;

public sealed class OpenBoxesApiClient : IDisposable
{
    private HttpClient? _client;
    private CookieContainer? _cookieContainer;
    private string _baseUrl = string.Empty;
    private readonly bool _useProxy;
    private readonly DebugLogService _debug;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public OpenBoxesApiClient(IOptions<OpenBoxesApiOptions> options, DebugLogService debug)
    {
        _debug = debug;
        var apiOptions = options.Value;
        _useProxy = apiOptions.UseProxy;
        if (!string.IsNullOrWhiteSpace(apiOptions.BaseUrl))
        {
            SetBaseUrl(apiOptions.BaseUrl);
        }
    }

    public string BaseUrl => _baseUrl;

    public async Task<bool> CanReachServerAsync(CancellationToken cancellationToken = default)
    {
        if (_client is null)
        {
            return false;
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, string.Empty);
            using var response = await _client.SendAsync(request, cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void SetBaseUrl(string baseUrl)
    {
        var normalized = NormalizeBaseUrl(baseUrl)
            ?? throw new InvalidOperationException("API base URL is invalid. Use an absolute http(s) URL.");

        if (string.Equals(_baseUrl, normalized, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _baseUrl = normalized;

        _client?.Dispose();
        _cookieContainer = new CookieContainer();
        _client = BuildClient(_baseUrl, _cookieContainer, _useProxy);
        _debug.Info("ApiClient", $"Base URL set to {_baseUrl} (UseProxy={_useProxy})");
    }

    public async Task<Session?> GetSessionAsync(CancellationToken cancellationToken = default)
    {
        return await SendAsync<Session>(HttpMethod.Get, "getAppContext", null, cancellationToken);
    }

    public async Task LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var request = new LoginRequest
        {
            Username = username,
            Password = password
        };

        await SendAsync<object>(HttpMethod.Post, "login", request, cancellationToken);
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        await SendAsync<object>(HttpMethod.Post, "logout", new { }, cancellationToken);
    }

    public async Task<IReadOnlyList<Location>> GetLocationsAsync(CancellationToken cancellationToken = default)
    {
        return await SendAsync<List<Location>>(
            HttpMethod.Get,
            "locations?locationTypeCode=DEPOT&activityCodes=MANAGE_INVENTORY&applyUserFilter=true",
            null,
            cancellationToken) ?? [];
    }

    public async Task SetCurrentLocationAsync(string locationId, CancellationToken cancellationToken = default)
    {
        await SendAsync<object>(HttpMethod.Put, $"chooseLocation/{Uri.EscapeDataString(locationId)}", null, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        return await SendAsync<List<Product>>(HttpMethod.Get, "generic/product", null, cancellationToken) ?? [];
    }

    public async Task<IReadOnlyList<Product>> SearchProductsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var request = new ProductSearchRequest
        {
            SearchAttributes =
            [
                new ProductSearchRequest.SearchAttribute
                {
                    Property = "name",
                    Operator = "like",
                    Value = $"{name}%"
                }
            ]
        };

        return await SendAsync<List<Product>>(HttpMethod.Post, "mobile/products/search", request, cancellationToken) ?? [];
    }

    public async Task<IReadOnlyList<Product>> SearchProductByCodeAsync(string productCode, CancellationToken cancellationToken = default)
    {
        var request = new ProductSearchRequest
        {
            SearchAttributes =
            [
                new ProductSearchRequest.SearchAttribute
                {
                    Property = "productCode",
                    Operator = "like",
                    Value = $"{productCode}%"
                }
            ]
        };

        return await SendAsync<List<Product>>(HttpMethod.Post, "generic/product/search", request, cancellationToken) ?? [];
    }

    public Task<JsonNode?> SearchProductGloballyAsync(string value, CancellationToken cancellationToken = default)
    {
        return SendJsonAsync(HttpMethod.Post, "mobile/products/search", new { value }, cancellationToken);
    }

    public Task<JsonNode?> GetProductByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return SendJsonAsync(HttpMethod.Get, $"mobile/products/{Uri.EscapeDataString(id)}/details", null, cancellationToken);
    }

    public Task<JsonNode?> UpdateProductIdentifierAsync(
        string id,
        string type,
        string value,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            identifier = new
            {
                type,
                value
            }
        };

        return SendJsonAsync(HttpMethod.Put, $"mobile/products/{Uri.EscapeDataString(id)}/identifiers", payload, cancellationToken);
    }

    public async Task<IReadOnlyList<PickTask>> GetPickTasksAsync(
        string facilityId,
        string deliveryTypeCode,
        int ordersCount,
        CancellationToken cancellationToken = default)
    {
        var endpoint =
            $"facilities/{Uri.EscapeDataString(facilityId)}/pick-tasks?deliveryTypeCode={Uri.EscapeDataString(deliveryTypeCode)}&ordersCount={ordersCount}";

        return await SendAsync<List<PickTask>>(HttpMethod.Get, endpoint, null, cancellationToken) ?? [];
    }

    public Task<JsonNode?> GetPutawaysAsync(string? query = null, CancellationToken cancellationToken = default)
    {
        var endpoint = string.IsNullOrWhiteSpace(query)
            ? "mobile/putaways"
            : $"mobile/putaways?q={Uri.EscapeDataString(query)}";
        return SendJsonAsync(HttpMethod.Get, endpoint, null, cancellationToken);
    }

    public Task<JsonNode?> GetPutawayCandidatesAsync(string locationId, CancellationToken cancellationToken = default)
    {
        return SendJsonAsync(HttpMethod.Get, $"locations/{Uri.EscapeDataString(locationId)}/putawayCandidates", null, cancellationToken);
    }

    public Task<JsonNode?> GetProductByBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
    {
        return SendJsonAsync(HttpMethod.Get, $"barcodes?id={Uri.EscapeDataString(barcode)}", null, cancellationToken);
    }

    public Task<JsonNode?> GetOrdersPickingAsync(string originId, string? identifier = null, CancellationToken cancellationToken = default)
    {
        var endpoint =
            "stockMovements?exclude=lineItems&direction=OUTBOUND&requisitionStatusCode=PICKING&sort=expectedShippingDate&order=asc"
            + $"&origin={Uri.EscapeDataString(originId)}";

        if (!string.IsNullOrWhiteSpace(identifier))
        {
            endpoint += $"&identifier={Uri.EscapeDataString(identifier)}";
        }

        return SendJsonAsync(HttpMethod.Get, endpoint, null, cancellationToken);
    }

    public Task<JsonNode?> GetStockMovementByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return SendJsonAsync(HttpMethod.Get, $"stockMovements/{Uri.EscapeDataString(id)}", null, cancellationToken);
    }

    public Task<JsonNode?> GetPickListAsync(string id, CancellationToken cancellationToken = default)
    {
        return SendJsonAsync(HttpMethod.Get, $"picklists/{Uri.EscapeDataString(id)}", null, cancellationToken);
    }

    public Task<JsonNode?> SubmitPickListItemAsync(string id, object payload, CancellationToken cancellationToken = default)
    {
        return SendJsonAsync(HttpMethod.Post, $"picklistItems/{Uri.EscapeDataString(id)}", payload, cancellationToken);
    }

    public Task<JsonNode?> GetPickupAllocationOrdersAsync(string originId, CancellationToken cancellationToken = default)
    {
        var endpoint =
            "stockMovements?exclude=lineItems&direction=OUTBOUND&requisitionStatusCode=VERIFYING&sort=dateCreated&order=asc&deliveryTypeCode=PICK_UP"
            + $"&origin={Uri.EscapeDataString(originId)}";

        return SendJsonAsync(HttpMethod.Get, endpoint, null, cancellationToken);
    }

    public Task<JsonNode?> GetPutawayTasksAsync(string facilityId, string productId, CancellationToken cancellationToken = default)
    {
        return SendJsonAsync(
            HttpMethod.Get,
            $"facilities/{Uri.EscapeDataString(facilityId)}/putaway-tasks?statusCategory=OPEN&product={Uri.EscapeDataString(productId)}",
            null,
            cancellationToken);
    }

    public Task<JsonNode?> PatchPutawayTaskAsync(string facilityId, string taskId, object payload, CancellationToken cancellationToken = default)
    {
        return SendJsonAsync(
            HttpMethod.Patch,
            $"facilities/{Uri.EscapeDataString(facilityId)}/putaway-tasks/{Uri.EscapeDataString(taskId)}",
            payload,
            cancellationToken);
    }

    public Task<JsonNode?> GetPutawayDetailsByContainerAsync(string facilityId, string containerId, CancellationToken cancellationToken = default)
    {
        return SendJsonAsync(
            HttpMethod.Get,
            $"facilities/{Uri.EscapeDataString(facilityId)}/putaway-tasks?statusCategory=OPEN&container={Uri.EscapeDataString(containerId)}",
            null,
            cancellationToken);
    }

    public Task<JsonNode?> GetAlternativeDestinationsAsync(string facilityId, string taskId, CancellationToken cancellationToken = default)
    {
        return SendJsonAsync(
            HttpMethod.Get,
            $"facilities/{Uri.EscapeDataString(facilityId)}/putaway-tasks/{Uri.EscapeDataString(taskId)}/alternate-destinations",
            null,
            cancellationToken);
    }

    public Task<JsonNode?> GetShipmentsReadyToPackAsync(string locationId, string shipmentStatusCode, CancellationToken cancellationToken = default)
    {
        var endpoint =
            $"shipments?origin={Uri.EscapeDataString(locationId)}&shipmentStatusCode={Uri.EscapeDataString(shipmentStatusCode)}"
            + "&requisitionStatus=PICKED&requisitionStatus=CHECKING";

        return SendJsonAsync(HttpMethod.Get, endpoint, null, cancellationToken);
    }

    public Task<JsonNode?> GetInboundShipmentsAsync(string destinationId, CancellationToken cancellationToken = default)
    {
        return SendJsonAsync(HttpMethod.Get, $"shipments?destination={Uri.EscapeDataString(destinationId)}", null, cancellationToken);
    }

    public Task<JsonNode?> GetPartialReceivingAsync(string shipmentId, CancellationToken cancellationToken = default)
    {
        return SendJsonAsync(HttpMethod.Get, $"partialReceiving/{Uri.EscapeDataString(shipmentId)}", null, cancellationToken);
    }

    public Task<JsonNode?> SubmitPartialReceivingAsync(string shipmentId, object payload, CancellationToken cancellationToken = default)
    {
        return SendJsonAsync(HttpMethod.Post, $"partialReceiving/{Uri.EscapeDataString(shipmentId)}", payload, cancellationToken);
    }

    public Task<JsonNode?> CreateReceivingBinLocationAsync(string shipmentId, CancellationToken cancellationToken = default)
    {
        return SendJsonAsync(HttpMethod.Post, $"partialReceiving/{Uri.EscapeDataString(shipmentId)}/receivingBinLocation", new { }, cancellationToken);
    }

    public Task<JsonNode?> GetShipmentAsync(string id, CancellationToken cancellationToken = default)
    {
        return SendJsonAsync(HttpMethod.Get, $"shipments/{Uri.EscapeDataString(id)}", null, cancellationToken);
    }

    public Task<JsonNode?> UpdateContainerStatusAsync(string id, object payload, CancellationToken cancellationToken = default)
    {
        return SendJsonAsync(HttpMethod.Post, $"containers/{Uri.EscapeDataString(id)}/status", payload, cancellationToken);
    }

    public Task<JsonNode?> SearchInternalLocationsAsync(
        string searchTerm,
        string parentLocationId,
        int max = 25,
        int offset = 0,
        CancellationToken cancellationToken = default)
    {
        var endpoint =
            $"internalLocations/search?searchTerm={Uri.EscapeDataString(searchTerm)}&parentLocation.id={Uri.EscapeDataString(parentLocationId)}&max={max}&offset={offset}";
        return SendJsonAsync(HttpMethod.Get, endpoint, null, cancellationToken);
    }

    public Task<JsonNode?> GetStockMovementsAsync(string direction, string status, string? originId = null, CancellationToken cancellationToken = default)
    {
        var endpoint = $"stockMovements?direction={Uri.EscapeDataString(direction)}&status={Uri.EscapeDataString(status)}";
        if (!string.IsNullOrWhiteSpace(originId))
        {
            endpoint += $"&origin={Uri.EscapeDataString(originId)}";
        }

        return SendJsonAsync(HttpMethod.Get, endpoint, null, cancellationToken);
    }

    public Task<JsonNode?> GetStockTransfersAsync(CancellationToken cancellationToken = default)
    {
        return SendJsonAsync(HttpMethod.Get, "stockTransfers", null, cancellationToken);
    }

    public Task<JsonNode?> GetLocationProductSummaryAsync(string locationId, CancellationToken cancellationToken = default)
    {
        return SendJsonAsync(HttpMethod.Get, $"locations/{Uri.EscapeDataString(locationId)}/productSummary", null, cancellationToken);
    }

    public Task<JsonNode?> SearchBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
    {
        return SendJsonAsync(HttpMethod.Get, $"globalSearch/{Uri.EscapeDataString(barcode)}", null, cancellationToken);
    }

    public Task<JsonNode?> GetContainersAsync(CancellationToken cancellationToken = default)
    {
        return SendJsonAsync(HttpMethod.Get, "containers", null, cancellationToken);
    }

    public Task<JsonNode?> CreateLpnAsync(object payload, CancellationToken cancellationToken = default)
    {
        return SendJsonAsync(HttpMethod.Post, "containers", payload, cancellationToken);
    }

    public Task<JsonNode?> GetReasonCodesByActivityAsync(string activityCode, CancellationToken cancellationToken = default)
    {
        return SendJsonAsync(HttpMethod.Get, $"reason-codes?activityCode={Uri.EscapeDataString(activityCode)}", null, cancellationToken);
    }

    private async Task<T?> SendAsync<T>(
        HttpMethod method,
        string endpoint,
        object? payload,
        CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        _debug.Info("HTTP", $"{method} {endpoint}");
        using var request = new HttpRequestMessage(method, endpoint);

        if (payload is not null)
        {
            request.Content = JsonContent.Create(payload);
        }

        HttpResponseMessage response;
        try
        {
            response = await GetClient().SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _debug.Error("HTTP", $"{method} {endpoint} failed in {sw.ElapsedMilliseconds}ms: {ex.Message}");
            throw BuildNetworkError(ex, endpoint);
        }
        using (response)
        {
            _debug.Info("HTTP", $"{method} {endpoint} -> {(int)response.StatusCode} in {sw.ElapsedMilliseconds}ms");
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                var message = string.IsNullOrWhiteSpace(body)
                    ? $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}"
                    : body;

                throw new InvalidOperationException(message);
            }

            if (typeof(T) == typeof(object) || response.Content.Headers.ContentLength == 0)
            {
                return default;
            }

            var text = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(text))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(text, _jsonOptions);
        }
    }

    private async Task<JsonNode?> SendJsonAsync(
        HttpMethod method,
        string endpoint,
        object? payload,
        CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        _debug.Info("HTTP", $"{method} {endpoint}");
        using var request = new HttpRequestMessage(method, endpoint);

        if (payload is not null)
        {
            request.Content = JsonContent.Create(payload);
        }

        HttpResponseMessage response;
        try
        {
            response = await GetClient().SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _debug.Error("HTTP", $"{method} {endpoint} failed in {sw.ElapsedMilliseconds}ms: {ex.Message}");
            throw BuildNetworkError(ex, endpoint);
        }
        using (response)
        {
            _debug.Info("HTTP", $"{method} {endpoint} -> {(int)response.StatusCode} in {sw.ElapsedMilliseconds}ms");
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                var message = string.IsNullOrWhiteSpace(body)
                    ? $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}"
                    : body;

                throw new InvalidOperationException(message);
            }

            var text = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            return JsonNode.Parse(text);
        }
    }

    public void Dispose()
    {
        _client?.Dispose();
    }

    private static HttpClient BuildClient(string baseUrl, CookieContainer cookieContainer, bool useProxy)
    {
        var handler = new HttpClientHandler
        {
            CookieContainer = cookieContainer,
            UseCookies = true,
            AllowAutoRedirect = true,
            UseProxy = useProxy
        };

        return new HttpClient(handler)
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(20)
        };
    }

    private InvalidOperationException BuildNetworkError(Exception ex, string endpoint)
    {
        var url = string.IsNullOrWhiteSpace(_baseUrl)
            ? endpoint
            : new Uri(new Uri(_baseUrl), endpoint).ToString();
        return ex switch
        {
            TaskCanceledException => new InvalidOperationException($"Request timeout while calling {url}", ex),
            HttpRequestException => new InvalidOperationException($"Network error while calling {url}: {ex.Message}", ex),
            _ => new InvalidOperationException($"Request failed while calling {url}: {ex.Message}", ex)
        };
    }

    private HttpClient GetClient()
    {
        if (_client is not null)
        {
            return _client;
        }

        const string message = "API base URL is not configured. Set it in Settings before using the app.";
        _debug.Error("ApiClient", message);
        throw new InvalidOperationException(message);
    }

    private static string? NormalizeBaseUrl(string value)
    {
        var trimmed = value.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return null;
        }

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
        {
            return null;
        }

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            return null;
        }

        return trimmed.TrimEnd('/') + '/';
    }
}
