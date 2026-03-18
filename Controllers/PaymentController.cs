using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SWD392_PROJECT.Data;
using SWD392_PROJECT.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SWD392_PROJECT.Controllers
{
    /// <summary>
    /// PaymentController (Coordinator) - implements UC-18: Process Payment (demo)
    /// - createPaymentRequest(orderData): validate order and create Payment record
    /// - handlePaymentResponse(transactionResult): process gateway response and update Order/Payment
    /// - processTimeout(): mark payment as timed out
    /// This controller simulates VNPay interactions for demo purposes (no external calls).
    /// Preconditions: user is authenticated and has an order ready to pay.
    /// </summary>
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(AppDbContext db, ILogger<PaymentController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // GET: /Payment
        public IActionResult Index(int? orderId)
        {
            ViewBag.OrderId = orderId ?? 0;
            if (orderId.HasValue)
            {
                var order = _db.Orders.Find(orderId.Value);
                if (order != null)
                {
                    ViewBag.OrderTotal = order.TotalPrice;
                }
                else
                {
                    TempData["Error"] = "Order not found.";
                }
            }

            return View();
        }

        // POST: createPaymentRequest(orderData)
        // UC-18: createPaymentRequest coordinates validation and gateway redirection
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePaymentRequest(int orderId, float amount)
        {
            // Precondition check: order exists
            var order = await _db.Orders.FindAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning("CreatePaymentRequest: order {OrderId} not found", orderId);
                TempData["Error"] = "Order not found.";
                return RedirectToAction(nameof(Index));
            }

            // Optional: check ownership if claim available
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var userId))
            {
                if (order.StudentId != userId && !User.IsInRole("Manager"))
                {
                    _logger.LogWarning("CreatePaymentRequest: user {UserId} attempted to pay order {OrderId} owned by {Owner}", userId, orderId, order.StudentId);
                    return Forbid();
                }
            }

            // Validate amount (E1)
            var provided = (decimal)amount;
            if (provided <= 0 || Math.Abs(order.TotalPrice - provided) > 0.5m)
            {
                _logger.LogWarning("CreatePaymentRequest: invalid amount for order {OrderId}. OrderTotal={OrderTotal} Provided={Amount}", orderId, order.TotalPrice, provided);
                TempData["Error"] = "Invalid payment data. Please check the amount.";
                return RedirectToAction(nameof(Index), new { orderId });
            }

            // Create Payment record (Payment.createPayment)
            var payment = new Payment
            {
                OrderId = orderId,
                Amount = amount,
                Status = "Pending"
            };

            _db.Payments.Add(payment);
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreatePaymentRequest: DB error when creating payment for order {OrderId}", orderId);
                TempData["Error"] = "Could not create payment record. Please try again.";
                return RedirectToAction(nameof(Index), new { orderId });
            }

            // Simulate gateway interaction (demo): redirect user to a simulated VNPay response handler
            try
            {
                // In a real integration you would call a PaymentService to build a VNPay URL and redirect
                // For demo: pick simulated outcome and redirect to HandleResponse
                var rnd = new Random();
                var pick = rnd.Next(0, 3); // 0: SUCCESS, 1: CANCELLED, 2: TIMEOUT
                string status = pick switch
                {
                    0 => "SUCCESS",
                    1 => "CANCELLED",
                    _ => "TIMEOUT",
                };

                _logger.LogInformation("CreatePaymentRequest: payment {PaymentId} for order {OrderId} created, simulating gateway result {Status}", payment.PaymentId, orderId, status);
                return RedirectToAction(nameof(HandleResponse), new { paymentId = payment.PaymentId, status });
            }
            catch (Exception ex)
            {
                // E2: System error communicating with gateway
                _logger.LogError(ex, "CreatePaymentRequest: error while sending to gateway for payment {PaymentId}", payment.PaymentId);
                payment.Status = "Error";
                _db.Payments.Update(payment);
                await _db.SaveChangesAsync();

                TempData["Error"] = "Could not connect to payment gateway. Please try again.";
                return RedirectToAction(nameof(Index), new { orderId });
            }
        }

        // GET: /Payment/HandleResponse?paymentId=1&status=SUCCESS
        // UC-18: handlePaymentResponse(transactionResult)
        public async Task<IActionResult> HandleResponse(int paymentId, string status)
        {
            _logger.LogInformation("HandleResponse: Starting with paymentId={PaymentId}, status={Status}", paymentId, status);
            
            var payment = await _db.Payments.FindAsync(paymentId);
            if (payment == null)
            {
                _logger.LogWarning("HandleResponse: payment {PaymentId} not found in database", paymentId);
                return NotFound();
            }

            _logger.LogInformation("HandleResponse: Found payment {PaymentId}, current status={OldStatus}, updating to {NewStatus}", paymentId, payment.Status, status);

            // Update payment status based on gateway response
            if (string.Equals(status, "SUCCESS", StringComparison.OrdinalIgnoreCase))
            {
                payment.Status = "Success";

                // Update related order status to Paid
                var order = await _db.Orders.FindAsync(payment.OrderId);
                if (order != null)
                {
                    _logger.LogInformation("HandleResponse: Updating order {OrderId} status to Paid", payment.OrderId);
                    order.Status = "Paid";
                    _db.Orders.Update(order);
                }
                else
                {
                    _logger.LogWarning("HandleResponse: Order {OrderId} not found for payment {PaymentId}", payment.OrderId, paymentId);
                }
            }
            else if (string.Equals(status, "CANCELLED", StringComparison.OrdinalIgnoreCase))
            {
                payment.Status = "Cancelled";
            }
            else if (string.Equals(status, "TIMEOUT", StringComparison.OrdinalIgnoreCase))
            {
                payment.Status = "Timeout";
            }
            else
            {
                payment.Status = status ?? "Unknown";
            }

            // Mark payment as modified and save
            _db.Payments.Update(payment);
            _logger.LogInformation("HandleResponse: Marked payment {PaymentId} for update with status {Status}", paymentId, payment.Status);
            
            try
            {
                var changeCount = await _db.SaveChangesAsync();
                _logger.LogInformation("HandleResponse: SaveChangesAsync completed successfully. Changes saved: {ChangeCount}", changeCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HandleResponse: Exception occurred while saving payment {PaymentId}. Message: {ExceptionMessage}", paymentId, ex.Message);
                TempData["Error"] = "An error occurred while recording payment result: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }

            // Postcondition: Payment and Order statuses updated; show feedback
            _logger.LogInformation("HandleResponse: Payment {PaymentId} successfully processed with status {Status}", paymentId, payment.Status);
            return View("Result", payment);
        }

        // POST: /Payment/CancelPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelPayment(int paymentId)
        {
            _logger.LogInformation("CancelPayment: Starting cancellation for paymentId={PaymentId}", paymentId);
            
            var payment = await _db.Payments.FindAsync(paymentId);
            if (payment == null)
            {
                _logger.LogWarning("CancelPayment: payment {PaymentId} not found", paymentId);
                return NotFound();
            }

            // Check if payment can be cancelled (must not be Success or already Cancelled)
            if (payment.Status == "Success")
            {
                _logger.LogWarning("CancelPayment: cannot cancel payment {PaymentId} with status Success", paymentId);
                TempData["Error"] = "Cannot cancel a payment that has already been successfully processed.";
                return RedirectToAction(nameof(Result), new { paymentId });
            }

            // Check ownership
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var userId))
            {
                var order = await _db.Orders.FindAsync(payment.OrderId);
                if (order != null && order.StudentId != userId && !User.IsInRole("Manager"))
                {
                    _logger.LogWarning("CancelPayment: user {UserId} attempted to cancel payment {PaymentId} owned by {Owner}", userId, paymentId, order.StudentId);
                    return Forbid();
                }
            }

            // Update payment status to Cancelled
            payment.Status = "Cancelled";
            _db.Payments.Update(payment);
            
            try
            {
                var changeCount = await _db.SaveChangesAsync();
                _logger.LogInformation("CancelPayment: Payment {PaymentId} cancelled successfully. Changes saved: {ChangeCount}", paymentId, changeCount);
                TempData["Message"] = "Thanh toán đã bị huỷ thành công.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CancelPayment: Exception occurred while cancelling payment {PaymentId}. Message: {ExceptionMessage}", paymentId, ex.Message);
                TempData["Error"] = "Không thể huỷ thanh toán. Vui lòng thử lại.";
            }

            return RedirectToAction(nameof(Result), new { paymentId });
        }

        // POST: /Payment/ProcessTimeout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessTimeout(int paymentId)
        {
            var payment = await _db.Payments.FindAsync(paymentId);
            if (payment == null) return NotFound();

            payment.Status = "Timeout";
            _db.Payments.Update(payment);
            await _db.SaveChangesAsync();

            TempData["Message"] = "Payment timed out.";
            return RedirectToAction(nameof(Result), new { paymentId = payment.PaymentId });
        }

        // GET: /Payment/Result/{paymentId}
        public async Task<IActionResult> Result(int paymentId)
        {
            var payment = await _db.Payments.FindAsync(paymentId);
            if (payment == null) return NotFound();
            return View("Result", payment);
        }

        // GET: /Payment/List
        public IActionResult List()
        {
            // If user identity available, show user's payments; otherwise show all (or for Manager role show all)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var userId) && !User.IsInRole("Manager"))
            {
                var items = _db.Payments.Where(p => _db.Orders.Any(o => o.OrderId == p.OrderId && o.StudentId == userId)).OrderByDescending(p => p.PaymentId).ToList();
                return View(items);
            }

            var all = _db.Payments.OrderByDescending(p => p.PaymentId).ToList();
            return View(all);
        }
    }
}

