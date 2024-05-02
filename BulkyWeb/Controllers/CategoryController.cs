using BulkyWeb.Data;
using BulkyWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Controllers;

public class CategoryController : Controller
{
    private readonly ApplicationDbContext _db;
    public CategoryController(ApplicationDbContext db)
    {
        _db = db;
    }
    public IActionResult Index()
    {
        List<Category> objCategoryList = _db.Categories.ToList();
        return View(objCategoryList);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(Category obj)
    {
        
        if(obj.Name == obj.DisplayOrder.ToString())
        {
            ModelState.AddModelError("name", "The DisplayOrder cannot exactly match the Name.");
        }
        if (ModelState.IsValid) {
            await _db.Categories.AddAsync(obj);
            await _db.SaveChangesAsync();
            TempData["success"] = "Category created successfully";
            return RedirectToAction("Index");
        }
        return View();
    }

    public IActionResult Edit(int? id)
    {
        if (id is null || id == 0)
            return NotFound();
        Category categoryFromDb = _db.Categories.Find(id);
        if (categoryFromDb is null)
            return NotFound();
        return View(categoryFromDb);
    }

    [HttpPost]
    public async Task<IActionResult> EditAsync(Category obj)
    {
        if (ModelState.IsValid)
        {
            _db.Categories.Update(obj);
            await _db.SaveChangesAsync();
            TempData["success"] = "Category updated successfully";
            return RedirectToAction("Index");
        }
        return View();
    }

    public IActionResult Delete(int? id)
    {
        if (id is null || id == 0)
            return NotFound();
        Category categoryFromDb = _db.Categories.Find(id);
        if (categoryFromDb is null)
            return NotFound();
        return View(categoryFromDb);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteAsync(int? id)
    {
        Category obj = _db.Categories.Find(id);
        if (obj is null)
            return NotFound();

        _db.Categories.Remove(obj);
        _db.SaveChanges();
        TempData["success"] = "Category deleted successfully";
        return RedirectToAction("Index");
    }


}
