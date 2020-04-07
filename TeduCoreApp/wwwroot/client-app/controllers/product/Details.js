var ProductDetailController = function() {
    this.initialize = function() {
        registerEvent();
    }

    function registerEvent() {
        $("#btnAddToCart").on('click', function(e) {
            e.preventDefault();
            var colorId = $("#ddlColorId").val();
            var sizeId = $("#ddlSizeId").val();
            var id = parseInt($(this).data("id"));
            $.ajax({
                url: "/Cart/AddToCart",
                type: 'post',
                dataType: 'json',
                data: {
                    productId: id,
                    quantity: parseInt($("#txtQuantity").val()),
                    color: colorId,
                    size: sizeId
                },
                success: function () {
                    tedu.notify("Add to cart Successful !", "seccess");
                    loadHeaderCart();
                }
            });
        });
    }

    function loadHeaderCart() {
        $("#headerCart").load("/AjaxContent/HeaderCart");
    }
}