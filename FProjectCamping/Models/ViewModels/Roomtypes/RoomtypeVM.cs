using FProjectCamping.Models.EFModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FProjectCamping.Models.ViewModels.Roomtypes
{
    public class RoomtypeVM
	{
		public int RoomtypeId { get; set; }	
		public int Id { get; set; }
		public int RoomId { get; set; }
		public string RoomName { get; set; }
		public string Description { get; set; }
		public int WeekendPrice { get; set; }
		public int WeekdayPrice { get; set; }
		public string RoomTypeName { get; set; }
		public Photo FileName { get; set; }

	}
}