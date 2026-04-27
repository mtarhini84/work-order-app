using System.Net;
using System.Net.Http.Headers;
using WorkOrderApp.Controllers;
using WorkOrderApp.Tests.Infrastructure;

namespace WorkOrderApp.Tests.Controllers
{
    public class CostControllerTests : IClassFixture<TestWebAppFactory>
    {
        private readonly TestWebAppFactory _factory;

        public CostControllerTests(TestWebAppFactory factory)
        {
            _factory = factory;
        }

        private HttpClient Client(string userId, string role)
        {
            var c = _factory.CreateClient();
            c.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", TokenHelper.GenerateToken(userId, role));
            return c;
        }

        // ── POST /api/cost/create ─────────────────────────────────────────────

        [Fact]
        public async Task Create_AsExecutor_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.ExecutorId, "Executor")
                .PostAsJsonAsync("/api/cost/create", new CreateCostDto
                {
                    WorkOrderId = seed.WorkOrderId,
                    UserId      = seed.ExecutorId,
                    Name        = "Tool rental",
                    Amount      = 75m,
                    Category    = "Equipment",
                });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Create_AsCustomer_ReturnsForbidden()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.CustomerId, "Customer")
                .PostAsJsonAsync("/api/cost/create", new CreateCostDto
                {
                    WorkOrderId = seed.WorkOrderId,
                    UserId      = seed.CustomerId,
                    Name        = "Not allowed",
                    Amount      = 10m,
                    Category    = "Other",
                });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // ── GET /api/cost/get/{id} ────────────────────────────────────────────

        [Fact]
        public async Task GetById_AsOperator_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.OperatorId, "Operator")
                .GetAsync($"/api/cost/get/{seed.CostId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetById_NonExistent_ReturnsBadRequest()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.AdminId, "Admin")
                .GetAsync("/api/cost/get/bad-id");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ── POST /api/cost/update ─────────────────────────────────────────────

        [Fact]
        public async Task Update_AsExecutor_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.ExecutorId, "Executor")
                .PostAsJsonAsync("/api/cost/update", new UpdateCostDto
                {
                    Id     = seed.CostId,
                    Amount = 200m,
                });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ── DELETE /api/cost/delete/{id} ──────────────────────────────────────

        [Fact]
        public async Task Delete_AsAdmin_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.AdminId, "Admin")
                .DeleteAsync($"/api/cost/delete/{seed.CostId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Delete_AsCustomer_ReturnsForbidden()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.CustomerId, "Customer")
                .DeleteAsync($"/api/cost/delete/{seed.CostId}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // ── PATCH /api/cost/activate ──────────────────────────────────────────

        [Fact]
        public async Task Activate_AsAdmin_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);
            var admin = Client(seed.AdminId, "Admin");
            await admin.PatchAsync($"/api/cost/deactivate?id={seed.CostId}", null);

            var response = await admin.PatchAsync($"/api/cost/activate?id={seed.CostId}", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
