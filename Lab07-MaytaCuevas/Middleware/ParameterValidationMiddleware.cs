using Microsoft.AspNetCore.Mvc.Controllers;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Lab07_MaytaCuevas.Middleware;

public class ParameterValidationMiddleware
{
    private readonly RequestDelegate _next;
    
    public ParameterValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Method == HttpMethods.Post || context.Request.Method == HttpMethods.Put)
        {
            // Habilitar buffering para leer el body múltiples veces
            context.Request.EnableBuffering();
            
            var model = await DeserializeRequestBody(context);
            
            // Reiniciar la posición del stream
            context.Request.Body.Position = 0;
            
            if (model == null)
            {
                context.Response.StatusCode = 400; // Bad Request
                await context.Response.WriteAsync("Cuerpo de la solicitud inválido o vacío.");
                return;
            }
            
            var validationErrors = ValidateModel(model);
            if (validationErrors.Any())
            {
                context.Response.StatusCode = 400; // Bad Request
                await context.Response.WriteAsync(string.Join("\n", validationErrors));
                return;
            }
        }
        
        await _next(context);
    }
    
    private async Task<object> DeserializeRequestBody(HttpContext context)
    {
        var contentType = context.Request.ContentType;
        
        // Validar que contentType no sea null
        if (contentType != null && contentType.Contains("application/json"))
        {
            // leaveOpen: true para no cerrar el stream
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            
            // Reiniciar posición
            context.Request.Body.Position = 0;
            
            var dtoType = context.GetEndpoint()?.Metadata?.OfType<ControllerActionDescriptor>()
                .FirstOrDefault()?.MethodInfo?.GetParameters().FirstOrDefault()?.ParameterType;
            
            if (dtoType != null && !string.IsNullOrEmpty(body))
            {
                var model = JsonConvert.DeserializeObject(body, dtoType);
                return model;
            }
        }
        return null;
    }
    
    private List<string> ValidateModel(object model)
    {
        var errors = new List<string>();
        var properties = model.GetType().GetProperties();
        
        foreach (var property in properties)
        {
            // Verificar si tiene el atributo [Required]
            var requiredAttribute = property.GetCustomAttributes(typeof(RequiredAttribute), true)
                .FirstOrDefault() as RequiredAttribute;
            
            if (requiredAttribute != null)
            {
                var value = property.GetValue(model);
                
                // Validar null y strings vacíos
                if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
                {
                    errors.Add($"El parámetro '{property.Name}' es obligatorio.");
                }
            }
        }
        return errors;
    }
}