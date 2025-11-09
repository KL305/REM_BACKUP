using System;

namespace REM_System.Data
{
    public class Property
    {
        public int PropertyId { get; set; }
        public int SellerId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PropertyType { get; set; } = string.Empty; // "RealEstate" or "Product"
        public string Address { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int? Bedrooms { get; set; }
        public int? Bathrooms { get; set; }
        public decimal? Area { get; set; } // Square feet or square meters
        public string Status { get; set; } = "Available"; // Available, Sold, Pending
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}

