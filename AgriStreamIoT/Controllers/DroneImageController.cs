using AgriStreamIoT.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgriStreamIoT.Controllers
{
    public class DroneImagesController : Controller
    {
        private readonly BlobStorageService _blobStorageService;

        public DroneImagesController(
            BlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                ViewBag.Message = "Please select an image.";
                return View("Index");
            }

            string extension = Path.GetExtension(image.FileName)
                .ToLowerInvariant();

            string[] allowedExtensions =
                { ".jpg", ".jpeg", ".png", ".webp" };

            if (!allowedExtensions.Contains(extension))
            {
                ViewBag.Message = "Only JPG, PNG and WEBP images are allowed.";
                return View("Index");
            }

            string fileName = Guid.NewGuid() + extension;

            using var stream = image.OpenReadStream();

            string imageUrl = await _blobStorageService.UploadImageAsync(
                stream, fileName);

            ViewBag.Message = "Image uploaded successfully!";
            ViewBag.ImageUrl = imageUrl;

            return View("Index");
        }
    }
}