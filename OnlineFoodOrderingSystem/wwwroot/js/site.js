// Foodie JS Utilities

// Add to Cart
function addToCart(btn, foodId) {
    const originalText = btn.innerHTML;
    btn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Adding...';
    btn.disabled = true;

    $.post('/Cart/Add', { foodId: foodId }, function (res) {
        if (res.success) {
            updateCartCount(res.cartCount);
            showToast('Success', 'Item added to cart!', 'success');
        } else if (res.conflict) {
            $('#conflictRestaurantName').text(res.restaurantName);
            $('#cartConflictModal').modal('show');
            $('#confirmClearCart').off('click').on('click', function () {
                $('#cartConflictModal').modal('hide');
                $.post('/Cart/Clear', function () {
                    addToCart(btn, foodId); // try again
                });
            });
        } else if (res.redirect) {
            window.location.href = res.redirectUrl;
        } else {
            showToast('Error', res.message || 'Could not add item.', 'danger');
        }
    }).fail(function () {
        showToast('Error', 'Something went wrong.', 'danger');
    }).always(function () {
        btn.innerHTML = originalText;
        btn.disabled = false;
    });
}

function updateCartCount(count) {
    const badge = $('#cartCount');
    if (badge.length) {
        badge.text(count);
        badge.removeClass('d-none');
    }
}

// Notifications
function fetchNotificationCount() {
    if ($('#notifCount').length) {
        $.get('/Notification/Count', function (res) {
            if (res.count > 0) {
                $('#notifCount').text(res.count).removeClass('d-none');
            } else {
                $('#notifCount').addClass('d-none');
            }
        });
    }
}

// Dark Mode Toggle
$('#darkModeToggle').click(function () {
    $('body').toggleClass('dark-mode');
    const isDark = $('body').hasClass('dark-mode');
    document.cookie = "darkMode=" + isDark + ";path=/;max-age=31536000";
});

// Toast System
function showToast(title, message, type = 'info') {
    const id = 'toast_' + Date.now();
    let icon = 'bi-info-circle';
    if (type === 'success') icon = 'bi-check-circle-fill text-success';
    if (type === 'danger') icon = 'bi-exclamation-triangle-fill text-danger';

    const toastHtml = `
        <div id="${id}" class="toast align-items-center border-0 shadow-sm mb-2" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body d-flex align-items-center gap-2">
                    <i class="bi ${icon} fs-5"></i>
                    <div>
                        <strong class="d-block">${title}</strong>
                        <span class="small">${message}</span>
                    </div>
                </div>
                <button type="button" class="btn-close me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>
    `;
    $('#toastContainer').append(toastHtml);
    const toastElement = new bootstrap.Toast(document.getElementById(id), { delay: 3000 });
    toastElement.show();
    document.getElementById(id).addEventListener('hidden.bs.toast', function () {
        this.remove();
    });
}

// Admin Sidebar Toggle
$('#sidebarToggle').click(function () {
    $('#adminSidebar').toggleClass('show');
});

$(document).ready(function () {
    fetchNotificationCount();
    // Refresh notif count every minute
    setInterval(fetchNotificationCount, 60000);
});
