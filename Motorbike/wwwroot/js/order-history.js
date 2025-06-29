document.addEventListener('DOMContentLoaded', function() {
    // Hiệu ứng scroll to top mượt mà
    const scrollTop = document.createElement('div');
    scrollTop.classList.add('scroll-top-btn');
    scrollTop.innerHTML = '<i class="fas fa-arrow-up"></i>';
    document.body.appendChild(scrollTop);
    
    scrollTop.addEventListener('click', function() {
        window.scrollTo({
            top: 0,
            behavior: 'smooth'
        });
    });
    
    window.addEventListener('scroll', function() {
        if (window.pageYOffset > 300) {
            scrollTop.classList.add('active');
        } else {
            scrollTop.classList.remove('active');
        }
    });
    
    // Hiệu ứng số đếm trong bảng tổng cộng
    function animateValue(obj, start, end, duration) {
        let startTimestamp = null;
        const step = (timestamp) => {
            if (!startTimestamp) startTimestamp = timestamp;
            const progress = Math.min((timestamp - startTimestamp) / duration, 1);
            const value = Math.floor(progress * (end - start) + start);
            
            // Hiển thị với định dạng VND
            obj.textContent = new Intl.NumberFormat('vi-VN').format(value) + ' VND';
            if (progress < 1) {
                window.requestAnimationFrame(step);
            }
        };
        window.requestAnimationFrame(step);
    }
    
    // Khi trang được tải, kiểm tra nếu có phần tử tổng cộng
    const totalPriceElement = document.querySelector('.total-price');
    if (totalPriceElement) {
        // Lấy giá trị từ text (loại bỏ "VND" và dấu phẩy)
        const rawValue = totalPriceElement.textContent.replace(/[^\d]/g, '');
        const targetValue = parseInt(rawValue, 10);
        
        // Áp dụng hiệu ứng đếm
        setTimeout(() => {
            animateValue(totalPriceElement, 0, targetValue, 1000);
        }, 500);
    }
    
    // Hiệu ứng fade-in cho các phần tử khi scroll
    const fadeElements = document.querySelectorAll('.order-detail-card, .info-item');
    
    const fadeInOptions = {
        threshold: 0.1,
        rootMargin: "0px 0px -50px 0px"
    };
    
    const fadeInObserver = new IntersectionObserver(function(entries, observer) {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('fade-in');
                fadeInObserver.unobserve(entry.target);
            }
        });
    }, fadeInOptions);
    
    fadeElements.forEach(el => {
        el.classList.add('fade-element');
        fadeInObserver.observe(el);
    });
});