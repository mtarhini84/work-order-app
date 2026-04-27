using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WorkOrderApp.Controllers;
using WorkOrderApp.Tests.Infrastructure;
using Xunit;

namespace WorkOrderApp.Tests.Controllers
{
    public class AttachmentControllerTests : IClassFixture<TestWebAppFactory>
    {
        private readonly TestWebAppFactory _factory;

        public AttachmentControllerTests(TestWebAppFactory factory)
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

        // ── POST /api/attachment/create ───────────────────────────────────────

        [Fact]
        public async Task Create_WithWorkOrderId_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.ExecutorId, "Executor")
                .PostAsJsonAsync("/api/attachment/create", new CreateAttachmentDto
                {
                    Url         = "https://storage.example.com/test/photo.jpg",
                    FileName    = "photo.jpg",
                    ContentType = "image/jpeg",
                    WorkOrderId = seed.WorkOrderId,
                });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithRequestId_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.CustomerId, "Customer")
                .PostAsJsonAsync("/api/attachment/create", new CreateAttachmentDto
                {
                    Url         = "https://storage.example.com/test/doc.pdf",
                    FileName    = "doc.pdf",
                    ContentType = "application/pdf",
                    RequestId   = seed.RequestId,
                });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithoutOwner_ReturnsBadRequest()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            // Neither RequestId nor WorkOrderId — should fail business validation.
            var response = await Client(seed.ExecutorId, "Executor")
                .PostAsJsonAsync("/api/attachment/create", new CreateAttachmentDto
                {
                    Url      = "https://storage.example.com/test/orphan.jpg",
                    FileName = "orphan.jpg",
                });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Create_Unauthenticated_ReturnsUnauthorized()
        {
            var response = await _factory.CreateClient()
                .PostAsJsonAsync("/api/attachment/create", new CreateAttachmentDto
                {
                    Url      = "https://storage.example.com/anon.jpg",
                    FileName = "anon.jpg",
                });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ── GET /api/attachment/get/{id} ──────────────────────────────────────

        [Fact]
        public async Task GetById_AsAuthUser_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.ExecutorId, "Executor")
                .GetAsync($"/api/attachment/get/{seed.AttachmentId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetById_NonExistent_ReturnsBadRequest()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.AdminId, "Admin")
                .GetAsync("/api/attachment/get/no-such-id");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ── GET /api/attachment/work-order/{id} ───────────────────────────────

        [Fact]
        public async Task GetByWorkOrder_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.OperatorId, "Operator")
                .GetAsync($"/api/attachment/work-order/{seed.WorkOrderId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ── GET /api/attachment/request/{id} ──────────────────────────────────

        [Fact]
        public async Task GetByRequest_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.OperatorId, "Operator")
                .GetAsync($"/api/attachment/request/{seed.RequestId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // ── DELETE /api/attachment/delete/{id} ────────────────────────────────

        [Fact]
        public async Task Delete_AsAdmin_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.AdminId, "Admin")
                .DeleteAsync($"/api/attachment/delete/{seed.AttachmentId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Delete_AsCustomer_ReturnsForbidden()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.CustomerId, "Customer")
                .DeleteAsync($"/api/attachment/delete/{seed.AttachmentId}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // ── PATCH /api/attachment/activate & deactivate ───────────────────────

        [Fact]
        public async Task Deactivate_AsAdmin_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);

            var response = await Client(seed.AdminId, "Admin")
                .PatchAsync($"/api/attachment/deactivate?id={seed.AttachmentId}", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Activate_AsAdmin_AfterDeactivate_ReturnsOk()
        {
            var seed = await DbHelper.SeedAsync(_factory.Services);
            var admin = Client(seed.AdminId, "Admin");
            await admin.PatchAsync($"/api/attachment/deactivate?id={seed.AttachmentId}", null);

            var response = await admin.PatchAsync($"/api/attachment/activate?id={seed.AttachmentId}", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
