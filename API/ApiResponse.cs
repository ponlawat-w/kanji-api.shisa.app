using Newtonsoft.Json;

namespace ShisaKanjis {
  public class ApiResponse {
    [JsonProperty("ok")]
    public bool Ok { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("data")]
    public object Data { get; set; }

    public static ApiResponse CreateSuccessful(object data = null) {
      return new ApiResponse {
        Ok = true,
        Status = null,
        Data = data
      };
    }

    public static ApiResponse CreateUnsuccessful(string status, object data = null) {
      return new ApiResponse {
        Ok = false,
        Status = status,
        Data = data
      };
    }
  }
}
