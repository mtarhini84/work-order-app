using Newtonsoft.Json;

namespace WorkOrderApp.Helpers.Expo
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PushReceiptRequest
    {
        
        [JsonProperty(PropertyName ="ids")]
        public List<string> PushTicketIds { get; set; }
    }
}
