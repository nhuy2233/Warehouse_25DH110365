using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Warehouse_25DH110365.Helpers;
using Warehouse_25DH110365.Models;

namespace Warehouse_25DH110365.Controllers
{
    public class ProductsController : Controller
    {
        private Warehouse_25DH110365Entities db = new Warehouse_25DH110365Entities();

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        // ===== các action của từng câu thêm ở đây =====
        // ===== CÂU 2a: Danh sách =====
        public ActionResult Index()
        {
            var list = db.Products.OrderBy(p => p.ProductID).ToList();
            return View(list);
        }

        // ===== CÂU 2b: Sửa =====
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(400);
            var p = db.Products.Find(id.Value);
            if (p == null) return HttpNotFound();
            return View(p);
        }

        [HttpPost]
        public ActionResult Edit(int id, string ProductName, string Category, string Price)
        {
            var p = db.Products.Find(id);
            if (p == null) return HttpNotFound();

            var errors = new List<string>();
            string name = (ProductName ?? "").Trim();
            string cat = (Category ?? "").Trim();
            decimal price;

            if (name == "") errors.Add("Tên sản phẩm không được để trống.");
            else if (name.Length > 100) errors.Add("Tên sản phẩm tối đa 100 ký tự.");
            if (cat == "") errors.Add("Danh mục không được để trống.");
            else if (cat.Length > 100) errors.Add("Danh mục tối đa 100 ký tự.");
            if (!StockHelper.TryParsePrice(Price, out price) || price <= 0)
                errors.Add("Đơn giá phải là số lớn hơn 0.");

            if (errors.Count > 0)
            {
                ViewBag.Errors = errors;
                ViewBag.InName = ProductName;
                ViewBag.InCat = Category;
                ViewBag.InPrice = Price;
                return View(p);   // p chưa bị thay đổi, không lưu
            }

            // Chỉ sửa 3 trường; ProductID và Quantity giữ nguyên
            p.ProductName = name;
            p.Category = cat;
            p.Price = price;
            db.SaveChanges();

            TempData["Msg"] = "Đã cập nhật sản phẩm #" + p.ProductID + ".";
            return RedirectToAction("Index");
        }
        // ===== CÂU 3: Tra cứu =====
        public ActionResult Search(string keyword, string status, string sort)
        {
            var q = db.Products.AsQueryable();

            string kw = (keyword ?? "").Trim();
            if (kw != "")
            {
                string k = kw.ToLower();
                q = q.Where(p => p.ProductName.ToLower().Contains(k));
            }

            if (status == "out") q = q.Where(p => p.Quantity == 0);
            else if (status == "low") q = q.Where(p => p.Quantity >= 1 && p.Quantity <= 4);
            else if (status == "ok") q = q.Where(p => p.Quantity >= 5);

            if (sort == "asc") q = q.OrderBy(p => p.Price);
            else if (sort == "desc") q = q.OrderByDescending(p => p.Price);
            else q = q.OrderBy(p => p.ProductID);

            ViewBag.Keyword = keyword;
            ViewBag.Status = status;
            ViewBag.Sort = sort;
            return View(q.ToList());
        }
        // ===== CÂU 4: Kiểm kê 1 sản phẩm =====
        private void LoadProductList(int? selected = null)
        {
            var items = db.Products.OrderBy(p => p.ProductID).ToList()
                .Select(p => new { p.ProductID, Text = p.ProductID + " - " + p.ProductName + " (SL hiện tại: " + p.Quantity + ")" });
            ViewBag.ProductList = new SelectList(items, "ProductID", "Text", selected);
        }

        public ActionResult Stocktake()
        {
            LoadProductList();
            return View();
        }

        [HttpPost]
        public ActionResult Stocktake(int? productId, string actualQty)
        {
            LoadProductList(productId);
            ViewBag.ActualQty = actualQty;

            var p = productId == null ? null : db.Products.Find(productId.Value);
            if (p == null)
            {
                ViewBag.Error = "Sản phẩm không tồn tại.";
                return View();
            }

            int actual;
            if (!StockHelper.TryParseQty(actualQty, out actual))
            {
                ViewBag.Error = "Quantity thực tế phải là số nguyên không âm.";
                return View();
            }

            int old = p.Quantity;
            string diff;
            if (actual > old) diff = "Chênh lệch tăng (+" + (actual - old) + ")";
            else if (actual < old) diff = "Chênh lệch giảm (-" + (old - actual) + ")";
            else diff = "Không chênh lệch";

            p.Quantity = actual;
            db.SaveChanges();

            LoadProductList(productId);
            ViewBag.Result = p.ProductName + ": số lượng cũ " + old + " → thực tế " + actual
                             + ". Kết quả: " + diff + ". Tình trạng mới: " + StockHelper.GetStatus(actual) + ".";
            return View();
        }
        // ===== CÂU 5: Kiểm kê 2 sản phẩm =====
        private void LoadTwoLists(int? a, int? b)
        {
            var items = db.Products.OrderBy(p => p.ProductID).ToList()
                .Select(p => new { p.ProductID, Text = p.ProductID + " - " + p.ProductName + " (SL hiện tại: " + p.Quantity + ")" })
                .ToList();
            ViewBag.ListA = new SelectList(items, "ProductID", "Text", a);
            ViewBag.ListB = new SelectList(items, "ProductID", "Text", b);
        }

