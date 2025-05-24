using System.ComponentModel.DataAnnotations;

namespace MagniseTestTask.Models;

public class Asset
{
    [Key]
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Symbol { get; set; }
    public string Provider { get; set; }
}