using Microsoft.AspNetCore.Mvc;

namespace HttpsEnforcementDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransferController : Controller
    {
        [HttpPost("Transfer")]
        public IActionResult Transfer([FromBody] TransferRequest model)
        {
            if (model == null || string.IsNullOrEmpty(model.AccountNumber) || model.Amount <= 0)
            {
                return BadRequest(new { Message = "بيانات التحويل غير صالحة." });
            }

            return Ok(new
            {
                Message = $"Transfer completed successfully!, Transfer: {model.Amount} to Account: {model.AccountNumber}"
            });
        }
    }

    public class TransferRequest
    {
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}