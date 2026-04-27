using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WorkOrderApp.Controllers;
using WorkOrderApp.Entities;
using WorkOrderApp.Tests.Infrastructure;

namespace WorkOrderApp.Tests.Controllers
{
    public class RequestControllerTests : IClassFixture<TestWebAppFactory>
    {
        private readonly TestWebAppFactory _factory;

        public RequestControllerTests(TestWebAppFactory factory)
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

        // ── POST /api/request/create ──────────────────────────────────────────

        [Fact]
        public async Task Create_AsCustomer_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.CustomerId, "Customer")
                .PostAsJsonAsync("/api/request/create", new CreateRequestDto
                {
                    Title      = "Fix broken pipe",
                    LocationId = seed.LocationId,
                    Priority   = Priority.Medium,
                });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Create_AsOperator_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.OperatorId, "Operator")
                .PostAsJsonAsync("/api/request/create", new CreateRequestDto
                {
                    Title      = "Operator-created request",
                    LocationId = seed.LocationId,
                    Priority   = Priority.Low,
                });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Create_AsExecutor_ReturnsForbidden()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.ExecutorId, "Executor")
                .PostAsJsonAsync("/api/request/create", new CreateRequestDto
                {
                    Title      = "Not allowed",
                    LocationId = seed.LocationId,
                    Priority   = Priority.Low,
                });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Create_Unauthenticated_ReturnsUnauthorized()
        {
            var response = await _factory.CreateClient()
                .PostAsJsonAsync("/api/request/create", new CreateRequestDto
                {
                    Title      = "No auth",
                    LocationId = Guid.NewGuid().ToString(),
                    Priority   = Priority.Low,
                });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ── GET /api/request/get/{id} ─────────────────────────────────────────

        [Fact]
        public async Task GetById_AsAnyAuthUser_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.CustomerId, "Customer")
                .GetAsync($"/api/request/get/{seed.RequestId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetById_NonExistent_ReturnsBadRequest()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.AdminId, "Admin")
                .GetAsync("/api/request/get/does-not-exist");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ── GET /api/request/all ──────────────────────────────────────────────

        [Fact]
        public async Task GetAll_AsOperator_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.OperatorId, "Operator")
                .GetAsync("/api/request/all");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetAll_AsCustomer_ReturnsForbidden()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.CustomerId, "Customer")
                .GetAsync("/api/request/all");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // ── GET /api/request/my ───────────────────────────────────────────────

        [Fact]
        public async Task GetMine_ReturnsOnlyOwnRequests()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.CustomerId, "Customer")
                .GetAsync("/api/request/my");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ── POST /api/request/approve ─────────────────────────────────────────

        [Fact]
        public async Task Approve_AsOperator_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.OperatorId, "Operator")
                .PostAsJsonAsync("/api/request/approve", new ApproveRequestDto
                {
                    Id    = seed.RequestId,
                    Notes = "Looks good",
                });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Approve_AsCustomer_ReturnsForbidden()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.CustomerId, "Customer")
                .PostAsJsonAsync("/api/request/approve", new ApproveRequestDto
                {
                    Id = seed.RequestId,
                });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // ── POST /api/request/decline ─────────────────────────────────────────

        [Fact]
        public async Task Decline_AsOperator_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.OperatorId, "Operator")
                .PostAsJsonAsync("/api/request/decline", new DeclineRequestDto
                {
                    Id            = seed.RequestId,
                    DeclineReason = "Out of scope",
                });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ── POST /api/request/update ──────────────────────────────────────────

        [Fact]
        public async Task Update_AsCustomer_OnPendingRequest_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.CustomerId, "Customer")
                .PostAsJsonAsync("/api/request/update", new UpdateRequestDto
                {
                    Id          = seed.RequestId,
                    Title       = "Updated title",
                    Description = "Added more details.",
                });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ── PATCH /api/request/done ───────────────────────────────────────────

        [Fact]
        public async Task MarkDone_AsOperator_OnPendingRequest_ReturnsBadRequest()
        {
            // Pending request cannot be marked done — business rule.
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.OperatorId, "Operator")
                .PatchAsync($"/api/request/done?id={seed.RequestId}", null);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ── GET /api/request/{id}/logs ────────────────────────────────────────

        [Fact]
        public async Task GetLogs_AsOperator_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.OperatorId, "Operator")
                .GetAsync($"/api/request/{seed.RequestId}/logs");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetLogs_AsCustomer_ReturnsForbidden()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.CustomerId, "Customer")
                .GetAsync($"/api/request/{seed.RequestId}/logs");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}
