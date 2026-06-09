namespace Pcf.Contracts.Events
{
    public interface IPromoCodeIssuedEvent
    {
        Guid PromoCodeId { get; }
        string ServiceInfo { get; }
        Guid PartnerId { get; }
        string PromoCode { get; }
        Guid PreferenceId { get; }
        DateTime BeginDate { get; }
        DateTime EndDate { get; }
        Guid? PartnerManagerId { get; }
    }
}
