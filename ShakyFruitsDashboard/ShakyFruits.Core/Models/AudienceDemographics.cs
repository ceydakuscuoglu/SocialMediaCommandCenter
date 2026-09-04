using System.Collections.Generic;

// API.DTOs yerine doğrudan servislerinin veya core katmanının namespace'ini veriyoruz
namespace ShakyFruits.Core.Models
{
    public class AudienceDemographics
    {
        public string WomenPercentage { get; set; } = "0%";
        public string MenPercentage { get; set; } = "0%";
        public List<DemographicItem> TopCountries { get; set; } = new();
        public List<DemographicItem> TopCities { get; set; } = new();
    }

    public class DemographicItem
    {
        public string Name { get; set; }
        public string Percentage { get; set; }
    }
}