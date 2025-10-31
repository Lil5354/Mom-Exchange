function showNotification(type, message) {
    // Create notification element
    const notification = document.createElement('div');
    notification.className = `alert alert-${type === 'success' ? 'success' : 'danger'} alert-dismissible fade show`;
    notification.style.cssText = 'position: fixed; top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
    notification.innerHTML = `
        <i class="fa-solid fa-${type === 'success' ? 'check-circle' : 'exclamation-triangle'}"></i>
        ${message}
        <button type="button" class="btn-close" onclick="this.parentElement.remove()"></button>
    `;
    
    document.body.appendChild(notification);
    
    // Auto remove after 5 seconds
    setTimeout(() => {
        if (notification.parentElement) {
            notification.remove();
        }
    }, 5000);
}

function openAccountModal(url, modalId, onLoaded) {
    const containerId = 'accountModalContainer';
    let container = document.getElementById(containerId);
    if (!container) {
        container = document.createElement('div');
        container.id = containerId;
        document.body.appendChild(container);
    }
    container.innerHTML = '<div class="modal-overlay"><div class="modal-content-custom"><div class="modal-body-custom text-center"><i class="fas fa-spinner fa-spin fa-2x text-primary"></i><p class="mt-3">Đang tải...</p></div></div></div>';
    document.body.classList.add('modal-open');
    fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' } })
        .then(r => r.text())
        .then(html => { container.innerHTML = html; if (onLoaded) onLoaded(); })
        .catch(() => { 
            container.innerHTML = '<div class="modal-overlay"><div class="modal-content-custom"><div class="modal-body-custom text-center"><i class="fas fa-exclamation-triangle fa-2x text-danger"></i><p class="mt-3">Không thể tải nội dung.</p></div></div></div>'; 
        });
}

function openChangePasswordModal() {
    openAccountModal('/Account/ChangePassword', 'changePasswordModal');
}
function openForgotPasswordModal() {
    openAccountModal('/Account/ForgotPasswordRequest', 'forgotRequestModal');
}
function openVerifyResetModal() {
    openAccountModal('/Account/ForgotPasswordVerify', 'forgotVerifyModal');
}

function closeModal(id) {
    const el = document.getElementById(id);
    if (el) el.remove();
    const container = document.getElementById('accountModalContainer');
    if (container) container.innerHTML = '';
    document.body.classList.remove('modal-open');
}

function submitChangePassword(e) {
    e.preventDefault();
    const form = e.target;
    const submitBtn = form.querySelector('button[type="submit"]');
    const originalText = submitBtn.innerHTML;
    
    // Show loading state
    submitBtn.disabled = true;
    submitBtn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Đang cập nhật...';
    
    fetch(form.action, { method: 'POST', body: new FormData(form), headers: { 'X-Requested-With': 'XMLHttpRequest' } })
        .then(r => r.json())
        .then(res => {
            if (res.success) {
                showNotification('success', res.message);
                closeModal('changePasswordModal');
                // Clear form
                form.reset();
            } else {
                showNotification('error', res.message);
            }
        })
        .catch(() => showNotification('error', 'Có lỗi xảy ra. Vui lòng thử lại.'))
        .finally(() => {
            // Restore button state
            submitBtn.disabled = false;
            submitBtn.innerHTML = originalText;
        });
    return false;
}

function submitForgotRequest(e) {
    e.preventDefault();
    const form = e.target;
    const submitBtn = form.querySelector('button[type="submit"]');
    const originalText = submitBtn.innerHTML;
    
    // Show loading state
    submitBtn.disabled = true;
    submitBtn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Đang gửi...';
    
    fetch(form.action, { method: 'POST', body: new FormData(form), headers: { 'X-Requested-With': 'XMLHttpRequest' } })
        .then(r => r.json())
        .then(res => {
            if (res.success) {
                // Show success message
                showNotification('success', res.message);
                closeModal('forgotRequestModal');
                
                // Open verify modal with pre-filled email
                const email = form.querySelector('input[name="UsernameOrEmail"]').value;
                const nextUrl = res.next || ('/Account/ForgotPasswordVerify?u=' + encodeURIComponent(email));
                openAccountModal(nextUrl, 'forgotVerifyModal');
            } else {
                showNotification('error', res.message);
            }
        })
        .catch(() => showNotification('error', 'Có lỗi xảy ra. Vui lòng thử lại.'))
        .finally(() => {
            // Restore button state
            submitBtn.disabled = false;
            submitBtn.innerHTML = originalText;
        });
    return false;
}

