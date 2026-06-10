using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Pcf.GivingToCustomer.Core.Abstractions.Repositories;
using Pcf.GivingToCustomer.Core.Domain;
using Pcf.GivingToCustomer.Core.Services;
using Pcf.GivingToCustomer.WebHost.Models;

namespace Pcf.GivingToCustomer.WebHost.Controllers
{
    /// <summary>
    /// Промокоды
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PromocodesController : ControllerBase
    {
        private readonly IRepository<PromoCode> _promoCodesRepository;
        private readonly IPromoCodeService _promoCodeService;

        public PromocodesController(
            IRepository<PromoCode> promoCodesRepository,
            IPromoCodeService promoCodeService)
        {
            _promoCodesRepository = promoCodesRepository;
            _promoCodeService = promoCodeService;
        }

        /// <summary>
        /// Получить все промокоды
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<PromoCodeShortResponse>>> GetPromocodesAsync()
        {
            var promocodes = await _promoCodesRepository.GetAllAsync();

            var response = promocodes.Select(x => new PromoCodeShortResponse()
            {
                Id = x.Id,
                Code = x.Code,
                BeginDate = x.BeginDate.ToString("yyyy-MM-dd"),
                EndDate = x.EndDate.ToString("yyyy-MM-dd"),
                PartnerId = x.PartnerId,
                ServiceInfo = x.ServiceInfo
            }).ToList();

            return Ok(response);
        }

        /// <summary>
        /// Создать промокод и выдать его клиентам с указанным предпочтением
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GivePromoCodesToCustomersWithPreferenceAsync(GivePromoCodeRequest request)
        {
            try
            {
                var message = new Core.Messages.GivePromoCodeMessage
                {
                    ServiceInfo = request.ServiceInfo,
                    PartnerId = request.PartnerId,
                    PromoCodeId = request.PromoCodeId,
                    PromoCode = request.PromoCode,
                    PreferenceId = request.PreferenceId,
                    BeginDate = request.BeginDate,
                    EndDate = request.EndDate
                };

                await _promoCodeService.GivePromoCodesToCustomersWithPreferenceAsync(message);
                return CreatedAtAction(nameof(GetPromocodesAsync), new { }, null);
            }
            catch (PreferenceNotFoundException)
            {
                return BadRequest();
            }
        }
    }
}