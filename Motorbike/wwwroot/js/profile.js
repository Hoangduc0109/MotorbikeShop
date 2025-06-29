document.addEventListener('DOMContentLoaded', function() {
    // Hiệu ứng cho đo lường mật khẩu mạnh/yếu
    const newPasswordInput = document.getElementById('newPassword');
    const passwordStrength = document.querySelector('.password-strength');
    const strengthBar = document.querySelector('.strength-bar');
    const strengthText = document.querySelector('.password-strength-text span');
    
    if (newPasswordInput && passwordStrength) {
        passwordStrength.style.display = 'block';
        
        newPasswordInput.addEventListener('input', function() {
            const password = this.value;
            let strength = 0;
            
            // Xóa tất cả các class độ mạnh trước đó
            passwordStrength.classList.remove('strength-weak', 'strength-medium', 'strength-strong', 'strength-very-strong');
            
            if (password.length === 0) {
                strengthText.textContent = 'Yếu';
                return;
            }
            
            // Tính điểm độ mạnh của mật khẩu
            if (password.length >= 6) strength += 1;
            if (password.length >= 10) strength += 1;
            if (/[A-Z]/.test(password)) strength += 1;
            if (/[0-9]/.test(password)) strength += 1;
            if (/[^A-Za-z0-9]/.test(password)) strength += 1;
            
            // Hiển thị độ mạnh
            switch (strength) {
                case 0:
                case 1:
                    passwordStrength.classList.add('strength-weak');
                    strengthText.textContent = 'Yếu';
                    break;
                case 2:
                    passwordStrength.classList.add('strength-medium');
                    strengthText.textContent = 'Trung bình';
                    break;
                case 3:
                case 4:
                    passwordStrength.classList.add('strength-strong');
                    strengthText.textContent = 'Mạnh';
                    break;
                case 5:
                    passwordStrength.classList.add('strength-very-strong');
                    strengthText.textContent = 'Rất mạnh';
                    break;
            }
        });
    }

    // Hiệu ứng highlight cho input fields
    const formControls = document.querySelectorAll('.form-control');
    if (formControls) {
        formControls.forEach(input => {
            if (!input.disabled) {
                input.addEventListener('focus', function() {
                    this.classList.add('input-focus');
                });
                
                input.addEventListener('blur', function() {
                    this.classList.remove('input-focus');
                });
            }
        });
    }

    // Thêm CSS cho hiệu ứng highlight
    const style = document.createElement('style');
    style.textContent = `
        .input-focus {
            box-shadow: 0 0 0 0.25rem rgba(13, 110, 253, 0.25);
            border-color: #86b7fe;
        }
        
        .strength-weak .strength-bar {
            width: 25%;
            background-color: #dc3545;
        }
        
        .strength-medium .strength-bar {
            width: 50%;
            background-color: #ffc107;
        }
        
        .strength-strong .strength-bar {
            width: 75%;
            background-color: #0dcaf0;
        }
        
        .strength-very-strong .strength-bar {
            width: 100%;
            background-color: #28a745;
        }
    `;
    document.head.appendChild(style);
    
    // Hiệu ứng tự đóng alert sau 5 giây
    const alerts = document.querySelectorAll('.alert');
    if (alerts) {
        alerts.forEach(alert => {
            setTimeout(() => {
                // Kiểm tra xem alert có phương thức close không
                if (typeof bootstrap !== 'undefined' && bootstrap.Alert) {
                    const bsAlert = new bootstrap.Alert(alert);
                    bsAlert.close();
                } else {
                    // Fallback nếu bootstrap không được tải
                    alert.style.opacity = '0';
                    setTimeout(() => {
                        if (alert.parentNode) {
                            alert.parentNode.removeChild(alert);
                        }
                    }, 500);
                }
            }, 5000);
        });
    }
});