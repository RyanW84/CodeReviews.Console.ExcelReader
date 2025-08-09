namespace ExcelReader.RyanW84.Data.Models;

public class ExcelBeginner
{
	public int Id { get; set; }
	public string? Name { get; set; }
	public int Age { get; set; }
	public string Sex { get; set; } = string.Empty;
	public string Colour { get; set; } = string.Empty;
	public string Height { get; set; } = string.Empty;
	// Additional properties can be added as needed
}