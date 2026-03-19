using Microsoft.EntityFrameworkCore;
using SWD392_PROJECT.Data;
using SWD392_PROJECT.Data.Repositories.Interfaces;
using SWD392_PROJECT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWD392_PROJECT.Data.Repositories.Implementations
{
    /// <summary>
    /// PromotionRepository - Implementation of IPromotionRepository
    /// Handles all database operations for Promotion entity
    /// UC24 - View Promotions
    /// </summary>
    public class PromotionRepository : IPromotionRepository
    {
        private readonly AppDbContext _context;

        public PromotionRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Retrieve all promotions from the database (excluding deleted)
        /// </summary>
        public async Task<List<Promotion>> FindAll()
        {
            try
            {
                return await _context.Promotions
                    .AsNoTracking()
                    .Where(p => p.Status != "Deleted")
                    .OrderByDescending(p => p.CreatedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving all promotions from database.", ex);
            }
        }

        /// <summary>
        /// Retrieve a specific promotion by ID
        /// </summary>
        public async Task<Promotion?> FindByPromotionId(int promotionId)
        {
            if (promotionId <= 0)
            {
                throw new ArgumentException("Promotion ID must be greater than 0.", nameof(promotionId));
            }

            try
            {
                return await _context.Promotions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PromotionId == promotionId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving promotion with ID {promotionId}.", ex);
            }
        }

        /// <summary>
        /// Retrieve all active promotions that haven't expired
        /// </summary>
        public async Task<List<Promotion>> FindAllActive()
        {
            try
            {
                var now = DateTime.UtcNow;
                return await _context.Promotions
                    .AsNoTracking()
                    .Where(p => p.Status == "Active" && p.ExpiryDate > now)
                    .OrderByDescending(p => p.DiscountRate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving active promotions from database.", ex);
            }
        }

        /// <summary>
        /// Create a new promotion in the database
        /// </summary>
        public async Task<Promotion> Create(Promotion promotion)
        {
            if (promotion == null)
            {
                throw new ArgumentNullException(nameof(promotion), "Promotion object cannot be null.");
            }

            try
            {
                _context.Promotions.Add(promotion);
                await _context.SaveChangesAsync();
                return promotion;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Error creating promotion in database.", ex);
            }
        }

        /// <summary>
        /// Update an existing promotion
        /// </summary>
        public async Task<Promotion> Update(Promotion promotion)
        {
            if (promotion == null)
            {
                throw new ArgumentNullException(nameof(promotion), "Promotion object cannot be null.");
            }

            if (promotion.PromotionId <= 0)
            {
                throw new ArgumentException("Promotion ID must be greater than 0.", nameof(promotion));
            }

            try
            {
                var existingPromotion = await _context.Promotions.FirstOrDefaultAsync(p => p.PromotionId == promotion.PromotionId);
                
                if (existingPromotion == null)
                {
                    throw new InvalidOperationException($"Promotion with ID {promotion.PromotionId} not found.");
                }

                _context.Entry(existingPromotion).CurrentValues.SetValues(promotion);
                await _context.SaveChangesAsync();
                return existingPromotion;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Error updating promotion in database.", ex);
            }
        }

        /// <summary>
        /// Delete a promotion (soft delete by changing status to "Deleted")
        /// </summary>
        public async Task<bool> Delete(int promotionId)
        {
            if (promotionId <= 0)
            {
                throw new ArgumentException("Promotion ID must be greater than 0.", nameof(promotionId));
            }

            try
            {
                var promotion = await _context.Promotions.FirstOrDefaultAsync(p => p.PromotionId == promotionId);
                
                if (promotion == null)
                {
                    return false;
                }

                promotion.DeletePromotion(); // Calls business logic to set status to "Deleted"
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Error deleting promotion from database.", ex);
            }
        }

        /// <summary>
        /// Check if a promotion exists by ID
        /// </summary>
        public async Task<bool> Exists(int promotionId)
        {
            if (promotionId <= 0)
            {
                throw new ArgumentException("Promotion ID must be greater than 0.", nameof(promotionId));
            }

            try
            {
                return await _context.Promotions.AnyAsync(p => p.PromotionId == promotionId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error checking if promotion exists.", ex);
            }
        }
    }
}
