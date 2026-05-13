using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CCS.Api.Admin.Tests;

public sealed class AdminApiTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Health_ReturnsHealthyServiceName()
    {
        var client = factory.CreateClient();

        using var response = await client.GetAsync("/health");
        var body = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("healthy", body["status"]!.GetValue<string>());
        Assert.Equal("CCS.Api.Admin", body["service"]!.GetValue<string>());
    }

    [Fact]
    public async Task AdminWorkflow_CreatesOwnerVehicleAndRule()
    {
        var client = factory.CreateClient();

        using var ownerResponse = await client.PostAsync("/owners", Json("""
            {
              "documentType": "CC",
              "documentNumber": "123456789",
              "fullName": "Propietario Demo",
              "email": "owner@example.com",
              "phoneE164": "+573001111111"
            }
            """));
        var owner = JsonNode.Parse(await ownerResponse.Content.ReadAsStringAsync())!;
        var ownerId = owner["ownerId"]!.GetValue<Guid>();

        using var vehicleResponse = await client.PostAsync("/vehicles", Json($$"""
            {
              "ownerId": "{{ownerId}}",
              "vehicleType": "Truck",
              "plate": "ABC123",
              "deviceId": "DEV-SMOKE"
            }
            """));
        var vehicle = JsonNode.Parse(await vehicleResponse.Content.ReadAsStringAsync())!;
        var vehicleId = vehicle["vehicleId"]!.GetValue<Guid>();

        using var ruleResponse = await client.PostAsync("/rules", Json($$"""
            {
              "ownerId": "{{ownerId}}",
              "vehicleId": "{{vehicleId}}",
              "name": "Panico - propietario y autoridad",
              "eventType": "Panic",
              "priority": 1,
              "isActive": true,
              "actions": [
                {
                  "actionType": "Sms",
                  "targetType": "Owner",
                  "targetReference": "+573001111111",
                  "order": 1,
                  "isCritical": true
                }
              ]
            }
            """));

        using var rulesResponse = await client.GetAsync($"/rules?vehicleId={vehicleId}&eventType=Panic");
        var rules = JsonNode.Parse(await rulesResponse.Content.ReadAsStringAsync())!;

        Assert.Equal(HttpStatusCode.Created, ownerResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, vehicleResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, ruleResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, rulesResponse.StatusCode);
        Assert.Single(rules["items"]!.AsArray());
    }

    [Fact]
    public async Task PostOwner_WhenRequiredFieldsAreMissing_ReturnsBadRequest()
    {
        var client = factory.CreateClient();

        using var response = await client.PostAsync("/owners", Json("""
            {
              "documentType": "CC",
              "documentNumber": "",
              "fullName": "",
              "email": null,
              "phoneE164": null
            }
            """));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetOwners_ReturnsCreatedOwners()
    {
        var client = factory.CreateClient();

        using var ownerResponse = await client.PostAsync("/owners", Json("""
            {
              "documentType": "CC",
              "documentNumber": "987654321",
              "fullName": "Propietario Consulta",
              "email": "consulta@example.com",
              "phoneE164": "+573002222222"
            }
            """));

        using var response = await client.GetAsync("/owners");
        var body = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;

        Assert.Equal(HttpStatusCode.Created, ownerResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(body["items"]!.AsArray(), item =>
            item?["documentNumber"]?.GetValue<string>() == "987654321");
    }

    [Fact]
    public async Task PostVehicle_WhenRequiredFieldsAreMissing_ReturnsBadRequest()
    {
        var client = factory.CreateClient();

        using var response = await client.PostAsync("/vehicles", Json("""
            {
              "ownerId": "00000000-0000-0000-0000-000000000000",
              "vehicleType": "Truck",
              "plate": "",
              "deviceId": ""
            }
            """));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostRule_WhenActionsAreMissing_ReturnsBadRequest()
    {
        var client = factory.CreateClient();

        using var response = await client.PostAsync("/rules", Json("""
            {
              "ownerId": "11111111-1111-1111-1111-111111111111",
              "vehicleId": null,
              "name": "Regla incompleta",
              "eventType": "Panic",
              "priority": 1,
              "isActive": true,
              "actions": []
            }
            """));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetVehicles_WhenNoVehiclesExist_ReturnsEmptyPage()
    {
        var client = factory.CreateClient();

        using var response = await client.GetAsync("/vehicles");
        var body = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(body["items"]!.AsArray());
        Assert.Null(body["nextCursor"]);
    }

    private static StringContent Json(string body)
    {
        return new StringContent(body, Encoding.UTF8, "application/json");
    }
}