        public ActionResult Stocktake2()
        {
            LoadTwoLists(null, null);
            return View();
        }

        [HttpPost]
        public ActionResult Stocktake2(int? idA, string qtyA, int? idB, string qtyB)
        {
            LoadTwoLists(idA, idB);
            ViewBag.QtyA = qtyA;
            ViewBag.QtyB = qtyB;

            var errors = new List<string>();
            var a = idA == null ? null : db.Products.Find(idA.Value);
            var b = idB == null ? null : db.Products.Find(idB.Value);
            int na, nb;

            if (a == null) errors.Add("Sản phẩm A không tồn tại.");
            if (b == null) errors.Add("Sản phẩm B không tồn tại.");
            if (idA != null && idB != null && idA == idB) errors.Add("Hai sản phẩm phải khác nhau.");
            if (!StockHelper.TryParseQty(qtyA, out na)) errors.Add("Số lượng thực tế của A phải là số nguyên không âm.");
            if (!StockHelper.TryParseQty(qtyB, out nb)) errors.Add("Số lượng thực tế của B phải là số nguyên không âm.");

            // Có bất kỳ lỗi nào: KHÔNG cập nhật gì cả
            if (errors.Count > 0)
            {
                ViewBag.Errors = errors;
                return View();
            }

            int oldA = a.Quantity, oldB = b.Quantity;

            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    a.Quantity = na;
                    b.Quantity = nb;
                    db.SaveChanges();
                    tx.Commit();
                }
                catch (Exception)
                {
                    tx.Rollback();
                    ViewBag.Errors = new List<string> { "Lỗi khi cập nhật, đã hoàn tác cả hai sản phẩm." };
                    return View();
                }
            }

            LoadTwoLists(idA, idB);
            ViewBag.Result = "A (" + a.ProductName + "): " + oldA + " → " + na + " | B (" + b.ProductName + "): " + oldB + " → " + nb;
            return View();
        }
        // ===== CÂU 6: Thêm sản phẩm =====
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(string ProductName, string Category, string Price, string Quantity)
        {
            var errors = new List<string>();
            string name = (ProductName ?? "").Trim();
            string cat = (Category ?? "").Trim();
            decimal price;
            int qty;

            if (name == "") errors.Add("Tên sản phẩm không được để trống.");
            else if (name.Length > 100) errors.Add("Tên sản phẩm tối đa 100 ký tự.");
            if (cat == "") errors.Add("Danh mục không được để trống.");
            else if (cat.Length > 100) errors.Add("Danh mục tối đa 100 ký tự.");
            if (!StockHelper.TryParsePrice(Price, out price) || price <= 0)
                errors.Add("Đơn giá phải là số lớn hơn 0.");
            if (!StockHelper.TryParseQty(Quantity, out qty))
                errors.Add("Số lượng phải là số nguyên không âm.");

            // Kiểm tra trùng tên: không phân biệt hoa/thường, bỏ khoảng trắng 2 đầu
            if (name != "")
            {
                string key = name.ToLower();
                bool exists = db.Products.Any(p => p.ProductName.Trim().ToLower() == key);
                if (exists) errors.Add("Tên sản phẩm đã tồn tại.");
            }

            if (errors.Count > 0)
            {
                ViewBag.Errors = errors;
                ViewBag.InName = ProductName;
                ViewBag.InCat = Category;
                ViewBag.InPrice = Price;
                ViewBag.InQty = Quantity;
                return View();
            }

            var np = new Product { ProductName = name, Category = cat, Price = price, Quantity = qty };
            db.Products.Add(np);
            db.SaveChanges();

            TempData["Msg"] = "Đã thêm sản phẩm \"" + np.ProductName + "\" (ID " + np.ProductID + ").";
            return RedirectToAction("Index");
        }
        // ===== CÂU 7: Thống kê =====
        public ActionResult Stats()
        {
            ViewBag.Total = db.Products.Count();
            ViewBag.OutOfStock = db.Products.Count(p => p.Quantity == 0);
            ViewBag.TotalValue = db.Products.Sum(p => (decimal?)(p.Price * p.Quantity)) ?? 0m;
            return View();
        }
    }
}