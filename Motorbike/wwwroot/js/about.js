// JavaScript tinh gọn cho trang Giới thiệu

document.addEventListener('DOMContentLoaded', function() {
    // Animation khi scroll
    function revealOnScroll() {
        var elements = document.querySelectorAll('.reveal-element');
        
        elements.forEach(function(element) {
            var windowHeight = window.innerHeight;
            var elementTop = element.getBoundingClientRect().top;
            var elementVisible = 150;
            
            if (elementTop < windowHeight - elementVisible) {
                // Thêm delay nếu có thuộc tính data-delay
                var delay = element.getAttribute('data-delay') || 0;
                setTimeout(function() {
                    element.classList.add('active');
                }, delay);
            }
        });
        
        // Activate fade-in-lines
        var fadeInLines = document.querySelectorAll('.fade-in-lines');
        fadeInLines.forEach(function(element) {
            var windowHeight = window.innerHeight;
            var elementTop = element.getBoundingClientRect().top;
            
            if (elementTop < windowHeight - 150) {
                element.classList.add('active');
            }
        });
    }
    
    // Thực thi khi scroll và khi trang load
    window.addEventListener('scroll', revealOnScroll);
    revealOnScroll();
    
    // Hiệu ứng typing text
    const typingText = document.getElementById('typingText');
    if (typingText) {
        // Ẩn ban đầu để tạo hiệu ứng tốt hơn
        typingText.style.visibility = 'hidden';
        
        // Thêm animation sau khi trang load
        setTimeout(function() {
            typingText.style.visibility = 'visible';
            // Reset animation
            typingText.style.animation = 'none';
            void typingText.offsetWidth; // Force reflow
            typingText.style.animation = 'typing 3.5s steps(40, end), blink-caret 0.75s step-end infinite';
        }, 500);
    }
    
    // Hiệu ứng highlight cho các accent
    const accentElements = document.querySelectorAll('.accent');
    accentElements.forEach(function(element) {
        element.addEventListener('mouseenter', function() {
            this.style.color = '#e63946';
        });
        
        element.addEventListener('mouseleave', function() {
            this.style.color = 'var(--secondary-color)';
        });
    });
    
    // Parallax effect for images on mouse move
    const aboutContainer = document.querySelector('.about-container');
    const aboutImages = document.querySelectorAll('.image-item');
    
    if (aboutContainer && aboutImages.length) {
        aboutContainer.addEventListener('mousemove', function(e) {
            const speed = 5;
            const x = (window.innerWidth - e.pageX * speed) / 100;
            const y = (window.innerHeight - e.pageY * speed) / 100;
            
            aboutImages.forEach(function(image, index) {
                const factor = (index + 1) * 0.2;
                image.style.transform = `translateX(${x * factor}px) translateY(${y * factor}px)`;
            });
        });
        
        aboutContainer.addEventListener('mouseleave', function() {
            aboutImages.forEach(function(image) {
                image.style.transform = 'translateX(0) translateY(0)';
            });
        });
    }
});