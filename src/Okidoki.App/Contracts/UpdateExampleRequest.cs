using System.ComponentModel.DataAnnotations;

namespace Okidoki.App.Contracts;

public class UpdateExampleRequest
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
}
