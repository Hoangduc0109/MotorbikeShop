document.addEventListener('DOMContentLoaded', function() {
    console.log('Cart script loaded');
    
    // Xử lý tăng số lượng
    document.querySelectorAll('.quantity-btn.increase').forEach(button => {
        button.addEventListener('click', function() {
            const row = this.closest('.cart-item');
            const motorbikeId = row.dataset.id;
            const input = row.querySelector('.quantity-input');
            const currentQuantity = parseInt(input.value);
            updateQuantity(motorbikeId, currentQuantity + 1);
        });
    });

    // Xử lý giảm số lượng
    document.querySelectorAll('.quantity-btn.decrease').forEach(button => {
        button.addEventListener('click', function() {
            const row = this.closest('.cart-item');
            const motorbikeId = row.dataset.id;
            const input = row.querySelector('.quantity-input');
            const currentQuantity = parseInt(input.value);
            if (currentQuantity > 1) {
                updateQuantity(motorbikeId, currentQuantity - 1);
            }
        });
    });

    // Xử lý xóa sản phẩm khỏi giỏ hàng
    document.querySelectorAll('.btn-remove').forEach(button => {
        button.addEventListener('click', function() {
            const motorbikeId = this.dataset.id;
            removeFromCart(motorbikeId);
        });
    });

    // Hàm cập nhật số lượng
    function updateQuantity(motorbikeId, quantity) {
        console.log('Updating quantity:', motorbikeId, quantity);
        
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (!token) {
            console.error("CSRF token không tồn tại!");
            return;
        }
        
        fetch('/Cart/UpdateQuantity', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify({
                motorbikeId: motorbikeId,
                quantity: quantity
            })
        })
        .then(response => {
            if (!response.ok) {
                throw new Error('Lỗi mạng: ' + response.status);
            }
            return response.json();
        })
        .then(data => {
            console.log('Success:', data);
            if (data.success) {
                // Cập nhật UI
                const row = document.querySelector(`.cart-item[data-id="${motorbikeId}"]`);
                if (row) {
                    if (quantity <= 0) {
                        // Xóa hàng nếu số lượng = 0
                        row.remove();
                        checkEmptyCart();
                    } else {
                        // Cập nhật hiển thị số lượng và tổng tiền
                        row.querySelector('.quantity-input').value = quantity;
                        row.querySelector('.product-total').textContent = formatCurrency(data.itemTotal);
                    }
                }
                
                // Cập nhật tổng giỏ hàng
                document.getElementById('cartTotal').textContent = formatCurrency(data.cartTotal);
                
                // Cập nhật số lượng trên icon giỏ hàng
                updateCartCount(data.totalItems);
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Không thể cập nhật số lượng. Vui lòng thử lại!');
        });
    }

    // Hàm xóa sản phẩm khỏi giỏ hàng
    function removeFromCart(motorbikeId) {
        if (confirm('Bạn có chắc chắn muốn xóa sản phẩm này khỏi giỏ hàng?')) {
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            if (!token) {
                console.error("CSRF token không tồn tại!");
                return;
            }
            
            fetch('/Cart/RemoveFromCart', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify({
                    motorbikeId: motorbikeId
                })
            })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Lỗi mạng: ' + response.status);
                }
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    // Xóa hàng khỏi bảng
                    const row = document.querySelector(`.cart-item[data-id="${motorbikeId}"]`);
                    if (row) {
                        row.remove();
                    }
                    
                    // Kiểm tra giỏ hàng trống
                    checkEmptyCart();
                    
                    // Cập nhật số lượng trên icon giỏ hàng
                    updateCartCount(data.totalItems);
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert('Không thể xóa sản phẩm. Vui lòng thử lại!');
            });
        }
    }

    // Kiểm tra nếu giỏ hàng trống
    function checkEmptyCart() {
        const cartItems = document.querySelectorAll('.cart-item');
        if (cartItems.length === 0) {
            // Nếu không còn sản phẩm nào, hiển thị giỏ hàng trống
            document.querySelector('.cart-content').innerHTML = `
                <div class="empty-cart">
                    <div class="empty-cart-content">
                        <i class="fas fa-shopping-cart fa-5x"></i>
                        <h2>Giỏ hàng của bạn đang trống</h2>
                        <p>Hãy thêm sản phẩm vào giỏ hàng để tiếp tục mua sắm</p>
                        <a href="/Product" class="btn-shop-now">
                            Khám phá sản phẩm ngay
                        </a>
                    </div>
                </div>
            `;
        }
    }

    // Hàm định dạng tiền tệ
    function formatCurrency(amount) {
        return new Intl.NumberFormat('vi-VN', {
            style: 'decimal',
            maximumFractionDigits: 0
        }).format(amount) + ' VND';
    }

    // Hàm cập nhật số lượng trên icon giỏ hàng
    function updateCartCount(count) {
        const cartCount = document.querySelector('.cart-count');
        if (cartCount) {
            cartCount.textContent = count;
        }
    }
});