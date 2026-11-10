using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace MortgageApplications.IntegrationTests
{
    [TestFixture]
    public class MortgageApplicationIntegrationTests
    {
        private CustomWebApplicationFactory _factory = null!;
        private HttpClient _client = null!;

        [SetUp]
        public void SetUp()
        {
            _factory = new CustomWebApplicationFactory();

            _client = _factory.CreateClient(
                new Microsoft.AspNetCore.Mvc.Testing
                    .WebApplicationFactoryClientOptions
                {
                    BaseAddress = new Uri("https://localhost")
                });

            _client.DefaultRequestHeaders.Add(
                "X-Api-Key", "integration-test-key");
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        private static object ValidApplication() => new
        {
            annualIncome = 60_000m,
            monthlyExpenses = 500m,
            depositAmount = 50_000m,
            propertyValue = 300_000m,
            termYears = 25
        };

        private async Task<Guid> CreateApplicationAsync()
        {
            var response = await _client.PostAsJsonAsync(
                "/MortgageApplication",
                ValidApplication());

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var id = await response.Content.ReadFromJsonAsync<Guid>();

            Assert.That(id, Is.Not.EqualTo(Guid.Empty));

            return id;
        }

        [Test]
        public async Task Post_ValidApplication_ReturnsOkWithId()
        {
            // Act
            var response = await _client.PostAsJsonAsync(
                "/MortgageApplication",
                ValidApplication());

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var id = await response.Content.ReadFromJsonAsync<Guid>();

            Assert.That(id, Is.Not.EqualTo(Guid.Empty));
        }

        [Test]
        public async Task Post_InvalidApplication_ReturnsBadRequest()
        {
            // Arrange
            var request = new
            {
                annualIncome = 0,
                monthlyExpenses = -100,
                depositAmount = 350_000,
                propertyValue = 300_000,
                termYears = 50
            };

            // Act
            var response = await _client.PostAsJsonAsync(
                "/MortgageApplication",
                request);

            // Assert
            Assert.That(
                response.StatusCode,
                Is.EqualTo(HttpStatusCode.BadRequest));

            using var json = JsonDocument.Parse(
                await response.Content.ReadAsStringAsync());

            var errors = json.RootElement.GetProperty("errors");

            Assert.Multiple(() =>
            {
                Assert.That(
                    errors.TryGetProperty("AnnualIncome", out _), Is.True);
                Assert.That(
                    errors.TryGetProperty("MonthlyExpenses", out _), Is.True);
                Assert.That(
                    errors.TryGetProperty("DepositAmount", out _), Is.True);
                Assert.That(
                    errors.TryGetProperty("TermYears", out _), Is.True);
            });
        }

        [Test]
        public async Task Post_ThenGet_ReturnsSavedApplication()
        {
            // Arrange - Create an application through the API
            var id = await CreateApplicationAsync();

            // Act - Retrieve it through the API
            var response = await _client.GetAsync(
                $"/MortgageApplication/{id}");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            using var json = JsonDocument.Parse(
                await response.Content.ReadAsStringAsync());

            var application = json.RootElement;

            Assert.Multiple(() =>
            {
                Assert.That(
                    application.GetProperty("id").GetGuid(),
                    Is.EqualTo(id));

                Assert.That(
                    application.GetProperty("annualIncome").GetDecimal(),
                    Is.EqualTo(60_000m));

                Assert.That(
                    application.GetProperty("depositAmount").GetDecimal(),
                    Is.EqualTo(50_000m));

                Assert.That(
                    application.GetProperty("termYears").GetInt32(),
                    Is.EqualTo(25));
            });
        }

        [Test]
        public async Task Get_NonexistentApplication_ReturnsNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync(
                $"/MortgageApplication/{id}");

            // Assert
            Assert.That(
                response.StatusCode,
                Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task GetAssessment_ExistingApplication_ReturnsAssessment()
        {
            // Arrange
            var id = await CreateApplicationAsync();

            // Act
            var response = await _client.GetAsync(
                $"/MortgageApplication/Assess/{id}");

            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            using var json = JsonDocument.Parse(
                await response.Content.ReadAsStringAsync());

            var assessment = json.RootElement;

            Assert.Multiple(() =>
            {
                Assert.That(
                    assessment.GetProperty("loanAmount").GetDecimal(),
                    Is.EqualTo(250_000m));

                Assert.That(
                    assessment.GetProperty("decision").GetString(),
                    Is.EqualTo("Approved"));

                Assert.That(
                    assessment.TryGetProperty("maximumBorrowing", out _),
                    Is.True);

                Assert.That(
                    assessment.TryGetProperty("reasons", out _),
                    Is.True);
            });
        }

        [Test]
        public async Task GetAssessment_NonexistentApplication_ReturnsNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync(
                $"/MortgageApplication/Assess/{id}");

            // Assert
            Assert.That(
                response.StatusCode,
                Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task Get_WithoutApiKey_ReturnsUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Remove("X-Api-Key");

            // Act
            var response = await _client.GetAsync(
                $"/MortgageApplication/{Guid.NewGuid()}");

            // Assert
            Assert.That(
                response.StatusCode,
                Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task Get_WithInvalidApiKey_ReturnsUnauthorized()
        {
            // Arrange
            _client.DefaultRequestHeaders.Remove("X-Api-Key");
            _client.DefaultRequestHeaders.Add(
                "X-Api-Key", "incorrect-key");

            // Act
            var response = await _client.GetAsync(
                $"/MortgageApplication/{Guid.NewGuid()}");

            // Assert
            Assert.That(
                response.StatusCode,
                Is.EqualTo(HttpStatusCode.Unauthorized));
        }
    }
}
