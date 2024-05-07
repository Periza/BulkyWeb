
using Microsoft.AspNetCore.Mvc;
using Bulky.DataAccess.Data;
using Bulky.Models;
using Bulky.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc.Rendering;
using Bulky.Models.ViewModels;

namespace BulkyWeb.Areas.Admin.Controllers;

[Area("Admin")]
public class ProductController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    public ProductController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public IActionResult Index()
    {
        List<Product> objProductList = _unitOfWork.Product.GetAll().ToList();
        
        return View(objProductList);
    }

    public IActionResult Upsert(int? id) //UpdateInsert
    {
        IEnumerable<SelectListItem> CategoryList = _unitOfWork.Category
            .GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });

        ProductVM productVM = new()
        {
            CategoryList = CategoryList,
            Product = new Product()
        };
        if(id is null or 0)
        {
            // Create
            return View(productVM);
        }
        else
        {
            // Update
            productVM.Product = _unitOfWork.Product.Get(u => u.Id == id);
            return View(productVM);
        }
        return View(productVM);
    }

    [HttpPost]
    public IActionResult Upsert(ProductVM productVm, IFormFile? file)
    {

        if (ModelState.IsValid)
        {
            _unitOfWork.Product.Add(productVm.Product);
            _unitOfWork.Save();
            TempData["success"] = "Product created successfully";
            return RedirectToAction("Index");
        }
        else
        {
            productVm.CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });
            return View();
        }
        
    }

    public IActionResult Delete(int? id)
    {
        if (id is null || id == 0)
            return NotFound();
        Product productFromDb = _unitOfWork.Product.Get(u => u.Id == id);
        if (productFromDb is null)
            return NotFound();
        return View(productFromDb);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteAsync(int? id)
    {
        Product obj = _unitOfWork.Product.Get(u => u.Id == id);
        if (obj is null)
            return NotFound();

        _unitOfWork.Product.Remove(obj);
        _unitOfWork.Save();
        TempData["success"] = "Product deleted successfully";
        return RedirectToAction("Index");
    }


}
