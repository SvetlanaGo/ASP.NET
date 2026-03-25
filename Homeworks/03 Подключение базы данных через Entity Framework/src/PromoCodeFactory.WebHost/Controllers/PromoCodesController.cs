using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Mapping;
using PromoCodeFactory.WebHost.Models.PromoCodes;

namespace PromoCodeFactory.WebHost.Controllers;

/// <summary>
/// Промокоды
/// </summary>
public class PromoCodesController : BaseController
{
    private readonly IRepository<PromoCode> promoCodeRepository;
    private readonly IRepository<Customer> customerRepository;
    private readonly IRepository<CustomerPromoCode> customerPromoCodeRepository;
    private readonly IRepository<Preference> preferenceRepository;
    private readonly IRepository<Employee> employeeRepository;

    public PromoCodesController(
        IRepository<PromoCode> promoCodeRepository,
        IRepository<Customer> customerRepository,
        IRepository<CustomerPromoCode> customerPromoCodeRepository,
        IRepository<Preference> preferenceRepository,
        IRepository<Employee> employeeRepository)
    {
        this.promoCodeRepository = promoCodeRepository;
        this.customerRepository = customerRepository;
        this.customerPromoCodeRepository = customerPromoCodeRepository;
        this.preferenceRepository = preferenceRepository;
        this.employeeRepository = employeeRepository;
    }

    /// <summary>
    /// Получить все промокоды
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PromoCodeShortResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PromoCodeShortResponse>>> Get(CancellationToken ct)
    {
        var promoCodes = await promoCodeRepository.GetAll(true, ct);

        return Ok(promoCodes.Select(PromoCodesMapper.ToPromoCodeShortResponse));
    }

    /// <summary>
    /// Получить промокод по id
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PromoCodeShortResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PromoCodeShortResponse>> GetById(Guid id, CancellationToken ct)
    {
        var promoCode = await promoCodeRepository.GetById(id, true, ct);

        return promoCode is null
            ? NotFound(new ProblemDetails
                        {
                            Status = StatusCodes.Status404NotFound,
                            Title = "Resource not found",
                            Detail = $"Promo code with Id {id} does not exist.",
                            Type = "https://httpstatuses.com/404",
                            Instance = HttpContext.Request.Path
                        })
            : Ok(PromoCodesMapper.ToPromoCodeShortResponse(promoCode));
    }

    /// <summary>
    /// Создать промокод и выдать его клиентам с указанным предпочтением
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PromoCodeShortResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PromoCodeShortResponse>> Create(PromoCodeCreateRequest request, CancellationToken ct)
    {
        var partnerManager = await employeeRepository.GetById(request.PartnerManagerId, ct: ct);
        if (partnerManager is null)
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "No partner manager found",
                Detail = $"Partner manager with Id {request.PartnerManagerId} does not exist.",
                Type = "https://httpstatuses.com/400",
                Instance = HttpContext.Request.Path
            });

        var preference = await preferenceRepository.GetById(request.PreferenceId, ct: ct);
        if (preference is null)
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "No preference found",
                Detail = $"Preference with Id {request.PreferenceId} does not exist.",
                Type = "https://httpstatuses.com/400",
                Instance = HttpContext.Request.Path
            });

        var customerPreferences = await customerRepository.GetWhere(c => c.Preferences.Contains(preference), ct: ct);
        if (!customerPreferences.Any())
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "No Customers Found",
                Detail = $"No customers with preference '{preference.Name}' found to receive this promo code.",
                Type = "https://httpstatuses.com/404",
                Instance = HttpContext.Request.Path
            });

        var customerPromoCodes = customerPreferences
            .Select(c => new CustomerPromoCode
            {
                Id = Guid.NewGuid(),
                CustomerId = c.Id,
                CreatedAt = DateTimeOffset.UtcNow
            });

        var promoCode = PromoCodesMapper.ToPromoCode(request, partnerManager, preference, customerPromoCodes);
        await promoCodeRepository.Add(promoCode, ct);
        var promoCodeModel = PromoCodesMapper.ToPromoCodeShortResponse(promoCode);

        return CreatedAtAction(nameof(GetById), new { id = promoCode.Id }, promoCodeModel);
    }

    /// <summary>
    /// Применить промокод (отметить, что клиент использовал промокод)
    /// </summary>
    [HttpPost("{id:guid}/apply")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Apply(
        [FromRoute] Guid id,
        [FromBody] PromoCodeApplyRequest request,
        CancellationToken ct)
    {
        var customerPromoCodes = await customerPromoCodeRepository
            .GetWhere(c => request.CustomerId == c.CustomerId
                        && id == c.PromoCodeId
                        && !c.AppliedAt.HasValue, ct: ct);

        var customerPromoCode = customerPromoCodes.FirstOrDefault();
        if (customerPromoCode is null)
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Active promo code not found",
                Detail = $"No active promo code '{id}' found for customer '{id}'",
                Type = "https://httpstatuses.com/404",
                Instance = HttpContext.Request.Path
            });

        customerPromoCode.AppliedAt = DateTimeOffset.UtcNow;
        await customerPromoCodeRepository.Update(customerPromoCode, ct);

        return NoContent();
    }
}
