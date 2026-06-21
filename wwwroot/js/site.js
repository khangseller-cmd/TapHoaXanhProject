// Tự động thực thi khi document sẵn sàng
$(document).ready(function () {
    // Thêm CSS animation cho cart badge
    $('<style>.cart-badge.bounce { animation: bounce 0.5s; } @keyframes bounce { 0%, 100% { transform: scale(1); } 50% { transform: scale(1.3); } }</style>').appendTo('head');

    // Xử lý khi click nút "Thêm vào giỏ hàng"
    $(document).on('click', '.add-to-cart-btn, .btn-add-to-cart, button[type="submit"][formaction*="AddToCart"]', function (e) {
        // Chỉ xử lý nếu là form AJAX
        var $form = $(this).closest('form');
        var actionUrl = $form.attr('action');

        if (actionUrl && actionUrl.includes('AddToCart')) {
            e.preventDefault();

            var productId = $(this).data('product-id');
            var quantity = $(this).data('quantity') || 1;

            // Nếu không có data attribute, tìm trong form
            if (!productId) {
                productId = $form.find('input[name="productId"]').val();
            }

            if (!quantity) {
                quantity = $form.find('input[name="quantity"]').val() || 1;
            }

            // Gửi AJAX request
            $.ajax({
                url: actionUrl,
                type: 'POST',
                data: {
                    productId: parseInt(productId),
                    quantity: parseInt(quantity)
                },
                headers: {
                    'X-Requested-With': 'XMLHttpRequest',
                    'RequestVerificationToken': $form.find('input[name="__RequestVerificationToken"]').val()
                },
                success: function (response) {
                    if (response.success) {
                        // Cập nhật số lượng giỏ hàng
                        updateCartCount(response.cartCount);

                        // Hiển thị thông báo thành công
                        showToast(response.message || 'Đã thêm vào giỏ hàng!', 'success');
                    } else {
                        showToast(response.message || 'Có lỗi xảy ra!', 'error');
                    }
                },
                error: function (xhr, status, error) {
                    console.error('Lỗi khi thêm vào giỏ:', error);
                    showToast('Có lỗi xảy ra khi thêm vào giỏ hàng!', 'error');
                }
            });
        }
        // Nếu không phải form AddToCart, để form submit bình thường
    });

    // Function cập nhật số lượng giỏ hàng
    function updateCartCount(count) {
        // Cập nhật badge
        var $badge = $('.cart-badge');
        if ($badge.length > 0) {
            $badge.text(count);

            // Thêm animation bounce
            $badge.addClass('bounce');
            setTimeout(function () {
                $badge.removeClass('bounce');
            }, 500);
        }

        // Nếu có element #cartCount, cập nhật nó
        var $cartCount = $('#cartCount');
        if ($cartCount.length > 0) {
            $cartCount.text(count);
        }
    }

    // Function hiển thị toast notification
    function showToast(message, type) {
        // Nếu có Toastify
        if (typeof Toastify !== 'undefined') {
            var bgColor = type === 'success'
                ? 'linear-gradient(to right, #00b09b, #96c93d)'
                : 'linear-gradient(to right, #ff5f6d, #ffc371)';

            Toastify({
                text: message,
                duration: 3000,
                gravity: "top",
                position: "right",
                backgroundColor: bgColor,
                stopOnFocus: true,
                className: "toast-notification"
            }).showToast();
        } else {
            // Fallback: dùng alert
            alert(message);
        }
    }

    // Xử lý xóa sản phẩm khỏi giỏ hàng (nếu cần)
    $(document).on('click', '.remove-from-cart', function (e) {
        e.preventDefault();

        var cartItemId = $(this).data('cart-item-id');
        var $row = $(this).closest('tr');

        if (confirm('Bạn có chắc muốn xóa sản phẩm này khỏi giỏ hàng?')) {
            $.ajax({
                url: '/Cart/RemoveFromCart',
                type: 'POST',
                data: {
                    cartItemId: cartItemId
                },
                headers: {
                    'X-Requested-With': 'XMLHttpRequest',
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                },
                success: function (response) {
                    if (response.success) {
                        $row.fadeOut(300, function () {
                            $(this).remove();
                        });
                        updateCartCount(response.cartCount);
                        showToast('Đã xóa sản phẩm khỏi giỏ hàng!', 'success');
                    } else {
                        showToast(response.message || 'Có lỗi xảy ra!', 'error');
                    }
                },
                error: function () {
                    showToast('Có lỗi xảy ra!', 'error');
                }
            });
        }
    });

    // Xử lý cập nhật số lượng trong giỏ hàng
    $(document).on('change', '.cart-item-quantity', function () {
        var $input = $(this);
        var cartItemId = $input.data('cart-item-id');
        var newQuantity = parseInt($input.val());

        if (newQuantity < 1) {
            $input.val(1);
            newQuantity = 1;
        }

        // Auto-save khi thay đổi số lượng (nếu cần)
        // Có thể thêm AJAX call ở đây
    });

    // Loading overlay
    $(document).ajaxStart(function () {
        $('#loadingOverlay').fadeIn(200);
    }).ajaxStop(function () {
        $('#loadingOverlay').fadeOut(200);
    });

    // Smooth scroll cho các link
    $('a[href^="#"]').on('click', function (e) {
        var target = $(this.getAttribute('href'));
        if (target.length) {
            e.preventDefault();
            $('html, body').stop().animate({
                scrollTop: target.offset().top - 100
            }, 800);
        }
    });

    // Auto-hide alerts sau 5 giây
    setTimeout(function () {
        $('.alert').fadeOut(500);
    }, 5000);
});

// Utility functions
window.TapHoaXanh = {
    // Format số tiền
    formatCurrency: function (amount) {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(amount);
    },

    // Debounce function cho search
    debounce: function (func, wait) {
        var timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }
};