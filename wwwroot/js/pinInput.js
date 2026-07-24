// JS interop helper for the PIN input component: distributes pasted text
// across the individual digit boxes by dispatching native 'input' events,
// so Blazor's existing @oninput handlers pick each digit up automatically.
export function attachPasteHandler(container) {
    container.addEventListener('paste', function (e) {
        e.preventDefault();
        var text = (e.clipboardData || window.clipboardData).getData('text');
        var digits = (text || '').replace(/\D/g, '');
        var inputs = container.querySelectorAll('.pin-digit');

        for (var i = 0; i < inputs.length; i++) {
            inputs[i].value = i < digits.length ? digits[i] : '';
            inputs[i].dispatchEvent(new Event('input', { bubbles: true }));
        }

        var focusIndex = digits.length > 0 ? Math.min(digits.length, inputs.length - 1) : 0;
        if (inputs[focusIndex]) {
            inputs[focusIndex].focus();
        }
    });
}
