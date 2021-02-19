using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PaymentAPI.Controllers
{
	public class PaymentsController : Controller
	{
		#region Methods

		/// <summary>
		/// Will return the user name and the associated claims
		/// </summary>
		/// <returns></returns>
		[Authorize]
		public IActionResult Get()
		{
			var result = new Result();

			if(this.User != null)
			{
				result.Name = this.User.Identity?.Name ?? "Unknown Name";
				result.Claims = (from c in this.User.Claims select c.Type + ":" + c.Value).ToList();
			}

			return new JsonResult(result);
		}

		#endregion
	}

	public class Result
	{
		#region Properties

		public List<string>? Claims { get; set; }
		public string? Name { get; set; }

		#endregion
	}
}