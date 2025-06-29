document.addEventListener('DOMContentLoaded', function() {
    // Xử lý hiển thị thông tin ngân hàng khi chọn chuyển khoản
    const bankTransfer = document.getElementById('bank');
    const bankInfo = document.getElementById('bankInfo');
    
    // Xử lý tất cả các radio button phương thức thanh toán
    const paymentMethods = document.querySelectorAll('input[name="PaymentMethod"]');
    const paymentItems = document.querySelectorAll('.payment-method-item');
    
    // Thêm class selected cho phương thức thanh toán được chọn
    function updateSelectedPayment() {
        paymentItems.forEach(item => {
            item.classList.remove('selected');
        });
        
        const checkedInput = document.querySelector('input[name="PaymentMethod"]:checked');
        if (checkedInput) {
            checkedInput.closest('.payment-method-item').classList.add('selected');
        }
        
        // Hiển thị thông tin ngân hàng nếu chọn chuyển khoản
        if (bankTransfer && bankTransfer.checked) {
            bankInfo.style.display = 'block';
        } else if (bankInfo) {
            bankInfo.style.display = 'none';
        }
    }
    
    // Thêm sự kiện khi thay đổi phương thức thanh toán
    if (paymentMethods) {
        paymentMethods.forEach(method => {
            method.addEventListener('change', updateSelectedPayment);
        });
    }
    
    // Gọi hàm lần đầu để thiết lập giao diện ban đầu
    updateSelectedPayment();
    
    // Xử lý form checkout
    const btnPlaceOrder = document.getElementById('btnPlaceOrder');
    const checkoutForm = document.getElementById('checkoutForm');
    const errorMessage = document.getElementById('errorMessage');
    
    if (btnPlaceOrder && checkoutForm) {
        btnPlaceOrder.addEventListener('click', function() {
            // Reset thông báo lỗi
            errorMessage.style.display = 'none';
            errorMessage.textContent = '';
            document.querySelectorAll('.field-validation-error').forEach(el => {
                el.textContent = '';
            });
            
            // Lấy dữ liệu từ form
            const formData = {
                CustomerName: document.getElementById('CustomerName').value.trim(),
                Address: document.getElementById('Address').value.trim(),
                Phone: document.getElementById('Phone').value.trim(),
                PaymentMethod: document.querySelector('input[name="PaymentMethod"]:checked').value
            };
            
            // Validate dữ liệu
            let isValid = true;
            
            if (!formData.CustomerName) {
                document.querySelector('[data-valmsg-for="CustomerName"]').textContent = 'Vui lòng nhập họ và tên';
                isValid = false;
            }
            
            if (!formData.Address) {
                document.querySelector('[data-valmsg-for="Address"]').textContent = 'Vui lòng nhập địa chỉ giao hàng';
                isValid = false;
            }
            
            if (!formData.Phone) {
                document.querySelector('[data-valmsg-for="Phone"]').textContent = 'Vui lòng nhập số điện thoại';
                isValid = false;
            } else if (!/^0\d{9}$/.test(formData.Phone)) {
                document.querySelector('[data-valmsg-for="Phone"]').textContent = 'Số điện thoại không hợp lệ';
                isValid = false;
            }
            
            if (!isValid) {
                return;
            }
            
            // Hiển thị loading
            btnPlaceOrder.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Đang xử lý...';
            btnPlaceOrder.disabled = true;
            
            // Lấy token CSRF
            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
            
            // Gửi request AJAX
            fetch('/Checkout/PlaceOrderAjax', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify(formData)
            })
            .then(response => {
                if (!response.ok) {
                    return response.json().then(data => {
                        throw new Error(data.message || 'Có lỗi xảy ra');
                    });
                }
                return response.json();
            })
            .then(data => {
                console.log('Server response:', data); // Debug response
                
                if (data.success) {
                    // Xử lý thành công
                    if (data.orderId) {
                        console.log('Redirecting to order confirmation page with ID:', data.orderId);
                        window.location.href = window.location.origin + '/Checkout/OrderConfirmation/' + data.orderId;
                    } else {
                        console.error('Missing orderId in response');
                        showErrorMessage('Không thể lấy mã đơn hàng');
                    }
                } else {
                    if (data.outOfStock) {
                        // Hiển thị thông báo lỗi tồn kho với định dạng HTML
                        let htmlContent = `
                            <div class="custom-alert-title">Số lượng sản phẩm vượt quá tồn kho hiện có</div>
                            <ul>
                        `;
                        
                        for (const [productName, stock] of Object.entries(data.invalidItems || {})) {
                            htmlContent += `<li><strong>${productName}:</strong> Chỉ còn <span class="badge bg-warning text-dark">${stock}</span> sản phẩm</li>`;
                        }
                        
                        htmlContent += `
                            </ul>
                            <div class="custom-alert-action">
                                <a href="/Cart">
                                    <i class="fas fa-shopping-cart"></i> Quay lại giỏ hàng và cập nhật số lượng
                                </a>
                            </div>
                        `;
                        
                        showErrorMessage(htmlContent, 'warning');
                    } else {
                        // Hiển thị thông báo lỗi khác
                        showErrorMessage(data.message || 'Có lỗi xảy ra khi xử lý đơn hàng');
                    }
                }
            })
            .catch(error => {
                console.error('Error:', error);
                showErrorMessage(error.message || 'Có lỗi xảy ra khi xử lý đơn hàng');
            });
        });
    }
    
    // Thay thế hàm showErrorMessage
    function showErrorMessage(message, type = 'error') {
        const errorMessage = document.getElementById('errorMessage');
        
        // Kiểm tra xem errorMessage có tồn tại không
        if (!errorMessage) {
            console.error('Element with id "errorMessage" not found');
            alert(message); // Hiển thị thông báo bằng alert nếu không tìm thấy phần tử
            return;
        }
        
        // Xóa tất cả class alert hiện tại
        errorMessage.className = 'custom-alert';
        // Thêm class type mới
        errorMessage.classList.add(type);
        
        // Tìm hoặc tạo phần tử errorMessageContent
        let errorMessageContent = document.getElementById('errorMessageContent');
        
        // Nếu không tìm thấy, kiểm tra xem có phần tử .custom-alert-content không
        if (!errorMessageContent) {
            errorMessageContent = errorMessage.querySelector('.custom-alert-content');
            
            // Nếu vẫn không tìm thấy, tạo mới phần tử
            if (!errorMessageContent) {
                const contentDiv = document.createElement('div');
                contentDiv.className = 'custom-alert-content';
                contentDiv.id = 'errorMessageContent';
                
                // Tìm vị trí để chèn phần tử mới
                const iconDiv = errorMessage.querySelector('.custom-alert-icon');
                if (iconDiv) {
                    iconDiv.parentNode.insertBefore(contentDiv, iconDiv.nextSibling);
                } else {
                    errorMessage.appendChild(contentDiv);
                }
                
                errorMessageContent = contentDiv;
            }
        }
        
        // Thay đổi icon tùy theo loại thông báo
        let icon = 'fa-exclamation-circle';
        if (type === 'warning') icon = 'fa-exclamation-triangle';
        if (type === 'info') icon = 'fa-info-circle';
        if (type === 'success') icon = 'fa-check-circle';
        
        // Cập nhật icon nếu tìm thấy
        const iconElement = errorMessage.querySelector('.custom-alert-icon i');
        if (iconElement) {
            iconElement.className = `fas ${icon}`;
        }
        
        // Hiển thị nội dung thông báo
        errorMessageContent.innerHTML = message;
        errorMessage.style.display = 'block';
        
        // Cuộn lên trên để người dùng thấy thông báo lỗi
        try {
            window.scrollTo({
                top: errorMessage.offsetTop - 100,
                behavior: 'smooth'
            });
        } catch (e) {
            window.scrollTo(0, 0); // Backup nếu scrollTo với options không được hỗ trợ
        }
    }
});