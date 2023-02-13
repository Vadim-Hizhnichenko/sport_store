using Business.Contracts;
using Data.ContextDb;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.ViewModels;

namespace WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
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
            return View();
        }

        /// <summary>
        /// Get Method
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Upsert(int? id)
        {

            ProductViewModel productViewModel = new ProductViewModel()
            {
                Product = new Product(),
                CategorySelectItems = GetSelectCategoryListItmes(),

            };
            
            if (id == null || id == 0)
            {
                return View(productViewModel);
            }

            else
            {
                productViewModel.Product = _unitOfWork.ProductRepository.GetFirstOrDifault(p => p.Id == id);
                return View(productViewModel);
            }
        }

        /// <summary>
        /// View bag for select list of category
        /// </summary>
        /// <returns></returns>
        private IEnumerable<SelectListItem> GetSelectCategoryListItmes()
        {
            IEnumerable<SelectListItem> result = _unitOfWork.CategoryRepository.GetAll().Select(
                c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                });

            return result;
        }


        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductViewModel productViewModel,IFormFile? file)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    if (file != null)
                    {
                        var rootPath = _webHostEnvironment.WebRootPath;
                        var fileName = Path.GetFileName(file.FileName);
                        var uploadPath = Path.Combine(rootPath, @"images\products\");

                        // delete image for update 
                        if (productViewModel.Product.ImageUrl != null)
                        {
                            DeleteExsistImage(productViewModel, rootPath);
                        }
                        
                        using var fileStream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create);
                        file.CopyTo(fileStream);

                        productViewModel.Product.ImageUrl = @"\images\products\" + fileName;
                    }

                    if (productViewModel.Product.Id == 0)
                    {
                        _unitOfWork.ProductRepository.Add(productViewModel.Product);
                    }

                    else
                    {
                        _unitOfWork.ProductRepository.Update(productViewModel.Product);
                    }

                    _unitOfWork.Save();
                    TempData["success"] = "Product created successfully";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception)
            {

                throw new FileLoadException();
            }

            return View(productViewModel);
        }

        private void DeleteExsistImage(ProductViewModel productViewModel, string rootPath)
        {
            var trimImagePath = productViewModel.Product.ImageUrl.TrimStart('\\');
            var imagePath = Path.Combine(rootPath, trimImagePath);

            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
        }

        /// <summary>
        /// Get Api Method for Data Table to get all items
        /// </summary>
        /// <returns></returns>
        public IActionResult GetAll()
        {
            var productList = _unitOfWork.ProductRepository.GetAll(includeProperties: "Category");
            return Json(new { data = productList });
        }

        /// <summary>
        /// Get Api Method for Data Table to delete item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var product = _unitOfWork.ProductRepository.GetFirstOrDifault(c => c.Id == id);

            if (product == null)
            {
                return Json(new {success = false, message = "Can not deleting"});
            }

            var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageUrl.TrimStart('\\'));

            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            _unitOfWork.ProductRepository.Delete(product);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Product was deleting!" });
        }

    }
}
