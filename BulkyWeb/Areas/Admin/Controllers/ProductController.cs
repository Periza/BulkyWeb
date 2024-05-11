﻿
using Microsoft.AspNetCore.Mvc;
using Bulky.DataAccess.Data;
using Bulky.Models;
using Bulky.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc.Rendering;
using Bulky.Models.ViewModels;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace BulkyWeb.Areas.Admin.Controllers;

[Area("Admin")]
public class ProductController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _webHostEnvironment;
    public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
    {
        _unitOfWork = unitOfWork;
        _webHostEnvironment = webHostEnvironment;

    }
    public IActionResult Index()
    {
        List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
        
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
    }

    [HttpPost]
    public IActionResult Upsert(ProductVM productVm, IFormFile? file)
    {

        if (ModelState.IsValid)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;

            if(file is not null)
            {
                string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string productPath = Path.Combine(wwwRootPath, @"images\product");

                if(!string.IsNullOrEmpty(productVm.Product.ImageUrl))
                {
                    // delete the old image
                    var oldImagePath = Path.Combine(wwwRootPath, productVm.Product.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }

                using FileStream fileSteram = new(Path.Combine(productPath, filename), FileMode.Create);

                file.CopyTo(fileSteram);

                productVm.Product.ImageUrl = @"\images\product\" + filename;

            }

            if (productVm.Product.Id == 0)
                _unitOfWork.Product.Add(productVm.Product);
            else
                _unitOfWork.Product.Update(productVm.Product);

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

    #region API CALLS
    [HttpGet]
    public IActionResult GetAll()
    {
        List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
        return Json( new { data = objProductList });
    }
    #endregion


}
