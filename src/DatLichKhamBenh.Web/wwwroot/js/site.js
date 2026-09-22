// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Native <input type="date"> always displays in the browser/OS locale format (usually mm/dd/yyyy
// on this machine), which cannot be overridden via HTML/CSS. These fields are rendered as plain
// text inputs carrying a "flatpickr-date" class and an ISO (yyyy-MM-dd) value; Flatpickr shows the
// user a dd/mm/yyyy display while the underlying input keeps submitting the ISO value the server expects.
document.addEventListener('DOMContentLoaded', function () {
    if (typeof flatpickr === 'undefined') return;

    document.querySelectorAll('input.flatpickr-date').forEach(function (el) {
        flatpickr(el, {
            dateFormat: 'Y-m-d',
            altInput: true,
            altInputClass: el.className,
            altFormat: 'd/m/Y',
            allowInput: true,
            minDate: el.dataset.min || null,
            maxDate: el.dataset.max || null
        });
    });
});
