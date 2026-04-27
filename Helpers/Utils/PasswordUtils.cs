using System.Security.Cryptography;

namespace WorkOrderApp.Helpers.Utils
{
	public static class PasswordUtils
	{
		public static string HashPassword(string password, int iterations = 10000)
		{
			byte[] salt = new byte[16];
			using (var rng = new RNGCryptoServiceProvider())
			{
				rng.GetBytes(salt);
			}

			using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
			{
				byte[] hash = pbkdf2.GetBytes(20);
				byte[] hashBytes = new byte[36];
				Array.Copy(salt, 0, hashBytes, 0, 16);
				Array.Copy(hash, 0, hashBytes, 16, 20);

				return Convert.ToBase64String(hashBytes);
			}
		}

		public static bool VerifyPassword(string storedHash, string password)
		{
			byte[] hashBytes = Convert.FromBase64String(storedHash);

			byte[] salt = new byte[16];
			Array.Copy(hashBytes, 0, salt, 0, 16);

			byte[] storedHashBytes = new byte[20];
			Array.Copy(hashBytes, 16, storedHashBytes, 0, 20);

			using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256))
			{
				byte[] testHash = pbkdf2.GetBytes(20);

				for (int i = 0; i < 20; i++)
				{
					if (storedHashBytes[i] != testHash[i])
					{
						return false;
					}
				}
			}

			return true;
		}

		public static bool ValidatePassword(string password)
		{
			const int minLength = 4;
			const int maxLength = 100;
			const int minUppercase = 1;
			const int minLowercase = 0;
			const int minNumbers = 1;
			const int minSpecial = 0;

			if (string.IsNullOrWhiteSpace(password))
				return false;

			if (password.Length < minLength || password.Length > maxLength)
				return false;

			int uppercaseCount = password.Count(char.IsUpper);
			int lowercaseCount = password.Count(char.IsLower);
			int numberCount = password.Count(char.IsDigit);
			int specialCount = password.Count(c => !char.IsLetterOrDigit(c));

			return uppercaseCount >= minUppercase
				   && lowercaseCount >= minLowercase
				   && numberCount >= minNumbers
				   && specialCount >= minSpecial;
		}
	}

	public static class CodeGenerator
	{
		/// <summary>
		/// Generates a random 10-character code with exactly 6 letters (A–Z) and 4 digits (0–9).
		/// </summary>
		public static string GenerateRandomCode6L4D()
		{
			const string Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			const string Digits = "0123456789";

			var chars = new List<char>(10);

			// Pick 6 random letters
			for (int i = 0; i < 6; i++)
			{
				int idx = RandomNumberGenerator.GetInt32(Letters.Length);
				chars.Add(Letters[idx]);
			}

			// Pick 4 random digits
			for (int i = 0; i < 4; i++)
			{
				int idx = RandomNumberGenerator.GetInt32(Digits.Length);
				chars.Add(Digits[idx]);
			}

			// Fisher–Yates shuffle
			for (int i = chars.Count - 1; i > 0; i--)
			{
				int j = RandomNumberGenerator.GetInt32(i + 1);
				(chars[i], chars[j]) = (chars[j], chars[i]);
			}

			return new string(chars.ToArray());
		}
	}
}
