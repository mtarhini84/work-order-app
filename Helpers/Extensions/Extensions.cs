using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace WorkOrderApp.Helpers.Extensions
{
	public static class Extensions
	{
		public static T GetValueSafe<T>(this IDictionary<string, object> dictionary, string key)
		{
			if (dictionary == null || !dictionary.ContainsKey(key) || dictionary[key] == null)
			{
				return default(T);
			}

			if (dictionary.TryGetValue(key, out object value) && value is T)
			{
				return (T)value;
			}

			else if (typeof(T) == typeof(DateTime) && value is string stringValue)
			{
				if (DateTime.TryParse(stringValue, out DateTime dateTimeValue))
				{
					return (T)(object)dateTimeValue;
				}
			}

			else if (typeof(T) == typeof(DateTime) && value.GetType() == typeof(DateTimeOffset))
			{
				return (T)(object)((DateTimeOffset)value).UtcDateTime;
			}

			return default(T);
		}

		public static T GetNumericValueSafe<T>(this IDictionary<string, object> dictionary, string key)
		{
			if (dictionary == null || !dictionary.ContainsKey(key) || dictionary[key] == null)
			{
				return default;
			}

			object value = dictionary[key];

			if (value is T variable)
			{
				return variable;
			}

			try
			{
				if (value is string str)
				{
					if (typeof(T) == typeof(DateTime) && DateTime.TryParse(str, out var dt))
						return (T)(object)dt;

					if (typeof(T) == typeof(decimal) && decimal.TryParse(str, out var dec))
						return (T)(object)dec;

					if (typeof(T) == typeof(int) && int.TryParse(str, out var i))
						return (T)(object)i;

					if (typeof(T) == typeof(long) && long.TryParse(str, out var l))
						return (T)(object)l;

					if (typeof(T) == typeof(double) && double.TryParse(str, out var d))
						return (T)(object)d;
				}

				if (typeof(T) == typeof(DateTime) && value is DateTimeOffset dto)
				{
					return (T)(object)dto.UtcDateTime;
				}

				return (T)Convert.ChangeType(value, typeof(T));
			}
			catch
			{
				return default;
			}
		}
	}

	public static class GeneralExtensions
	{
		public static Dictionary<string, object> ToDictionary(this object obj)
		{
			return obj.GetType().GetProperties().ToDictionary(
				prop => prop.Name,
				prop => prop.GetValue(obj, null)
			);
		}

		public static bool IsValidExtension(this IFormFile file)
		{
			string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

			string[] acceptedExtensions = { ".png", ".jpeg", ".jpg" };

			return acceptedExtensions.Contains(extension) && file.Length <= 5 * 1024 * 1024;
		}

		public static (bool, string) IsExcelOrCsv(this IFormFile file)
		{
			if (file == null || file.Length == 0)
				return (false, "No file uploaded.");

			var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
			if (!new[] { ".csv", ".xls", ".xlsx" }.Contains(extension))
				return (false, "Unsupported file type.");

			if (file.Length > 50 * 1024 * 1024)
				return (false, "File size exceeds 50 MB.");

			return (true, "");
		}
	}

	public static class StringExtensions
	{
		public static void ThrowIfNullOrEmpty(this string str)
		{
			if (str == null || str.Trim().Equals(""))
			{
				throw new ArgumentException("Encountered a null or empty value");
			}
		}

		public static void ThrowIfInvalidEmail(this string str)
		{
			str.ThrowIfNullOrEmpty();

			string pattern = @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";

			if (!Regex.IsMatch(str, pattern))
			{
				throw new ArgumentException("Invalid Email Address");
			}
		}

		public static bool IsValidDate(this string dateString)
		{
			DateTime tempDate;
			string[] formats = {
				"yyyy-MM-dd", // ISO 8601 Date
                "yyyy-MM-ddTHH:mm:ss", // ISO 8601 Date and Time without timezone
                "yyyy-MM-ddTHH:mm:ss.fffZ", // ISO 8601 Date and Time with milliseconds and UTC timezone
                "MM/dd/yyyy", // U.S. format
                "MM-dd-yyyy" // U.S. format with dashes
                // Add other formats as needed
            };

			return DateTime.TryParseExact(dateString, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind, out tempDate);
		}

		public static string GenerateRefNumber()
		{
			DateTime today = DateTime.Today;
			Random generator = new();

			return $"{today.Year}{today.Month:D2}{today.Day}-{generator.Next(0, 1000000):D6}";
		}

		public static string GenerateBarcode()
		{
			return Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper();
		}
	}

	public static class DateExtensions
	{
		public static string DateToString(this DateTime date)
		{
			return $"{date:yyyy'-'MM'-'dd}";
		}
	}
}
