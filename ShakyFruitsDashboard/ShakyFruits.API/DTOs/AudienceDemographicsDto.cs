using System.Collections.Generic;

namespace ShakyFruits.API.DTOs
{
    public class AudienceDemographicsDto
    {
        public string WomenPercentage { get; set; } = "0%";
        public string MenPercentage { get; set; } = "0%";
        public List<DemographicItemDto> TopCountries { get; set; } = new();
        public List<DemographicItemDto> TopCities { get; set; } = new();
    }

    public class DemographicItemDto
    {
        public string Name { get; set; } = string.Empty;
        public string Percentage { get; set; } = string.Empty;
    }
}
