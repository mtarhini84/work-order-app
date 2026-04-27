using System.Net;
using System.Net.Http.Json;
using WorkOrderApp.Controllers;
using WorkOrderApp.Tests.Infrastructure;

namespace WorkOrderApp.Tests.Controllers
{
    public class UserControllerTests : IClassFixture<TestWebAppFactory>
    {
        private readonly HttpClient _client;
        private readonly TestWebAppFactory _factory;

        public UserControllerTests(TestWebAppFactory factory)
        {
            _factory = factory;
            _client  = factory.CreateClient();
        }

        // ── POST /api/user/login ──────────────────────────────────────────────

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOkWithToken()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await _client.PostAsJsonAsync("/api/user/login", new LoginModel
            {
                Identifier = "seed.admin@test.com",
                Password   = "Test1234",
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<ApiWrapper>();
            Assert.True(body?.Success);
            Assert.NotNull(body?.Token);
        }

        [Fact]
        public async Task Login_WithWrongPassword_ReturnsBadRequest()
        {
            await DbHelper.SeedAsync(_factory.Services);

            var response = await _client.PostAsJsonAsync("/api/user/login", new LoginModel
            {
                Identifier = "seed.admin@test.com",
                Password   = "wrong",
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithUnknownEmail_ReturnsBadRequest()
        {
            var response = await _client.PostAsJsonAsync("/api/user/login", new LoginModel
            {
                Identifier = "nobody@nowhere.com",
                Password   = "Test1234",
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ── POST /api/user/create ─────────────────────────────────────────────

        [Fact]
        public async Task CreateUser_WithValidData_ReturnsOk()
        {
            var response = await _client.PostAsJsonAsync("/api/user/create", new CreateUserDto
            {
                Name  = "New User",
                Email = $"newuser_{Guid.NewGuid():N}@test.com",
                Password = "New1234",
                Role  = "Customer",
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CreateUser_WithDuplicateEmail_ReturnsBadRequest()
        {
            var email = $"dup_{Guid.NewGuid():N}@test.com";

            await _client.PostAsJsonAsync("/api/user/create", new CreateUserDto
            {
                Name = "First", Email = email, Password = "Test1234", Role = "Customer",
            });

            var response = await _client.PostAsJsonAsync("/api/user/create", new CreateUserDto
            {
                Name = "Second", Email = email, Password = "Test1234", Role = "Customer",
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ── GET /api/user/get/{id} ────────────────────────────────────────────

        [Fact]
        public async Task GetById_AsAdmin_ReturnsUser()
        {
            var seed  = await DbHelper.SeedAsync(_factory.Services);
            var token = TokenHelper.GenerateToken(seed.AdminId, "Admin");
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync($"/api/user/get/{seed.CustomerId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetById_WithoutToken_ReturnsUnauthorized()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);
            var fresh = _factory.CreateClient(); // no auth header

            var response = await fresh.GetAsync($"/api/user/get/{seed.AdminId}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetById_NonExistent_ReturnsBadRequest()
        {
            var seed  = await DbHelper.SeedAsync(_factory.Services);
            var token = TokenHelper.GenerateToken(seed.AdminId, "Admin");
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/user/get/nonexistent-id");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ── GET /api/user/all ─────────────────────────────────────────────────

        [Fact]
        public async Task GetAll_AsAdmin_ReturnsOk()
        {
            var seed  = await DbHelper.SeedAsync(_factory.Services);
            var token = TokenHelper.GenerateToken(seed.AdminId, "Admin");
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/user/all");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetAll_AsOperator_ReturnsForbidden()
        {
            var seed  = await DbHelper.SeedAsync(_factory.Services);
            var token = TokenHelper.GenerateToken(seed.OperatorId, "Operator");
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("/api/user/all");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }

    // ── Minimal response wrapper for assertions ───────────────────────────────
    file record ApiWrapper(bool Success, string? Token, object? Data);
}
