
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
				string currentUserAccount = User.Identity.Name; // 替换为实际的用户身份验证或会话信息获取方法
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
					CartId = cartItemVm.CartId,
					RoomId = 1,
					CheckInDate = cartItemVm.CheckInDate,
					CheckOutDate = cartItemVm.CheckOutDate,
					ExtraBed = false,
					ExtraBedPrice = cartItemVm.ExtraBedPrice,
					Days = 3,
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
		public IHttpActionResult GetAvailableRooms()
		{
			try
			{
				var db = new AppDbContext();

				// 获取已订购的房型ID列表
				var OrderItemRoomIds = db.OrderItems.Select(o => o.RoomId).ToList();

				// 获取尚未订购的房型
				var availableRooms = db.Rooms.Where(r => !OrderItemRoomIds.Contains(r.Id)).ToList();

				// 只返回房型信息而不包括其他属性
				var availableRoomIds = availableRooms.Select(r => r.Id).ToList();

				return Ok(availableRoomIds);
			}
			catch (Exception ex)
			{
				return InternalServerError(ex);
			}
		}
	}
}