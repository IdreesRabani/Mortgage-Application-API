using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Mortage_Application_API.Domain;
using Mortage_Application_API.Domain.Affordability;
using Mortage_Application_API.Repositories;
using Mortgage_Application_API.Domain.Requests;

namespace Mortage_Application_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MortgageApplicationController : ControllerBase
    {
        private readonly IMortgageApplicationRepository _repository;
        private readonly ILogger<MortgageApplicationController> _logger;
        private readonly IAffordabilityCalculator _affordabilityCalculator;
        private readonly IValidator<CreateMortgageApplicationRequest> _validator;

        public MortgageApplicationController(
            IMortgageApplicationRepository repository, 
            ILogger<MortgageApplicationController> logger,
            IAffordabilityCalculator affordabilityCalculator,
            IValidator<CreateMortgageApplicationRequest> validator)
        {
            _repository = repository;
            _logger = logger;
            _affordabilityCalculator = affordabilityCalculator;
            _validator = validator;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var application = await _repository.GetByIdAsync(id);

            if (application is null)
            {
                _logger.LogError("Mortgage application {ApplicationId} not found", id);
                return NotFound();
            }

            _logger.LogInformation("Mortgage application {ApplicationId} retrieved", id);
            return Ok(application);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMortgageApplicationRequest request)
        {
            var validationResult = await _validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(
                        error.PropertyName,
                        error.ErrorMessage);
                }

                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var application = new MortgageApplication(
                request.AnnualIncome,
                request.MonthlyExpenses,
                request.DepositAmount,
                request.PropertyValue,
                request.TermYears);

            try
            {
                await _repository.AddAsync(application);

                _logger.LogInformation(
                    "Mortgage application {ApplicationId} created",
                    application.Id);

                return Ok(application.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error creating mortgage application");

                return StatusCode(
                    500,
                    "An unexpected error occurred");
            }
        }


        [HttpGet("Assess/{Id}")]
        public async Task<IActionResult> GetAssessment(Guid id)
        {
            var application = await _repository.GetByIdAsync(id);

            if (application is null)
            {
                _logger.LogError("Mortgage application {ApplicationId} not found", id);
                return NotFound();
            }

            _logger.LogInformation("Retrieved mortgage application {ApplicationId} for assessing", id);

            try
            {
                _logger.LogInformation("Assessing mortgage application {ApplicationId}", application.Id);
                var result = _affordabilityCalculator.Assess(application);

                _logger.LogInformation("Mortgage application {ApplicationId} assessed", application.Id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assessing mortgage application");

                return StatusCode(500, "An unexpected error occurred");
            }
        }

    }
}
