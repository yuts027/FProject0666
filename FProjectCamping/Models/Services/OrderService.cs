﻿
using FProjectCamping.Models.EFModels;
using FProjectCamping.Models.ViewModels.Carts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FProjectCamping.Models.Services
{
    public class OrderService
    {
        public  void CreateOrder(string account, CartVm cart, CheckoutVm vm)
        {
            // todo : 拉到Repo
            var db = new AppDbContext();
            var memberId = db.Members.First(m => m.Account == account).Id;

            var order = new Order
            {
                MemberId = memberId,
                Name = vm.Name,
                PhoneNum = vm.PhoneNum,
                // Email = vm.Email, 缺少...todo
                OrderTime = DateTime.Now,
                // Coupon = vm.Coupon, 缺少...todo..允許Null
                Status = 1, // todo : 建立enum
                PaymentTypeId = Convert.ToInt32(vm.PaymnetType),
                TotalPrice = cart.TotalPrice,
            };
            // 新增訂單明細
            foreach (var item in cart.Items)
            {
                var orderItem = new OrderItem
                {
                    RoomId = item.RoomId,
                    Days = item.Days,
                    CheckInDate = Convert.ToDateTime(item.CheckInDate),
                    CheckOutDate = Convert.ToDateTime(item.CheckOutDate),
                    ExtraBed = item.ExtraBed,
                    ExtraBedPrice = item.ExtraBedPrice,
                    SubTotal = item.SubTotal
                };
                order.OrderItems.Add(orderItem);
            }
            db.Orders.Add(order);
            db.SaveChanges();
        }
    }
}