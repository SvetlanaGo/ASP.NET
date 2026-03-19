using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Mapping;
using PromoCodeFactory.WebHost.Models.Customers;

namespace PromoCodeFactory.WebHost.Controllers;

/// <summary>
/// Клиенты
/// </summary>
public class CustomersController : BaseController
{
    private readonly IRepository<Customer> customerRepository;
    private readonly IRepository<Preference> preferenceRepository;
    private readonly IRepository<PromoCode> promoCodeRepository;

    public CustomersController(
        IRepository<Customer> customerRepository,
        IRepository<Preference> preferenceRepository,
        IRepository<PromoCode> promoCodeRepository)
    {
        this.customerRepository = customerRepository;
        this.preferenceRepository = preferenceRepository;
        this.promoCodeRepository = promoCodeRepository;
    }

    /// <summary>
    /// Получить данные всех клиентов
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CustomerShortResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CustomerShortResponse>>> Get(CancellationToken ct)
    {
        var customers = await customerRepository.GetAll(true, ct);

        return Ok(customers.Select(CustomersMapper.ToCustomerShortResponse));
    }

    /// <summary>
    /// Получить данные клиента по Id
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerResponse>> GetById(Guid id, CancellationToken ct)
    {
        var customer = await customerRepository.GetById(id, true, ct);
        if (customer is null)
            return NotFound(this.GetNotFoundProblemDetails(id));

        var promoCodes = await promoCodeRepository.GetByRangeId(customer.CustomerPromoCodes.Select(cp => cp.PromoCodeId), true, ct);

        return Ok(CustomersMapper.ToCustomerResponse(customer, promoCodes));
    }

    /// <summary>
    /// Создать клиента
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CustomerShortResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CustomerShortResponse>> Create([FromBody] CustomerCreateRequest request, CancellationToken ct)
    {
        var preferenceIds = request.PreferenceIds;
        var preferencesResult = await ValidateAndLoadPreferencesAsync(request.PreferenceIds, ct);
        if (preferencesResult.Error is not null)
            return BadRequest(preferencesResult.Error);

        var customer = CustomersMapper.ToCustomer(request, preferencesResult.Preferences!);

        await customerRepository.Add(customer, ct);
        var customerModel = CustomersMapper.ToCustomerShortResponse(customer);

        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customerModel);
    }

    /// <summary>
    /// Обновить клиента
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CustomerShortResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerShortResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] CustomerUpdateRequest request,
        CancellationToken ct)
    {
        var updatedCustomer = await customerRepository.GetById(id, true, ct);
        if (updatedCustomer is null)
            return NotFound(this.GetNotFoundProblemDetails(id));

        var preferenceIds = request.PreferenceIds;
        var preferencesResult = await ValidateAndLoadPreferencesAsync(request.PreferenceIds, ct);
        if (preferencesResult.Error is not null)
            return BadRequest(preferencesResult.Error);

        var customer = CustomersMapper.ToCustomer(request, preferencesResult.Preferences!, updatedCustomer);

        await customerRepository.Update(customer, ct);

        var customerModel = CustomersMapper.ToCustomerShortResponse(customer);

        return Ok(customerModel);
    }

    /// <summary>
    /// Удалить клиента
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await customerRepository.Delete(id, ct: ct);
        }
        catch (EntityNotFoundException)
        {
            return NotFound(this.GetNotFoundProblemDetails(id));
        }

        return NoContent();
    }

    private async Task<(ProblemDetails? Error, IReadOnlyCollection<Preference>? Preferences)>
    ValidateAndLoadPreferencesAsync(IEnumerable<Guid> preferenceIds, CancellationToken ct)
    {
        if (preferenceIds.Distinct().Count() != preferenceIds.Count())
            return (GetPreferenceProblemDetails(), null);

        var preferences = await preferenceRepository.GetByRangeId(preferenceIds, ct: ct);
        if (preferences.Count != preferenceIds.Count())
            return (GetPreferenceProblemDetails(), null);

        return (null, preferences);
    }

    private ProblemDetails GetNotFoundProblemDetails(Guid id) =>
        new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Resource not found",
            Detail = $"Customer with Id {id} does not exist.",
            Type = "https://httpstatuses.com/404",
            Instance = HttpContext.Request.Path
        };

    private ProblemDetails GetPreferenceProblemDetails() =>
        new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Invalid Preference IDs",
            Detail = "Preference Ids must be unique and exist in the system",
            Type = "https://httpstatuses.com/400",
            Instance = HttpContext.Request.Path
        };
}
