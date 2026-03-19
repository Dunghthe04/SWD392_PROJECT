using SWD392_PROJECT.Data.Repositories.Interfaces;
using SWD392_PROJECT.Models;
using Microsoft.EntityFrameworkCore;

namespace SWD392_PROJECT.Data.Repositories.Implementations;

/// <summary>
/// Repository implementation for Order entity using Entity Framework Core
/// Provides data access to Orders, OrderItems, and AuditLogs from the database
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all orders from the database
    /// </summary>
    public List<Order>? ReadOrderList()
    {
        try
        {
            return _context.Orders
                .Include(o => o.Items)
                .OrderByDescending(o => o.OrderTime)
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading order list: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Get a specific order by ID
    /// </summary>
    public Order? ReadOrder(int orderId)
    {
        try
        {
            return _context.Orders
                .Include(o => o.Items)
                .FirstOrDefault(o => o.OrderId == orderId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading order {orderId}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Update an order with optimistic concurrency control
    /// </summary>
    public bool UpdateOrder(Order updatedOrder, int expectedVersion)
    {
        try
        {
            var existingOrder = _context.Orders
                .Include(o => o.Items)
                .FirstOrDefault(o => o.OrderId == updatedOrder.OrderId);

            if (existingOrder is null)
            {
                return false;
            }

            // Check version for optimistic concurrency
            if (existingOrder.Version != expectedVersion)
            {
                return false;
            }

            // Update order details
            existingOrder.UpdateDetails(updatedOrder.Items, updatedOrder.Notes);
            existingOrder.Status = updatedOrder.Status;
            existingOrder.IsLocked = updatedOrder.IsLocked;
            existingOrder.Version += 1;
            existingOrder.LastUpdatedAt = DateTime.UtcNow;

            _context.Orders.Update(existingOrder);
            _context.SaveChanges();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating order: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Create a new order in the database
    /// </summary>
    public void CreateOrder(Order order)
    {
        try
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            _context.Orders.Add(order);
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating order: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Update only order total and status without modifying items
    /// Used for order placement to save total price correctly
    /// </summary>
    public void UpdateOrderTotal(int orderId, decimal totalPrice, string status)
    {
        try
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order is null)
            {
                throw new InvalidOperationException($"Order {orderId} not found");
            }

            order.TotalPrice = totalPrice;
            order.Status = status;
            order.LastUpdatedAt = DateTime.UtcNow;

            _context.Orders.Update(order);
            _context.SaveChanges();
            Console.WriteLine($"[UpdateOrderTotal] Order {orderId} updated: TotalPrice={totalPrice}, Status={status}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UpdateOrderTotal] Error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Delete an order from the database
    /// </summary>
    public bool DeleteOrder(int orderId)
    {
        try
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order is null)
            {
                return false;
            }

            _context.Orders.Remove(order);
            _context.SaveChanges();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting order: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Add an audit log entry to the database
    /// </summary>
    public void AddAuditLog(AuditLog log)
    {
        try
        {
            if (log == null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            _context.AuditLogs.Add(log);
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding audit log: {ex.Message}");
            throw;
        }
    }
}
