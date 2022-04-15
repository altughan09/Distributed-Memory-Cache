using Backend.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IDistributedCache _distributedCache;

        public EmployeesController(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        [HttpGet]
        public async Task<IEnumerable<Employee>> GetAsync()
        {
            // Find cached item
            byte[] objectFromCache = await _distributedCache.GetAsync("get_all_employees");

            if (objectFromCache != null)
            {
                // Deserialize it
                var jsonToDeserialize = System.Text.Encoding.UTF8.GetString(objectFromCache);
                var cachedResult = JsonSerializer.Deserialize<IEnumerable<Employee>>(jsonToDeserialize);
                if (cachedResult != null)
                {
                    // If found, then return it
                    return cachedResult;
                }
            }

            // If not found, then recalculate response
            var result = GetEmployees();

            // Serialize the response
            byte[] objectToCache = JsonSerializer.SerializeToUtf8Bytes(result);
            var cacheEntryOptions = new DistributedCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(30));

            // Cache it
            await _distributedCache.SetAsync("get_all_employees", objectToCache, cacheEntryOptions);

            return result;
        }

        private static IEnumerable<Employee> GetEmployees()
        {
            return new List<Employee>()
            {
                 new Employee() { Id = 1, Name = "John", Position = "Back-End Developer" },
                 new Employee() { Id = 2, Name = "Daniel", Position = "Front-End Developer" },
                 new Employee() { Id = 3, Name = "Steve", Position = "FullStack Developer" },
            };
        }
    }
}
