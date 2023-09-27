
using FProjectCamping.Models.EFModels;
using FProjectCamping.Models.ViewModels.Carts;
using FProjectCamping.Models.ViewModels.Rooms;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;




namespace FProjectCamping.Models.Carts
{
	public class CartApiController : ApiController
	{
		[HttpPost]

		public IHttpActionResult AddCartItem([FromBody] CartItemViewModel cartItemVm)
		{
			try
			{
				string currentUserAccount = User.Identity.Name;
				var db = new AppDbContext();


				Cart userCart = db.Carts.FirstOrDefault(c => c.MemberAccount == currentUserAccount);

				if (userCart == null)
				{
					userCart = new Cart
					{
						MemberAccount = currentUserAccount,
						TotalPrice = 0
					};
					db.Carts.Add(userCart);
				}


				var cartItems = new CartItem
				{
					CartId = 4,
					RoomId = cartItemVm.RoomId,
					CheckInDate = cartItemVm.CheckInDate,
					CheckOutDate = cartItemVm.CheckOutDate,
					ExtraBed = false,
					ExtraBedPrice = cartItemVm.ExtraBedPrice,
					Days = cartItemVm.Days,
					SubTotal = cartItemVm.SubTotal
				};

				userCart.CartItems.Add(cartItems);
				userCart.TotalPrice += cartItemVm.SubTotal;

				db.SaveChanges();


				var cartItemCount = userCart.CartItems.Count();
				return Ok(new { count = cartItemCount });
			}
			catch (Exception ex)
			{

				return InternalServerError(ex);
			}

		}
		[HttpGet]
		public IHttpActionResult GetOrderedRoomIds()
		{
			try
			{
				var db = new AppDbContext();

				// 获取已在 orderitem 中的 roomid 列表
				var orderedRoomIds = db.OrderItems.Select(oi => oi.RoomId).ToList();

				return Ok(orderedRoomIds);
			}
			catch (Exception ex)
			{
				return InternalServerError(ex);
			}
		}
	}
}