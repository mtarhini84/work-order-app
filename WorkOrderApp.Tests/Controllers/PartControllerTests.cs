using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WorkOrderApp.Controllers;
using WorkOrderApp.Tests.Infrastructure;

namespace WorkOrderApp.Tests.Controllers
{
    public class PartControllerTests : IClassFixture<TestWebAppFactory>
    {
        private readonly TestWebAppFactory _factory;

        public PartControllerTests(TestWebAppFactory factory)
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

        // ── POST /api/part/create ─────────────────────────────────────────────

        [Fact]
        public async Task Create_AsExecutor_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.ExecutorId, "Executor")
                .PostAsJsonAsync("/api/part/create", new CreatePartDto
                {
                    WorkOrderId = seed.WorkOrderId,
                    UserId      = seed.ExecutorId,
                    Name        = "Bolt M10",
                    UnitCost    = 2.5m,
                    Count       = 10,
                    QRCode      = $"QR-TEST-{Guid.NewGuid():N}",
                });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Create_AsCustomer_ReturnsForbidden()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.CustomerId, "Customer")
                .PostAsJsonAsync("/api/part/create", new CreatePartDto
                {
                    WorkOrderId = seed.WorkOrderId,
                    UserId      = seed.CustomerId,
                    Name        = "Not allowed",
                    UnitCost    = 1m,
                    Count       = 1,
                });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // ── GET /api/part/get/{id} ────────────────────────────────────────────

        [Fact]
        public async Task GetById_AsAuthUser_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.OperatorId, "Operator")
                .GetAsync($"/api/part/get/{seed.PartId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetById_NonExistent_ReturnsBadRequest()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.AdminId, "Admin")
                .GetAsync("/api/part/get/no-such-part");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ── GET /api/part/qr/{qrCode} ─────────────────────────────────────────

        [Fact]
        public async Task GetByQRCode_Existing_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.ExecutorId, "Executor")
                .GetAsync("/api/part/qr/QR-TEST-001");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetByQRCode_NonExistent_ReturnsBadRequest()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.ExecutorId, "Executor")
                .GetAsync("/api/part/qr/QR-UNKNOWN");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ── POST /api/part/update ─────────────────────────────────────────────

        [Fact]
        public async Task Update_AsExecutor_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.ExecutorId, "Executor")
                .PostAsJsonAsync("/api/part/update", new UpdatePartDto
                {
                    Id    = seed.PartId,
                    Count = 5,
                });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ── DELETE /api/part/delete/{id} ──────────────────────────────────────

        [Fact]
        public async Task Delete_AsAdmin_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.AdminId, "Admin")
                .DeleteAsync($"/api/part/delete/{seed.PartId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Delete_AsCustomer_ReturnsForbidden()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.CustomerId, "Customer")
                .DeleteAsync($"/api/part/delete/{seed.PartId}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // ── PATCH /api/part/deactivate ────────────────────────────────────────

        [Fact]
        public async Task Deactivate_AsAdmin_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.AdminId, "Admin")
                .PatchAsync($"/api/part/deactivate?id={seed.PartId}", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