function submitForgotVerify(e) {
    e.preventDefault();
    const form = e.target;
    const submitBtn = form.querySelector('button[type="submit"]');
    const originalText = submitBtn.innerHTML;
    const fd = new FormData(form);
    const usernameOrEmail = fd.get('UsernameOrEmail');
    
    // Show loading state
    submitBtn.disabled = true;
    submitBtn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Đang xác thực...';
    
    fetch(form.action, { method: 'POST', body: fd, headers: { 'X-Requested-With': 'XMLHttpRequest' } })
        .then(r => r.json())
        .then(res => {
            if (res.success) {
                showNotification('success', res.message);
                closeModal('forgotVerifyModal');
                const next = res.next || ('/Account/ResetPassword?u=' + encodeURIComponent(usernameOrEmail));
                openAccountModal(next, 'resetPasswordModal', function () {
                    const input = document.querySelector('#resetPasswordForm input[name="UsernameOrEmail"]');
                    if (input && !input.value) input.value = usernameOrEmail;
                });
            } else {
                showNotification('error', res.message);
            }
        })
        .catch(() => showNotification('error', 'Có lỗi xảy ra. Vui lòng thử lại.'))
        .finally(() => {
            // Restore button state
            submitBtn.disabled = false;
            submitBtn.innerHTML = originalText;
        });
    return false;
}

function submitResetPassword(e) {
    e.preventDefault();
    const form = e.target;
    const submitBtn = form.querySelector('button[type="submit"]');
    const originalText = submitBtn.innerHTML;
    
    // Show loading state
    submitBtn.disabled = true;
    submitBtn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Đang đặt lại...';
    
    fetch(form.action, { method: 'POST', body: new FormData(form), headers: { 'X-Requested-With': 'XMLHttpRequest' } })
        .then(r => r.json())
        .then(res => {
            if (res.success) {
                showNotification('success', res.message + ' Vui lòng đăng nhập với mật khẩu mới.');
                closeModal('resetPasswordModal');
                // Optionally redirect to login or show login modal
                setTimeout(() => {
                    window.location.href = '/Account/Login';
                }, 2000);
            } else {
                showNotification('error', res.message);
            }
        })
        .catch(() => showNotification('error', 'Có lỗi xảy ra. Vui lòng thử lại.'))
        .finally(() => {
            // Restore button state
            submitBtn.disabled = false;
            submitBtn.innerHTML = originalText;
        });
    return false;
}

// Function to resend verification code
function resendVerificationCode(email) {
    if (!email) {
        showNotification('error', 'Không tìm thấy thông tin email.');
        return;
    }
    
    const btn = event.target;
    const originalText = btn.innerHTML;
    
    btn.disabled = true;
    btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Đang gửi...';
    
    // Get the anti-forgery token from the current form
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    if (!token) {
        showNotification('error', 'Lỗi bảo mật. Vui lòng tải lại trang.');
        btn.disabled = false;
        btn.innerHTML = originalText;
        return;
    }
    
    fetch('/Account/ForgotPasswordRequest', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: `UsernameOrEmail=${encodeURIComponent(email)}&__RequestVerificationToken=${encodeURIComponent(token)}`
    })
    .then(r => r.json())
    .then(res => {
        if (res.success) {
            showNotification('success', 'Mã xác thực mới đã được gửi đến email của bạn.');
        } else {
            showNotification('error', res.message);
        }
    })
    .catch(() => showNotification('error', 'Có lỗi xảy ra khi gửi lại mã.'))
    .finally(() => {
        btn.disabled = false;
        btn.innerHTML = originalText;
    });
}


