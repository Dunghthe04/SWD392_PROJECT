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
            // First, try to get orders WITHOUT including items (to avoid foreign key issues)
            var orders = _context.Orders
                .OrderByDescending(o => o.OrderTime)
                .ToList();

            // Then manually load items for each order
            foreach (var order in orders)
            {
                order.Items = _context.OrderItems
                    .Where(oi => oi.OrderId == order.OrderId)
                    .ToList();
            }

            return orders;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading order list: {ex.Message}");
            Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
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
            var order = _context.Orders
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order != null)
            {
                // Manually load items
                order.Items = _context.OrderItems
                    .Where(oi => oi.OrderId == orderId)
                    .ToList();
            }

            return order;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading order {orderId}: {ex.Message}");
            Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
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
                .FirstOrDefault(o => o.OrderId == updatedOrder.OrderId);

            if (existingOrder is null)
            {
                return false;
            }

            // Manually load items
            existingOrder.Items = _context.OrderItems
                .Where(oi => oi.OrderId == existingOrder.OrderId)
                .ToList();

            // Check version for optimistic concurrency
            if (existingOrder.Version != expectedVersion)
            {
                return false;
            }

            // Update order details
            existingOrder.Notes = updatedOrder.Notes;
            foreach (var updatedItem in updatedOrder.Items)
            {
                var existingItem = existingOrder.Items.FirstOrDefault(i => i.MenuItemId == updatedItem.MenuItemId);
                if (existingItem is null)
                {
                    continue;
                }

                existingItem.ItemName = updatedItem.ItemName;
                existingItem.Quantity = updatedItem.Quantity;
                existingItem.UnitPrice = updatedItem.UnitPrice;
            }
            existingOrder.TotalPrice = (decimal)existingOrder.Items.Sum(i => i.LineTotal);
            existingOrder.Status = updatedOrder.Status;
            existingOrder.IsLocked = updatedOrder.IsLocked;
            existingOrder.Version += 1;
            existingOrder.LastUpdatedAt = DateTime.UtcNow;

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
    /// Create multiple order items in the database (batch insert)
    /// </summary>
    public void CreateOrderItems(List<OrderItem> orderItems)
    {
        try
        {
            if (orderItems == null || orderItems.Count == 0)
            {
                throw new ArgumentException("OrderItems list cannot be null or empty", nameof(orderItems));
            }

            // CRITICAL: Reset OrderItemId to 0 for all items to let database generate identity
            foreach (var orderItem in orderItems)
            {
                if (orderItem == null)
                {
                    throw new ArgumentNullException(nameof(orderItem));
                }

                // Explicitly set OrderItemId to 0 so EF doesn't try to insert it
                orderItem.OrderItemId = 0;
                Console.WriteLine($"[CreateOrderItems] Preparing item: OrderId={orderItem.OrderId}, MenuItemId={orderItem.MenuItemId}, OrderItemId reset to 0");
            }

            // Add all items to context
            foreach (var orderItem in orderItems)
            {
                _context.OrderItems.Add(orderItem);
            }

            // Save all at once (batch insert)
            _context.SaveChanges();
            Console.WriteLine($"[CreateOrderItems] Batch inserted {orderItems.Count} order items successfully");

            // Log each item with assigned IDs
            foreach (var item in orderItems)
            {
                Console.WriteLine($"[CreateOrderItems] ✓ OrderItemId={item.OrderItemId}, OrderId={item.OrderId}, MenuItemId={item.MenuItemId}, Qty={item.Quantity}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CreateOrderItems] Error: {ex.Message}");
            Console.WriteLine($"[CreateOrderItems] Inner exception: {ex.InnerException?.Message}");
            Console.WriteLine($"[CreateOrderItems] Stack trace: {ex.StackTrace}");
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
