using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Mortage_Application_API.Controllers;
using Mortage_Application_API.Domain;
using Mortage_Application_API.Domain.Affordability;
using Mortage_Application_API.Repositories;
using Mortgage_Application_API.Domain.Requests;

namespace MortgageApplications.UnitTests
{
    [TestFixture]
    public class MortgageApplicationControllerTests
    {
        private Mock<IMortgageApplicationRepository> _repository = null!;
        private Mock<ILogger<MortgageApplicationController>> _logger = null!;
        private Mock<IAffordabilityCalculator> _calculator = null!;
        private MortgageApplicationController _controller = null!;
        private Mock<IValidator<CreateMortgageApplicationRequest>> _validator = null!;

        [SetUp]
        public void SetUp()
        {
            _repository = new Mock<IMortgageApplicationRepository>();
            _logger = new Mock<ILogger<MortgageApplicationController>>();
            _calculator = new Mock<IAffordabilityCalculator>();
            _validator = new Mock<IValidator<CreateMortgageApplicationRequest>>();

            _controller = new MortgageApplicationController(
                _repository.Object,
                _logger.Object,
                _calculator.Object,
                _validator.Object);
        }

        private static MortgageApplication CreateApplication()
        {
            return new MortgageApplication(
                60_000m,
                500m,
                50_000m,
                300_000m,
                25);
        }

        private static CreateMortgageApplicationRequest CreateValidRequest()
        {
            return new CreateMortgageApplicationRequest
            {
                AnnualIncome = 60_000m,
                MonthlyExpenses = 500m,
                DepositAmount = 50_000m,
                PropertyValue = 300_000m,
                TermYears = 25
            };
        }

        // ==========================================
        // GET BY ID TESTS
        // ==========================================

        [Test]
        public async Task GetById_WhenApplicationExists_ReturnsOk()
        {
            // Arrange
            var application = CreateApplication();

            _repository
                .Setup(x => x.GetByIdAsync(application.Id))
                .ReturnsAsync(application);

            // Act
            var result = await _controller.GetById(application.Id);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());

            var okResult = (OkObjectResult)result;

            Assert.That(okResult.Value, Is.SameAs(application));

            _repository.Verify(
                x => x.GetByIdAsync(application.Id),
                Times.Once);
        }

        [Test]
        public async Task GetById_WhenApplicationDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();

            _repository
                .Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync((MortgageApplication?)null);

            // Act
            var result = await _controller.GetById(id);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());

            _repository.Verify(
                x => x.GetByIdAsync(id),
                Times.Once);
        }

        // ==========================================
        // CREATE TESTS
        // ==========================================

        [Test]
        public async Task Create_WhenRequestIsValid_ReturnsOk()
        {
            // Arrange
            var request = CreateValidRequest();

            _validator
                .Setup(x => x.ValidateAsync(
                    request,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _repository
                .Setup(x => x.AddAsync(It.IsAny<MortgageApplication>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(request);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());

            var okResult = (OkObjectResult)result;

            Assert.That(okResult.Value, Is.TypeOf<Guid>());
            Assert.That((Guid)okResult.Value!, Is.Not.EqualTo(Guid.Empty));

            _repository.Verify(
                x => x.AddAsync(It.Is<MortgageApplication>(a =>
                    a.AnnualIncome == request.AnnualIncome &&
                    a.DepositAmount == request.DepositAmount &&
                    a.TermYears == request.TermYears)),
                Times.Once);
        }

        [Test]
        public async Task Create_WhenRequestIsInvalid_ReturnsBadRequest()
        {
            // Arrange
            var request = CreateValidRequest();
            request.AnnualIncome = 0;

            var errors = new List<ValidationFailure>
            {
                new ValidationFailure(
                nameof(request.AnnualIncome),
                "Annual income must be positive.")
            };

            _validator
                .Setup(x => x.ValidateAsync(
                    request,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(errors));

            // Act
            var result = await _controller.Create(request);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());

            var badRequest = (BadRequestObjectResult)result;
            var details = badRequest.Value as ValidationProblemDetails;

            Assert.That(details, Is.Not.Null);
            Assert.That(
                details!.Errors.ContainsKey("AnnualIncome"),
                Is.True);

            _repository.Verify(
                x => x.AddAsync(It.IsAny<MortgageApplication>()),
                Times.Never);
        }

        // ==========================================
        // AFFORDABILITY ASSESSMENT TESTS
        // ==========================================

        [Test]
        public async Task GetAssessment_WhenApplicationExists_ReturnsOk()
        {
            // Arrange
            var application = CreateApplication();

            var calculator = new AffordabilityCalculator(new AffordabilityOptions());

            var assessment = calculator.Assess(application);

            _repository
                .Setup(x => x.GetByIdAsync(application.Id))
                .ReturnsAsync(application);

            _calculator
                .Setup(x => x.Assess(application))
                .Returns(assessment);

            // Act
            var result = await _controller.GetAssessment(application.Id);

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());

            var okResult = (OkObjectResult)result;

            Assert.That(okResult.Value, Is.SameAs(assessment));

            _repository.Verify(
                x => x.GetByIdAsync(application.Id),
                Times.Once);

            _calculator.Verify(
                x => x.Assess(application),
                Times.Once);
        }

        [Test]
        public async Task GetAssessment_WhenApplicationDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();

            _repository
                .Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync((MortgageApplication?)null);

            // Act
            var result = await _controller.GetAssessment(id);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());

            _repository.Verify(
                x => x.GetByIdAsync(id),
                Times.Once);
        }
    }
}
