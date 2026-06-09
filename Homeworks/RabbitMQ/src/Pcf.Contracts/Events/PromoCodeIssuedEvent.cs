namespace Pcf.Contracts.Events
{
    public class PromoCodeIssuedEvent : IPromoCodeIssuedEvent
    {
        public Guid PromoCodeId { get; set; }
        public string ServiceInfo { get; set; } = string.Empty;
        public Guid PartnerId { get; set; }
        public string PromoCode { get; set; } = string.Empty;
        public Guid PreferenceId { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid? PartnerManagerId { get; set; }
    }
}
