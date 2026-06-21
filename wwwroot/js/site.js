// ===== LOADING OVERLAY =====
function showLoading() {
    document.getElementById('loadingOverlay').classList.add('show');
}

function hideLoading() {
    document.getElementById('loadingOverlay').classList.remove('show');
}

// ===== TOAST NOTIFICATIONS =====
function showToast(message, type = 'success') {
    const colors = {
        success: '#00b14f',
        error: '#e53e3e',
        warning: '#f39c12',
        info: '#0984e3'
    };

    Toastify({
        text: message,
        duration: 3000,
        gravity: "top",
        position: "right",
        backgroundColor: colors[type] || colors.success,
        stopOnFocus: true,
        className: 'fade-in'
    }).showToast();
}

// ===== ADD TO CART AJAX =====
function addToCart(productId, quantity = 1) {
    showLoading();

    fetch(`/Cart/AddToCart?productId=${productId}&quantity=${quantity}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        }
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showToast('Đã thêm vào giỏ hàng!', 'success');
                updateCartCount(data.cartCount);
            } else {
                showToast('Có lỗi xảy ra!', 'error');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showToast('Có lỗi xảy ra!', 'error');
        })
        .finally(() => {
            hideLoading();
        });
}

// ===== UPDATE CART COUNT =====
function updateCartCount(count) {
    const cartBadge = document.getElementById('cartCount');
    if (cartBadge) {
        cartBadge.textContent = count;
        if (count > 0) {
            cartBadge.style.display = 'flex';
        } else {
            cartBadge.style.display = 'none';
        }
    }
}

// ===== CONFIRM DELETE =====
function confirmDelete(message = 'Bạn có chắc muốn xóa?') {
    return confirm(message);
}

// ===== AUTO HIDE ALERTS =====
document.addEventListener('DOMContentLoaded', function () {
    // Auto hide alerts after 5 seconds
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(alert => {
        setTimeout(() => {
            alert.style.opacity = '0';
            alert.style.transition = 'opacity 0.5s ease';
            setTimeout(() => alert.remove(), 500);
        }, 5000);
    });

    // Initialize cart count
    updateCartCountFromServer();
});

// ===== GET CART COUNT FROM SERVER =====
function updateCartCountFromServer() {
    fetch('/Cart/GetCartCount')
        .then(response => response.json())
        .then(data => {
            updateCartCount(data.count);
        })
        .catch(error => console.error('Error:', error));
}

// ===== SMOOTH SCROLL =====
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        e.preventDefault();
        const target = document.querySelector(this.getAttribute('href'));
        if (target) {
            target.scrollIntoView({
                behavior: 'smooth',
                block: 'start'
            });
        }
    });
});

// ===== FORM VALIDATION =====
function validateForm(formId) {
    const form = document.getElementById(formId);
    if (!form) return false;

    const inputs = form.querySelectorAll('input[required]');
    let isValid = true;

    inputs.forEach(input => {
        if (!input.value.trim()) {
            input.classList.add('is-invalid');
            isValid = false;
        } else {
            input.classList.remove('is-invalid');
        }
    });

    return isValid;
}

// ===== IMAGE PREVIEW =====
function previewImage(input, previewId) {
    if (input.files && input.files[0]) {
        const reader = new FileReader();
        reader.onload = function (e) {
            document.getElementById(previewId).src = e.target.result;
        };
        reader.readAsDataURL(input.files[0]);
    }
}

// ===== NUMBER FORMAT =====
function formatNumber(num) {
    return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ".");
}

// ===== COPY TO CLIPBOARD =====
function copyToClipboard(text) {
    navigator.clipboard.writeText(text).then(() => {
        showToast('Đã copy vào clipboard!', 'success');
    });
}

// ===== PRINT =====
function printElement(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        const printWindow = window.open('', '', 'height=600,width=800');
        printWindow.document.write(element.innerHTML);
        printWindow.document.close();
        printWindow.print();
    }
}