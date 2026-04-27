using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WorkOrderApp.Controllers;
using WorkOrderApp.Entities;
using WorkOrderApp.Tests.Infrastructure;

namespace WorkOrderApp.Tests.Controllers
{
    public class WorkOrderControllerTests : IClassFixture<TestWebAppFactory>
    {
        private readonly TestWebAppFactory _factory;

        public WorkOrderControllerTests(TestWebAppFactory factory)
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

        // ── POST /api/workorder/create ────────────────────────────────────────

        [Fact]
        public async Task Create_AsOperator_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.OperatorId, "Operator")
                .PostAsJsonAsync("/api/workorder/create", new CreateWorkOrderDto
                {
                    Title      = "New WO",
                    CustomerId = seed.CustomerId,
                    LocationId = seed.LocationId,
                    Priority   = Priority.Medium,
                });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Create_AsCustomer_ReturnsForbidden()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.CustomerId, "Customer")
                .PostAsJsonAsync("/api/workorder/create", new CreateWorkOrderDto
                {
                    Title      = "Not allowed",
                    CustomerId = seed.CustomerId,
                    LocationId = seed.LocationId,
                });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Create_Unauthenticated_ReturnsUnauthorized()
        {
            var response = await _factory.CreateClient()
                .PostAsJsonAsync("/api/workorder/create", new CreateWorkOrderDto
                {
                    Title      = "Anon",
                    CustomerId = Guid.NewGuid().ToString(),
                    LocationId = Guid.NewGuid().ToString(),
                });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ── GET /api/workorder/get/{id} ───────────────────────────────────────

        [Fact]
        public async Task GetById_AsAnyAuthUser_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.CustomerId, "Customer")
                .GetAsync($"/api/workorder/get/{seed.WorkOrderId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetById_NonExistent_ReturnsBadRequest()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.AdminId, "Admin")
                .GetAsync("/api/workorder/get/no-such-id");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ── GET /api/workorder/all ────────────────────────────────────────────

        [Fact]
        public async Task GetAll_AsOperator_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.OperatorId, "Operator")
                .GetAsync("/api/workorder/all");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetAll_AsCustomer_ReturnsForbidden()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.CustomerId, "Customer")
                .GetAsync("/api/workorder/all");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // ── POST /api/workorder/update ────────────────────────────────────────

        [Fact]
        public async Task Update_AsExecutor_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.ExecutorId, "Executor")
                .PostAsJsonAsync("/api/workorder/update", new UpdateWorkOrderDto
                {
                    Id          = seed.WorkOrderId,
                    Description = "Executor added notes",
                    Status      = WorkOrderStatus.InProgress,
                });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Update_AsCustomer_ReturnsForbidden()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.CustomerId, "Customer")
                .PostAsJsonAsync("/api/workorder/update", new UpdateWorkOrderDto
                {
                    Id     = seed.WorkOrderId,
                    Status = WorkOrderStatus.Done,
                });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // ── POST /api/workorder/assign ────────────────────────────────────────

        [Fact]
        public async Task Assign_AsOperator_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.OperatorId, "Operator")
                .PostAsJsonAsync("/api/workorder/assign", new AssignWorkOrderDto
                {
                    Id           = seed.WorkOrderId,
                    AssignedToId = seed.ExecutorId,
                });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Assign_AsExecutor_ReturnsForbidden()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.ExecutorId, "Executor")
                .PostAsJsonAsync("/api/workorder/assign", new AssignWorkOrderDto
                {
                    Id           = seed.WorkOrderId,
                    AssignedToId = seed.ExecutorId,
                });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // ── GET /api/workorder/my ─────────────────────────────────────────────

        [Fact]
        public async Task GetMine_AsExecutor_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.ExecutorId, "Executor")
                .GetAsync("/api/workorder/my");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ── GET /api/workorder/{id}/costs ─────────────────────────────────────

        [Fact]
        public async Task GetCosts_AsExecutor_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.ExecutorId, "Executor")
                .GetAsync($"/api/workorder/{seed.WorkOrderId}/costs");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ── GET /api/workorder/{id}/parts ─────────────────────────────────────

        [Fact]
        public async Task GetParts_AsExecutor_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.ExecutorId, "Executor")
                .GetAsync($"/api/workorder/{seed.WorkOrderId}/parts");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ── GET /api/workorder/{id}/attachments ───────────────────────────────

        [Fact]
        public async Task GetAttachments_AsOperator_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.OperatorId, "Operator")
                .GetAsync($"/api/workorder/{seed.WorkOrderId}/attachments");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ── GET /api/workorder/{id}/logs ──────────────────────────────────────

        [Fact]
        public async Task GetLogs_AsOperator_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.OperatorId, "Operator")
                .GetAsync($"/api/workorder/{seed.WorkOrderId}/logs");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ── PATCH /api/workorder/activate & deactivate ────────────────────────

        [Fact]
        public async Task Deactivate_AsAdmin_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.AdminId, "Admin")
                .PatchAsync($"/api/workorder/deactivate?id={seed.WorkOrderId}", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
