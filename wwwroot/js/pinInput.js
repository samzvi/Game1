// JS interop helpers for the PIN input component.
export function attachPasteHandler(container) {
    // Block any key that isn't a digit or a control/navigation key, so letters
    // can never be typed into the boxes in the first place.
    container.addEventListener('keydown', function (e) {
        if (e.ctrlKey || e.metaKey || e.altKey) return;
        if (e.key.length > 1) return; // Backspace, Delete, ArrowLeft/Right, Tab, etc.
        if (!/^[0-9]$/.test(e.key)) {
            e.preventDefault();
        }
    });

    // Distributes pasted text across the individual digit boxes by dispatching
    // native 'input' events, so Blazor's existing @oninput handlers pick each
    // digit up automatically. Non-digit characters are stripped.
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
