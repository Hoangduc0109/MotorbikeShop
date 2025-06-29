// JavaScript nâng cao cho trang Home

$(document).ready(function() {
    // Thêm hiệu ứng cho sản phẩm nổi bật
    $('.product-card').each(function(index) {
        if (index < 3) { // 3 sản phẩm đầu tiên là nổi bật nhất
            $(this).find('.product-image-container').append('<span class="tag">Hot</span>');
        }
    });
    
    // Animation cho các phần tử khi scroll
    function showElementsOnScroll() {
        // Thêm class hidden cho các phần tử cần animation khi scroll
        $('.brand-item, .product-card, .section-heading, .contact-section h2, form, .map-container').addClass('hidden');
        
        // Function để kiểm tra và thêm animation khi scroll đến element
        function checkScroll() {
            $('.hidden').each(function() {
                const elementTop = $(this).offset().top;
                const windowBottom = $(window).scrollTop() + $(window).height();
                
                if (windowBottom > elementTop + 100) {
                    // Xác định delay dựa vào vị trí của element
                    const delay = $(this).index() * 150;
                    
                    setTimeout(() => {
                        $(this).addClass('show-element');
                    }, delay);
                }
            });
        }
        
        // Kiểm tra khi trang load và khi scroll
        checkScroll();
        $(window).scroll(checkScroll);
    }
    
    // Khởi động các animation
    setTimeout(showElementsOnScroll, 500);
    
    // Hover effect 3D cho product cards
    $('.product-card').on('mousemove', function(e) {
        const card = $(this);
        const cardWidth = card.outerWidth();
        const cardHeight = card.outerHeight();
        const centerX = card.offset().left + cardWidth / 2;
        const centerY = card.offset().top + cardHeight / 2;
        const mouseX = e.pageX - centerX;
        const mouseY = e.pageY - centerY;
        const rotateY = (mouseX / (cardWidth / 2)) * 5; // Giới hạn góc xoay 5 độ
        const rotateX = -((mouseY / (cardHeight / 2)) * 5); // Ngược dấu để có hiệu ứng đúng
        
        card.css('transform', `perspective(1000px) rotateX(${rotateX}deg) rotateY(${rotateY}deg) translateY(-15px)`);
    });
    
    $('.product-card').on('mouseleave', function() {
        $(this).css('transform', '');
    });
    
    // Smooth scroll cho các anchor links
    $('a[href^="#"]').on('click', function(e) {
        e.preventDefault();
        $('html, body').animate({
            scrollTop: $($(this).attr('href')).offset().top - 100
        }, 800);
    });
    
    // Form validation và hiệu ứng submit
    $('form').on('submit', function(e) {
        e.preventDefault();
        
        const isValid = validateForm($(this));
        
        if (isValid) {
            const btn = $(this).find('button[type="submit"]');
            const originalText = btn.html();
            
            btn.html('<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span> Đang gửi...');
            btn.prop('disabled', true);
            
            // Giả lập gửi form
            setTimeout(function() {
                btn.html('<i class="fas fa-check me-2"></i> Đã gửi thành công!');
                
                setTimeout(function() {
                    btn.html(originalText);
                    btn.prop('disabled', false);
                    $('form')[0].reset();
                }, 2000);
            }, 1500);
        }
    });
    
    // Hiệu ứng ripple cho buttons
    $('.btn').on('mousedown', function(e) {
        const button = $(this);
        const ripple = $('<span class="ripple-effect"></span>');
        
        const offsetX = e.pageX - button.offset().left;
        const offsetY = e.pageY - button.offset().top;
        
        ripple.css({
            top: offsetY + 'px',
            left: offsetX + 'px'
        }).appendTo(button);
        
        setTimeout(() => ripple.remove(), 600);
    });
    
    // Hiệu ứng loading cho images
    $('img').each(function() {
        const img = $(this);
        img.css('opacity', '0');
        
        img.on('load', function() {
            $(this).animate({opacity: 1}, 300);
        }).each(function() {
            if(this.complete) $(this).trigger('load');
        });
    });
    
    // Function kiểm tra form
    function validateForm(form) {
        let isValid = true;
        
        form.find('[required]').each(function() {
            if (!$(this).val()) {
                isValid = false;
                $(this).addClass('is-invalid');
                
                if (!$(this).next('.invalid-feedback').length) {
                    $(this).after('<div class="invalid-feedback">Vui lòng điền thông tin này</div>');
                }
            } else {
                $(this).removeClass('is-invalid');
            }
        });
        
        // Kiểm tra email
        const emailField = form.find('input[type="email"]');
        if (emailField.length && emailField.val()) {
            const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailPattern.test(emailField.val())) {
                isValid = false;
                emailField.addClass('is-invalid');
                
                if (!emailField.next('.invalid-feedback').length) {
                    emailField.after('<div class="invalid-feedback">Email không hợp lệ</div>');
                }
            }
        }
        
        return isValid;
    }
    
    // Listeners cho các input fields để loại bỏ thông báo lỗi khi người dùng sửa
    $('form').find('input, textarea').on('input', function() {
        $(this).removeClass('is-invalid');
    });
});

// Thêm CSS cho hiệu ứng ripple
document.addEventListener('DOMContentLoaded', function() {
    const style = document.createElement('style');
    style.innerHTML = `
        .btn {
            position: relative;
            overflow: hidden;
        }
        
        .ripple-effect {
            position: absolute;
            border-radius: 50%;
            background-color: rgba(255, 255, 255, 0.5);
            width: 100px;
            height: 100px;
            margin-top: -50px;
            margin-left: -50px;
            animation: ripple 0.6s;
            opacity: 0;
        }
        
        @keyframes ripple {
            0% {
                transform: scale(0);
                opacity: 1;
            }
            100% {
                transform: scale(3);
                opacity: 0;
            }
        }
    `;
    document.head.appendChild(style);
});