namespace Okidoki.Api.Extensions;

using Azure.Storage.Blobs.Models;

public static class BlobCorsRuleExtensions
{
	public static bool HasSameValuesAs(this BlobCorsRule rule, BlobCorsRule other) =>
		rule.AllowedOrigins == other.AllowedOrigins &&
		rule.AllowedMethods == other.AllowedMethods &&
		rule.AllowedHeaders == other.AllowedHeaders &&
		rule.ExposedHeaders == other.ExposedHeaders &&
		rule.MaxAgeInSeconds == other.MaxAgeInSeconds;
}
