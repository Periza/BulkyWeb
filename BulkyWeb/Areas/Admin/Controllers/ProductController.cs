
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

    public IActionResult Create()
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
        return View(productVM);
    }

    [HttpPost]
    public IActionResult Create(ProductVM productVm)
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

    public IActionResult Edit(int? id)
    {
        if (id is null || id == 0)
            return NotFound();
        Product productFromDb = _unitOfWork.Product.Get(u => u.Id == id);
        if (productFromDb is null)
            return NotFound();
        return View(productFromDb);
    }

    [HttpPost]
    public IActionResult Edit(Product obj)
    {
        if (ModelState.IsValid)
        {
            _unitOfWork.Product.Update(obj);
            _unitOfWork.Save();
            TempData["success"] = "Product updated successfully";
            return RedirectToAction("Index");
        }
        return View();
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
