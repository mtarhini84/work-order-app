using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WorkOrderApp.Controllers;
using WorkOrderApp.Tests.Infrastructure;

namespace WorkOrderApp.Tests.Controllers
{
    public class LocationControllerTests : IClassFixture<TestWebAppFactory>
    {
        private readonly TestWebAppFactory _factory;

        public LocationControllerTests(TestWebAppFactory factory)
        {
            _factory = factory;
        }

        private HttpClient AdminClient(string adminId)
        {
            var c = _factory.CreateClient();
            c.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", TokenHelper.GenerateToken(adminId, "Admin"));
            return c;
        }

        private HttpClient OperatorClient(string operatorId)
        {
            var c = _factory.CreateClient();
            c.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", TokenHelper.GenerateToken(operatorId, "Operator"));
            return c;
        }

        private HttpClient CustomerClient(string customerId)
        {
            var c = _factory.CreateClient();
            c.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", TokenHelper.GenerateToken(customerId, "Customer"));
            return c;
        }

        // ── POST /api/location/create ─────────────────────────────────────────

        [Fact]
        public async Task Create_AsOperator_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await OperatorClient(seed.OperatorId)
                .PostAsJsonAsync("/api/location/create", new CreateLocationDto
                {
                    Name    = "New Lab",
                    Address = "99 Science Park",
                });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Create_AsCustomer_ReturnsForbidden()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await CustomerClient(seed.CustomerId)
                .PostAsJsonAsync("/api/location/create", new CreateLocationDto
                {
                    Name    = "Unauthorized",
                    Address = "Somewhere",
                });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Create_Unauthenticated_ReturnsUnauthorized()
        {
            var response = await _factory.CreateClient()
                .PostAsJsonAsync("/api/location/create", new CreateLocationDto
                {
                    Name    = "No Auth",
                    Address = "Nowhere",
                });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ── GET /api/location/get/{id} ────────────────────────────────────────

        [Fact]
        public async Task GetById_AsAnyAuthenticatedUser_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await CustomerClient(seed.CustomerId)
                .GetAsync($"/api/location/get/{seed.LocationId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetById_NonExistent_ReturnsBadRequest()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await CustomerClient(seed.CustomerId)
                .GetAsync("/api/location/get/does-not-exist");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ── GET /api/location/all ─────────────────────────────────────────────

        [Fact]
        public async Task GetAll_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await AdminClient(seed.AdminId).GetAsync("/api/location/all");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ── POST /api/location/update ─────────────────────────────────────────

        [Fact]
        public async Task Update_AsOperator_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await OperatorClient(seed.OperatorId)
                .PostAsJsonAsync("/api/location/update", new UpdateLocationDto
                {
                    Id      = seed.LocationId,
                    Name    = "Updated Name",
                    Address = "Updated Address",
                });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ── POST /api/location/assign-user ────────────────────────────────────

        [Fact]
        public async Task AssignUser_AsOperator_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await OperatorClient(seed.OperatorId)
                .PostAsJsonAsync("/api/location/assign-user", new AssignUserToLocationDto
                {
                    UserId     = seed.ExecutorId,
                    LocationId = seed.LocationId,
                });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task RemoveUser_AfterAssign_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);
            var client = OperatorClient(seed.OperatorId);

            var dto = new AssignUserToLocationDto { UserId = seed.ExecutorId, LocationId = seed.LocationId };
            await client.PostAsJsonAsync("/api/location/assign-user", dto);

            var response = await client.PostAsJsonAsync("/api/location/remove-user", dto);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ── GET /api/location/{id}/users ──────────────────────────────────────

        [Fact]
        public async Task GetUsers_AsOperator_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await OperatorClient(seed.OperatorId)
                .GetAsync($"/api/location/{seed.LocationId}/users");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ── GET /api/location/my ──────────────────────────────────────────────

        [Fact]
        public async Task GetMyLocations_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await CustomerClient(seed.CustomerId)
                .GetAsync("/api/location/my");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ── PATCH /api/location/activate & deactivate ─────────────────────────

        [Fact]
        public async Task Deactivate_AsAdmin_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await AdminClient(seed.AdminId)
                .PatchAsync($"/api/location/deactivate?id={seed.LocationId}", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Activate_AsAdmin_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);
            var admin = AdminClient(seed.AdminId);
            await admin.PatchAsync($"/api/location/deactivate?id={seed.LocationId}", null);

            var response = await admin.PatchAsync($"/api/location/activate?id={seed.LocationId}", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
