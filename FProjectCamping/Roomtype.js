
const urlParams = new URLSearchParams(window.location.search);
const roomTypeId = urlParams.get('roomTypeId');


if (roomTypeId === '1') {

document.getElementById('doubleRoomInfo').style.display = 'block';

document.getElementById('otherRoomInfo1').style.display = 'none';
document.getElementById('otherRoomInfo2').style.display = 'none';
document.getElementById('otherRoomInfo3').style.display = 'none';

} else if (roomTypeId === '2') {

document.getElementById('otherRoomInfo1').style.display = 'block';

document.getElementById('doubleRoomInfo').style.display = 'none';
document.getElementById('otherRoomInfo2').style.display = 'none';
document.getElementById('otherRoomInfo3').style.display = 'none';

} else if (roomTypeId === '3') {

document.getElementById('otherRoomInfo2').style.display = 'block';

document.getElementById('doubleRoomInfo').style.display = 'none';
document.getElementById('otherRoomInfo1').style.display = 'none';
document.getElementById('otherRoomInfo3').style.display = 'none';

} else if (roomTypeId === '4') {
document.getElementById('otherRoomInfo3').style.display = 'block';

document.getElementById('doubleRoomInfo').style.display = 'none';
document.getElementById('otherRoomInfo1').style.display = 'none';
document.getElementById('otherRoomInfo2').style.display = 'none';
} else {

}


const roomtypeDropdown = document.getElementById('RoomtypeDropdown');


window.addEventListener('load', function () {
fetch('/api/RoomsApi/')
.then(response => response.json())
.then(data => {
console.log(data);
roomtypeDropdown.innerHTML = '';

const defaultOption = document.createElement('option');
defaultOption.value = '0';
defaultOption.text = '請選擇房型';
roomtypeDropdown.append(defaultOption);

for (let i = 0; i < data.length; i++) {
const option = document.createElement('option');
option.value = data[i].value;
option.text = data[i].name;
roomtypeDropdown.append(option);
}
})
.catch(error => {
console.error(error);
});
});








let checkInDate;
let checkOutDate;
let daysDifference;

const arDateInput = document.getElementById('ar_date');
const deDateInput = document.getElementById('de_date');

document.addEventListener("DOMContentLoaded", function () {
var 今天 = new Date();
今天.setDate(今天.getDate() + 1);
arDateInput.min = 今天.toISOString().split("T")[0];

arDateInput.addEventListener("change", function () {
var selectedDate = new Date(arDateInput.value);
checkInDate = selectedDate;
var nextDay = new Date(selectedDate);
nextDay.setDate(selectedDate.getDate() + 1);
deDateInput.value = nextDay.toISOString().split("T")[0];

var maxDate = new Date(selectedDate);
maxDate.setDate(selectedDate.getDate() + 14);
deDateInput.max = maxDate.toISOString().split("T")[0];
});
});

    $(document).ready(function () {
        // 获取按钮元素
        const btnOrder = $(".btnOrder");

        // 初始化为全局变量
        let selectedRoomType;
        let checkInDateValue;
        let checkOutDateValue;

        // 初始时禁用按钮
        btnOrder.prop('disabled', true);

        // 监听 RoomtypeDropdown、ar_date 和 de_date 字段的变化
        $("#RoomtypeDropdown, #ar_date, #de_date, #booking").on("change", function () {
            selectedRoomType = $("#RoomtypeDropdown").val();
            checkInDateValue = arDateInput.value;
            checkOutDateValue = deDateInput.value;

            // 如果所有字段都已选择，启用按钮，否则禁用按钮
            if (selectedRoomType && checkInDateValue && checkOutDateValue && $("#booking").is(":checked")) {
                btnOrder.prop('disabled', false);
            } else {
                btnOrder.prop('disabled', true);
            }
        });

        // 添加按钮点击事件监听器
btnOrder.click(function () {
// 检查是否所有字段都已选择
if (selectedRoomType && checkInDateValue && checkOutDateValue) {
const extraBed = false;

// 检查退房日期是否大于入住日期
if (new Date(checkOutDateValue) <= new Date(checkInDateValue)) {
alert('退房日期必须晚于入住日期');
return; // 不继续执行订单提交
}

// 计算天数
const timeDifference = new Date(checkOutDateValue) - new Date(checkInDateValue);
daysDifference = Math.floor(timeDifference / (1000 * 60 * 60 * 24));

const requestData = {
RoomId: 3,
CheckInDate: checkInDateValue,
CheckOutDate: checkOutDateValue,
ExtraBed: extraBed,
Days: daysDifference,
};

$.ajax({
url: '/api/CartApi',
type: 'POST',
contentType: 'application/json',
data: JSON.stringify(requestData),
success: function (data) {
console.log('成功响应:', data);
updateCartItemCount(data.count);
alert('已加入購物車');
},
error: function (xhr, textStatus, errorThrown) {
console.error('發生錯誤:', errorThrown);
}
});
} else {
// 如果有任何一个字段未选择，禁用按钮或显示错误消息
alert('请先选择所有必填字段');
// 或者禁用按钮的方式如下：
// btnOrder.prop('disabled', true);
}
});

        // 在执行搜索后启用 "加入訂單" 按钮
        $("#booking").click(function () {
            // 执行搜索的逻辑

            // 启用 "加入訂單" 按钮
            btnOrder.prop('disabled', false);
        });
    });
function updateCartItemCount(count) {
var cartItemCountElement = document.getElementById('cartItemCount');
if (cartItemCountElement) {
cartItemCountElement.textContent = count;
}
}



    $(document).ready(function () {
        // 获取按钮元素
        const booking = $("#booking");

        // 添加按钮点击事件监听器
        booking.click(function () {
            // 发送 AJAX 请求以获取已在 orderitem 中的 roomid 列表
            $.ajax({
                url: '/api/CartApi/GetOrderedRoomIds', // 替换为实际的 API 路由
                type: 'GET',
                success: function (orderedRoomIds) {
                    console.log('已订购的房间 ID 列表:', orderedRoomIds);

                    // 在这里处理已订购的房间 ID 数据
                    // 根据返回的 roomid 列表来过滤显示的房型
                    $(".btnOrder").each(function () {
                        const roomId = $(this).data("roomid");
                        if (orderedRoomIds.includes(roomId)) {
                            $(this).hide(); // 隐藏已订购的房型的按钮
                        }
                    });
                },
                error: function (xhr, textStatus, errorThrown) {
                    console.error('发生错误:', errorThrown);
                }
            });
        });
    });





    $("#booking").click(function () {
        // 发送请求以获取尚未预订的房型
        $.ajax({
            url: '/api/CartApi/GetAvailableRooms', // 请求的 URL
            type: 'GET', // 使用 GET 请求
            success: function (availableRoomIds) {
                console.log('尚未预订的房型:', availableRoomIds);

                // 获取页面上所有房型元素
                var roomElements = $(".room"); // 假设房型元素具有 "room" 类

                // 遍历每个房型元素
                roomElements.each(function () {
                    var roomId = $(this).data("room-id"); // 假设房型元素有一个 data 属性来存储房型 ID
                    if (availableRoomIds.includes(roomId)) {
                        // 如果房型 ID 在尚未预订的列表中，显示该房型
                        $(this).show();
                    } else {
                        // 否则，隐藏该房型
                        $(this).hide();
                    }
                });
            },
            error: function (xhr, textStatus, errorThrown) {
                console.error('发生错误:', errorThrown);
            }
        });
    });
