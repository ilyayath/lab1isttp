using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MerchDomain.Model;
using MerchInfrastructure;

namespace MerchInfrastructure.Controllers
{
    public class MerchOrdersController : Controller
    {
        private readonly MerchShopeContext _context;

        public MerchOrdersController(MerchShopeContext context)
        {
            _context = context;
        }

        // GET: MerchOrders
        public async Task<IActionResult> Index()
        {
            var merchShopeContext = _context.MerchOrders.Include(m => m.Buyer).Include(m => m.Payment).Include(m => m.Shipment).Include(m => m.Status);
            return View(await merchShopeContext.ToListAsync());
        }

        // GET: MerchOrders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var merchOrder = await _context.MerchOrders
                .Include(m => m.Buyer)
                .Include(m => m.Payment)
                .Include(m => m.Shipment)
                .Include(m => m.Status)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (merchOrder == null)
            {
                return NotFound();
            }

            return View(merchOrder);
        }

        // GET: MerchOrders/Create
        public IActionResult Create()
        {
            ViewData["BuyerId"] = new SelectList(_context.Buyers, "Id", "Username");
            ViewData["PaymentId"] = new SelectList(_context.Payments, "Id", "TypePayment");
            ViewData["ShipmentId"] = new SelectList(_context.Shipments, "Id", "TypeShipment");
            ViewData["StatusId"] = new SelectList(_context.OrderStatuses, "Id", "StatusName");
            return View();
        }

        // POST: MerchOrders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,ShipmentId,PaymentId,StatusId,OrderDate,Id")] MerchOrder merchOrder)
        {
            if (ModelState.IsValid)
            {
                _context.Add(merchOrder);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Buyers, "Id", "Username", merchOrder.BuyerId);
            ViewData["PaymentId"] = new SelectList(_context.Payments, "Id", "TypePayment", merchOrder.PaymentId);
            ViewData["ShipmentId"] = new SelectList(_context.Shipments, "Id", "TypeShipment", merchOrder.ShipmentId);
            ViewData["StatusId"] = new SelectList(_context.OrderStatuses, "Id", "StatusName", merchOrder.StatusId);
            return View(merchOrder);
        }

        // GET: MerchOrders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var merchOrder = await _context.MerchOrders.FindAsync(id);
            if (merchOrder == null)
            {
                return NotFound();
            }
            ViewData["BuyerId"] = new SelectList(_context.Buyers, "Id", "Username", merchOrder.BuyerId);
            ViewData["PaymentId"] = new SelectList(_context.Payments, "Id", "TypePayment", merchOrder.PaymentId);
            ViewData["ShipmentId"] = new SelectList(_context.Shipments, "Id", "TypeShipment", merchOrder.ShipmentId);
            ViewData["StatusId"] = new SelectList(_context.OrderStatuses, "Id", "StatusName", merchOrder.StatusId);
            return View(merchOrder);
        }

        // POST: MerchOrders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BuyerId,ShipmentId,PaymentId,StatusId,OrderDate,Id")] MerchOrder merchOrder)
        {
            if (id != merchOrder.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(merchOrder);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MerchOrderExists(merchOrder.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BuyerId"] = new SelectList(_context.Buyers, "Id", "Username", merchOrder.BuyerId);
            ViewData["PaymentId"] = new SelectList(_context.Payments, "Id", "TypePayment", merchOrder.PaymentId);
            ViewData["ShipmentId"] = new SelectList(_context.Shipments, "Id", "TypeShipment", merchOrder.ShipmentId);
            ViewData["StatusId"] = new SelectList(_context.OrderStatuses, "Id", "StatusName", merchOrder.StatusId);
            return View(merchOrder);
        }

        // GET: MerchOrders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var merchOrder = await _context.MerchOrders
                .Include(m => m.Buyer)
                .Include(m => m.Payment)
                .Include(m => m.Shipment)
                .Include(m => m.Status)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (merchOrder == null)
            {
                return NotFound();
            }

            return View(merchOrder);
        }

        // POST: MerchOrders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var merchOrder = await _context.MerchOrders.FindAsync(id);
            if (merchOrder != null)
            {
                _context.MerchOrders.Remove(merchOrder);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MerchOrderExists(int id)
        {
            return _context.MerchOrders.Any(e => e.Id == id);
        }
    }
}
