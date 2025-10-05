using Microsoft.AspNetCore.Mvc;
using Lab07_MaytaCuevas.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Lab07_MaytaCuevas.Controllers;

[ApiController]
[Route("api/products")]
[Authorize(Roles = "Admin")]
public class ProductController : ControllerBase
{
    [HttpPost]
    public IActionResult CreateProduct([FromBody] CreateProductDto product)
    {
        // Forzar un error para probar el middleware
        // throw new Exception("Error simulado para prueba");
        
        return Ok(new { message = "Producto creado exitosamente." });
    }
}