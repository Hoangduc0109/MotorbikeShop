document.addEventListener('DOMContentLoaded', function() {
    // Xử lý các alert messages
    const alerts = document.querySelectorAll('.alert-dismissible');
    if (alerts.length > 0) {
        alerts.forEach(alert => {
            setTimeout(() => {
                const closeButton = alert.querySelector('.btn-close');
                if (closeButton) {
                    closeButton.click();
                }
            }, 5000);
        });
    }
    
    // Kích hoạt tooltip nếu có
    if (typeof bootstrap !== 'undefined') {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl)
        });
    }

    if (window.location.pathname.includes('/Admin/Motorbike')) {
        // Alert tự động biến mất sau 5 giây
        const alerts = document.querySelectorAll('.alert-dismissible');
        if (alerts.length > 0) {
            alerts.forEach(alert => {
                setTimeout(() => {
                    const closeButton = alert.querySelector('.btn-close');
                    if (closeButton) {
                        closeButton.click();
                    }
                }, 5000);
            });
        }

        // Xử lý format số tiền trong bảng
        const priceElements = document.querySelectorAll('.price-format');
        if (priceElements.length > 0) {
            priceElements.forEach(el => {
                const price = parseFloat(el.textContent);
                if (!isNaN(price)) {
                    el.textContent = price.toLocaleString('vi-VN') + ' VNĐ';
                }
            });
        }
        
        // Preview hình ảnh khi upload
        const imageInput = document.getElementById('imageFile');
        const imagePreview = document.getElementById('imagePreview');
        
        if (imageInput && imagePreview) {
            imageInput.addEventListener('change', function() {
                if (this.files && this.files[0]) {
                    const reader = new FileReader();
                    reader.onload = function(e) {
                        imagePreview.src = e.target.result;
                        imagePreview.style.display = 'block';
                    }
                    reader.readAsDataURL(this.files[0]);
                }
            });
        }
    }

    // Cấu hình toastr nếu có
    if (typeof toastr !== 'undefined') {
        toastr.options = {
            "closeButton": true,
            "progressBar": true,
            "positionClass": "toast-top-center",
            "timeOut": "5000"
        };
        
        // Hiển thị thông báo từ TempData nếu có
        var successMessage = document.getElementById('successMessage');
        if (successMessage) {
            toastr.success(successMessage.value);
        }
        
        var errorMessage = document.getElementById('errorMessage');
        if (errorMessage) {
            toastr.error(errorMessage.value);
        }
    }

    // Xử lý lỗi Bootstrap Alert
    // Ngăn chặn lỗi khi alert đã bị xóa
    if (typeof $ !== 'undefined' && typeof $.fn.alert !== 'undefined') {
        $(document).on('closed.bs.alert', '.alert', function(e) {
            e.stopPropagation();
        });
        
        // Kiểm tra phần tử tồn tại trước khi gọi remove
        if ($.fn.alert.Constructor) {
            var originalClose = $.fn.alert.Constructor.prototype.close;
            $.fn.alert.Constructor.prototype.close = function() {
                if (this._element && document.contains(this._element)) {
                    originalClose.call(this);
                }
            };
        }
    }
});
