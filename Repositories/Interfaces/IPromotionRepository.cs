using SWD392_PROJECT.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SWD392_PROJECT.Data.Repositories.Interfaces
{
    /// <summary>
    /// IPromotionRepository - Data Access Interface for Promotion Entity
    /// Defines contract for promotion data operations
    /// UC24 - View Promotions
    /// </summary>
    public interface IPromotionRepository
    {
        /// <summary>
        /// Retrieve all promotions from the database
        /// </summary>
        /// <returns>List of all promotions</returns>
        Task<List<Promotion>> FindAll();

        /// <summary>
        /// Retrieve a specific promotion by ID
        /// </summary>
        /// <param name="promotionId">The promotion ID to search for</param>
        /// <returns>Promotion object if found, null otherwise</returns>
        Task<Promotion?> FindByPromotionId(int promotionId);

        /// <summary>
        /// Retrieve all active promotions that haven't expired
        /// </summary>
        /// <returns>List of active promotions</returns>
        Task<List<Promotion>> FindAllActive();

        /// <summary>
        /// Create a new promotion in the database
        /// </summary>
        /// <param name="promotion">Promotion object to create</param>
        /// <returns>The created promotion with assigned ID</returns>
        Task<Promotion> Create(Promotion promotion);

        /// <summary>
        /// Update an existing promotion
        /// </summary>
        /// <param name="promotion">Promotion object with updated values</param>
        /// <returns>The updated promotion</returns>
        Task<Promotion> Update(Promotion promotion);

        /// <summary>
        /// Delete a promotion (soft delete by changing status)
        /// </summary>
        /// <param name="promotionId">ID of promotion to delete</param>
        /// <returns>true if deletion was successful, false otherwise</returns>
        Task<bool> Delete(int promotionId);

        /// <summary>
        /// Check if a promotion exists by ID
        /// </summary>
        /// <param name="promotionId">The promotion ID to check</param>
        /// <returns>true if promotion exists, false otherwise</returns>
        Task<bool> Exists(int promotionId);
    }
}
