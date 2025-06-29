/**
 * JavaScript cho trang sản phẩm BikeShop
 * Bao gồm hiệu ứng và chức năng cho cả danh sách và chi tiết sản phẩm
 */
document.addEventListener('DOMContentLoaded', function() {
    // ===== CHỨC NĂNG DÙNG CHUNG =====
    
    // Hiệu ứng animation khi scroll
    const productCards = document.querySelectorAll('.product-card');
    if (productCards.length > 0) {
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('show');
                }
            });
        }, {
            threshold: 0.1
        });

        productCards.forEach(card => {
            observer.observe(card);
        });
    }

    // Hiệu ứng sticky cho phần filter
    const filterSection = document.querySelector('.product-filters-section');
    if (filterSection) {
        window.addEventListener('scroll', function() {
            if (window.scrollY > 200) {
                filterSection.classList.add('sticky');
            } else {
                filterSection.classList.remove('sticky');
            }
        });
    }

    // Hiệu ứng hover 3D cho các product cards
    const cards = document.querySelectorAll('.product-card');
    if (cards.length > 0) {
        cards.forEach(card => {
            card.addEventListener('mousemove', function(e) {
                const rect = card.getBoundingClientRect();
                const x = e.clientX - rect.left;
                const y = e.clientY - rect.top;
                
                const centerX = rect.width / 2;
                const centerY = rect.height / 2;
                
                const rotateX = (y - centerY) / 15 * -1;
                const rotateY = (x - centerX) / 15;
                
                this.style.transform = `perspective(1000px) rotateX(${rotateX}deg) rotateY(${rotateY}deg) scale3d(1.05, 1.05, 1.05)`;
            });
            
            card.addEventListener('mouseleave', function() {
                this.style.transform = 'rotateX(0) rotateY(0) scale3d(1, 1, 1)';
            });
        });
    }

    // Nút xem thêm chi tiết với hiệu ứng ripple
    const viewDetailsBtns = document.querySelectorAll('.btn-view-details');
    if (viewDetailsBtns.length > 0) {
        viewDetailsBtns.forEach(btn => {
            btn.addEventListener('click', function(e) {
                // Tạo hiệu ứng ripple
                const ripple = document.createElement('span');
                const rect = this.getBoundingClientRect();
                const size = Math.max(rect.width, rect.height);
                const x = e.clientX - rect.left - (size / 2);
                const y = e.clientY - rect.top - (size / 2);
                
                ripple.style.width = ripple.style.height = `${size}px`;
                ripple.style.left = `${x}px`;
                ripple.style.top = `${y}px`;
                ripple.classList.add('ripple');
                
                this.appendChild(ripple);
                
                setTimeout(() => {
                    ripple.remove();
                }, 600);
            });
        });
    }
    
    // Xử lý khi thay đổi sắp xếp
    const sortOrderSelect = document.getElementById('sortOrder');
    if (sortOrderSelect) {
        sortOrderSelect.addEventListener('change', function() {
            const searchParams = new URLSearchParams(window.location.search);
            const brandId = searchParams.get('brandId') || 0;
            const searchTerm = searchParams.get('searchTerm') || '';
            
            let url = `${window.location.pathname}?sortOrder=${this.value}`;
            
            if (brandId && brandId != 0) {
                url += `&brandId=${brandId}`;
            }
            
            if (searchTerm) {
                url += `&searchTerm=${encodeURIComponent(searchTerm)}`;
            }
            
            window.location.href = url;
        });
    }

    // Cập nhật liên kết nút giỏ hàng trên header
    const cartBtn = document.querySelector('.cart-btn');
    if (cartBtn) {
        cartBtn.addEventListener('click', function() {
            window.location.href = '/Cart';
        });
    }

    // Xử lý thêm nhanh vào giỏ hàng từ danh sách sản phẩm
    document.addEventListener('click', function(e) {
        // Kiểm tra xem phần tử được click có class 'btn-add-to-cart-quick' không
        if (e.target.classList.contains('btn-add-to-cart-quick') || 
            e.target.closest('.btn-add-to-cart-quick')) {
            
            const button = e.target.classList.contains('btn-add-to-cart-quick') ? 
                          e.target : e.target.closest('.btn-add-to-cart-quick');
            
            const motorbikeId = button.getAttribute('data-id');
            const quantity = 1; // Mặc định thêm 1 sản phẩm
            
            console.log("Thêm sản phẩm ID:", motorbikeId); // Để debug
            
            // Thêm hiệu ứng khi nhấp vào nút
            button.classList.add('add-to-cart-animation');
            setTimeout(() => {
                button.classList.remove('add-to-cart-animation');
            }, 500);
            
            // Lấy token CSRF
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            if (!token) {
                console.error("CSRF token không tồn tại!");
                alert("Lỗi bảo mật: Thiếu token xác thực!");
                return;
            }
            
            // Gọi API để thêm vào giỏ hàng
            fetch('/Cart/AddToCart', {
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
                    throw new Error('Lỗi mạng hoặc server');
                }
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    // Hiển thị thông báo thành công
                    showAddToCartNotification();
                    
                    // Cập nhật số lượng giỏ hàng trên header
                    updateCartCount(data.totalItems);
                }
            })
            .catch(error => {
                console.error('Lỗi:', error);
                showAddToCartError(error);
            });
            
            // Ngăn sự kiện lan truyền
            e.preventDefault();
            e.stopPropagation();
        }
    });
    
    // Hàm hiển thị thông báo thêm vào giỏ hàng thành công
    function showAddToCartNotification() {
        // Tạo element thông báo
        const notification = document.createElement('div');
        notification.className = 'add-cart-notification';
        notification.innerHTML = `
            <div class="notification-content">
                <i class="fas fa-check-circle"></i>
                <p>Sản phẩm đã được thêm vào giỏ hàng</p>
            </div>
            <a href="/Cart" class="view-cart-link">Xem giỏ hàng</a>
        `;
        
        // Thêm vào body
        document.body.appendChild(notification);
        
        // Hiển thị notification với animation
        setTimeout(() => {
            notification.classList.add('show');
        }, 10);
        
        // Ẩn và xóa sau 3 giây
        setTimeout(() => {
            notification.classList.remove('show');
            setTimeout(() => {
                document.body.removeChild(notification);
            }, 300);
        }, 3000);
    }
    
    // Hàm hiển thị thông báo lỗi khi thêm vào giỏ hàng không thành công
    function showAddToCartError(error) {
        // Tạo element thông báo lỗi
        const notification = document.createElement('div');
        notification.className = 'add-cart-notification error';
        notification.innerHTML = `
            <div class="notification-content">
                <i class="fas fa-exclamation-circle"></i>
                <p>Không thể thêm sản phẩm vào giỏ hàng</p>
            </div>
            <small>${error.toString()}</small>
        `;
        
        // Thêm vào body
        document.body.appendChild(notification);
        
        // Hiển thị notification với animation
        setTimeout(() => {
            notification.classList.add('show');
        }, 10);
        
        // Ẩn và xóa sau 3 giây
        setTimeout(() => {
            notification.classList.remove('show');
            setTimeout(() => {
                document.body.removeChild(notification);
            }, 300);
        }, 3000);
    }
    
    // Hàm cập nhật số lượng trên icon giỏ hàng
    function updateCartCount(count) {
        const cartCount = document.querySelector('.cart-count');
        if (cartCount) {
            cartCount.textContent = count;
        }
    }
    
    // === Phần code hiện tại giữ nguyên ===
    
    // Xử lý tăng/giảm số lượng trong trang chi tiết sản phẩm
    const detailQuantity = document.querySelector('.detail-quantity');
    if (detailQuantity) {
        const decreaseBtn = detailQuantity.querySelector('.decrease');
        const increaseBtn = detailQuantity.querySelector('.increase');
        const input = detailQuantity.querySelector('.quantity-input');
        
        decreaseBtn.addEventListener('click', function() {
            let value = parseInt(input.value);
            if (value > 1) {
                input.value = value - 1;
            }
        });
        
        increaseBtn.addEventListener('click', function() {
            let value = parseInt(input.value);
            let max = 10; // Hoặc lấy từ stock của sản phẩm
            if (value < max) {
                input.value = value + 1;
            }
        });
        
        // Cho phép nhập số lượng trực tiếp
        input.addEventListener('change', function() {
            let value = parseInt(this.value);
            if (isNaN(value) || value < 1) {
                this.value = 1;
            } else if (value > 10) { // Giới hạn tối đa
                this.value = 10;
            }
        });
    }

    // Cải thiện xử lý nút thêm vào giỏ hàng từ trang chi tiết
    const addToCartBtn = document.querySelector('.btn-add-to-cart');
    if (addToCartBtn) {
        addToCartBtn.addEventListener('click', function() {
            const motorbikeId = this.getAttribute('data-id');
            const quantityInput = document.getElementById('quantity');
            const quantity = parseInt(quantityInput?.value || 1, 10);
            
            // Hiển thị hiệu ứng loading
            this.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang xử lý...';
            this.disabled = true;
            
            // Lấy token CSRF
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            if (!token) {
                console.error("CSRF token không tồn tại!");
                showAddToCartError(new Error("Lỗi bảo mật: Thiếu token xác thực!"));
                resetAddToCartButton();
                return;
            }
            
            fetch('/Cart/AddToCart', {
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
                    throw new Error('Lỗi mạng hoặc server');
                }
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    // Hiển thị thông báo thành công
                    if (typeof Swal !== 'undefined') {
                        Swal.fire({
                            position: 'center',
                            icon: 'success',
                            title: 'Đã thêm vào giỏ hàng',
                            text: `Đã thêm ${quantity} sản phẩm vào giỏ hàng`,
                            showConfirmButton: true,
                            confirmButtonText: 'Xem giỏ hàng',
                            showCancelButton: true,
                            cancelButtonText: 'Tiếp tục mua sắm'
                        }).then((result) => {
                            if (result.isConfirmed) {
                                window.location.href = '/Cart';
                            }
                        });
                    } else {
                        showAddToCartNotification();
                    }
                    
                    // Cập nhật số lượng giỏ hàng trên header
                    updateCartCount(data.totalItems);
                }
            })
            .catch(error => {
                console.error('Lỗi:', error);
                showAddToCartError(error);
            })
            .finally(() => {
                resetAddToCartButton();
            });
            
            function resetAddToCartButton() {
                addToCartBtn.innerHTML = '<i class="fas fa-shopping-cart"></i> Thêm vào giỏ hàng';
                addToCartBtn.disabled = false;
            }
        });
    }
});
