using System;
using System.ComponentModel.DataAnnotations;

namespace SWD392_PROJECT.Models
{
    /// <summary>
    /// Promotion Entity - Represents a promotional offer in the system
    /// UC24 - View Promotions
    /// </summary>
    public class Promotion
    {
        // Private Properties (Information Hiding)
        [Key]
        private int _promotionId;

        [Required(ErrorMessage = "Promotion title is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
        private string _title;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        private string _description;

        [Required(ErrorMessage = "Discount rate is required")]
        [Range(0.01f, 100, ErrorMessage = "Discount rate must be between 0.01 and 100 percent")]
        private float _discountRate;

        [Required(ErrorMessage = "Expiry date is required")]
        private DateTime _expiryDate;

        [Required(ErrorMessage = "Status is required")]
        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters")]
        private string _status;

        // Optional: Track creation and update dates
        private DateTime _createdDate;
        private DateTime _lastUpdatedDate;

        // ============== Public Properties (Getters & Setters) ==============

        public int PromotionId
        {
            get { return _promotionId; }
            set { _promotionId = value; }
        }

        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public float DiscountRate
        {
            get { return _discountRate; }
            set { _discountRate = value; }
        }

        public DateTime ExpiryDate
        {
            get { return _expiryDate; }
            set { _expiryDate = value; }
        }

        public string Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public DateTime CreatedDate
        {
            get { return _createdDate; }
            set { _createdDate = value; }
        }

        public DateTime LastUpdatedDate
        {
            get { return _lastUpdatedDate; }
            set { _lastUpdatedDate = value; }
        }

        // ============== Constructors ==============

        /// <summary>
        /// Default constructor for EF Core
        /// </summary>
        public Promotion()
        {
            _title = string.Empty;
            _description = string.Empty;
            _status = "Active";
        }

        // ============== Business Logic Methods ==============

        /// <summary>
        /// Factory Method: createPromotion
        /// Initializes a promotion with provided values and sets status to "Active"
        /// </summary>
        /// <param name="title">Promotion title</param>
        /// <param name="description">Promotion description</param>
        /// <param name="discountRate">Discount percentage (0-100)</param>
        /// <param name="expiryDate">Expiry date of the promotion</param>
        /// <returns>Initialized Promotion object</returns>
        public static Promotion CreatePromotion(string title, string description, float discountRate, DateTime expiryDate)
        {
            return new Promotion
            {
                _title = title,
                _description = description,
                _discountRate = discountRate,
                _expiryDate = expiryDate,
                _status = "Active",
                _createdDate = DateTime.UtcNow,
                _lastUpdatedDate = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Read Method: readPromotion
        /// Returns a formatted string representation of the promotion
        /// </summary>
        /// <returns>Formatted promotion details string</returns>
        public string ReadPromotion()
        {
            return $"Title: {_title}, Discount: {_discountRate}%, Expires: {_expiryDate:MMM dd, yyyy}";
        }

        /// <summary>
        /// Update Method: updatePromotion
        /// Updates discount rate and expiry date if values are provided
        /// </summary>
        /// <param name="newDiscountRate">New discount rate (0-100)</param>
        /// <param name="newExpiryDate">New expiry date</param>
        public void UpdatePromotion(float? newDiscountRate, DateTime? newExpiryDate)
        {
            if (newDiscountRate.HasValue && newDiscountRate.Value > 0 && newDiscountRate.Value <= 100)
            {
                _discountRate = newDiscountRate.Value;
            }

            if (newExpiryDate.HasValue && newExpiryDate.Value > DateTime.UtcNow)
            {
                _expiryDate = newExpiryDate.Value;
            }

            _lastUpdatedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Delete Method: deletePromotion
        /// Changes status to "Deleted" for soft delete
        /// </summary>
        public void DeletePromotion()
        {
            _status = "Deleted";
            _lastUpdatedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Check if promotion has expired
        /// </summary>
        /// <returns>true if promotion has expired, false otherwise</returns>
        public bool IsExpired()
        {
            return DateTime.UtcNow > _expiryDate;
        }

        /// <summary>
        /// Check if promotion is active and not expired
        /// </summary>
        /// <returns>true if promotion is active and valid, false otherwise</returns>
        public bool IsActive()
        {
            return _status == "Active" && !IsExpired();
        }
    }
}
