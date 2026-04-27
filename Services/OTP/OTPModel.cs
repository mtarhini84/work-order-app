namespace WorkOrderApp.Services.OTP
{
	public class OTPModel
	{
		public string Id { get; set; }
		public string Action { get; set; }
		public string Token { get; set; }
		public string MobileNumber { get; set; }
		public int? CustomerId { get; set; }
		public DateTime ExpiryDate { get; set; }
		public bool Validated { get; set; }
		public int VerificationAttempts { get; set; }
		public int GenerateAttempts { get; set; }
		public bool Locked { get; set; }
		public DateTime? LockUntil { get; set; }
	}

	public class SendOTPModel
	{
		public string MobileNumber { get; set; }
		public bool IsSMS { get; set; }
	}
}
