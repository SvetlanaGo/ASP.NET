using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Mapping;
using PromoCodeFactory.WebHost.Models.Preferences;

namespace PromoCodeFactory.WebHost.Controllers;

/// <summary>
/// Предпочтения
/// </summary>
public class PreferencesController : BaseController
{
    private readonly IRepository<Preference> preferenceRepository;

    public PreferencesController(IRepository<Preference> preferenceRepository) =>
        this.preferenceRepository = preferenceRepository;

    /// <summary>
    /// Получить все доступные предпочтения
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PreferenceShortResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PreferenceShortResponse>>> Get(CancellationToken ct)
    {
        var preferences = await preferenceRepository.GetAll(ct: ct);

        return Ok(preferences.Select(PreferencesMapper.ToPreferenceShortResponse));
    }
}
