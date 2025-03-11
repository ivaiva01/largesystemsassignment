using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using apiCleaner.Models;
using apiCleaner.Services;
using apiCleaner.Dtos;

namespace apiCleaner.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CleanerService : ControllerBase
    {
        private readonly RabbitMqService _rabbitMqService;

        // Constructor injection for RabbitMqService
        public CleanerService(RabbitMqService rabbitMqService) 
        {
            _rabbitMqService = rabbitMqService;
        }

        [HttpGet]
        [Route("getdata")]
        public async Task<ResponseDto> GetData(string searchTerm)
        {
            List<Email> allLayerElements;
            allLayerElements = await _rabbitMqService.GetEmailsWithSearch(searchTerm);

            return new ResponseDto
            {
                MessageToClient = "All emails with matching results",
                ResponseData = allLayerElements
            };
        }
    }
}